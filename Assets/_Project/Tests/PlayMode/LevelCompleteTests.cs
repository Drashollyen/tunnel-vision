using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.Level;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>A goal and the Level Complete screen, built in a scene of their own.</summary>
    public class LevelCompleteTests
    {
        private const string ProgressKey = "HighestLevelUnlocked";

        private LevelGoal _goal;
        private LevelCompleteScreen _screen;
        private GameObject _overlay;
        private GameObject _panel;
        private TMP_Text _title;
        private TMP_Text _stats;
        private Button _next;
        private Button _select;
        private bool _hadProgress;
        private int _savedProgress;
        private bool _hadTeddies;
        private int _savedTeddies;

        [UnitySetUp]
        public IEnumerator Build()
        {
            _hadProgress = PlayerPrefs.HasKey(ProgressKey);
            _savedProgress = PlayerPrefs.GetInt(ProgressKey, 0);
            PlayerPrefs.SetInt(ProgressKey, 0);
            _hadTeddies = PlayerPrefs.HasKey(LevelProgress.CollectiblesKey(0));
            _savedTeddies = PlayerPrefs.GetInt(LevelProgress.CollectiblesKey(0), 0);
            PlayerPrefs.DeleteKey(LevelProgress.CollectiblesKey(0));

            yield return IsolatedScene.Create("LevelCompleteTest");
            Time.timeScale = 1f;

            new GameObject("EventSystem", typeof(EventSystem));
            var player = new GameObject("Player");
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<PlayerLives>();
            player.AddComponent<PlayerCollectibles>();
            player.AddComponent<PlayerRespawn>();

            var canvas = new GameObject("HUD", typeof(Canvas)).transform;
            var holder = new GameObject("Level Complete", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            holder.SetActive(false); // So Awake waits until the fields are filled in.
            _overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            _overlay.transform.SetParent(holder.transform, false);
            _overlay.GetComponent<Image>().color = Color.black;
            _panel = new GameObject("Level Complete Panel", typeof(RectTransform));
            _panel.transform.SetParent(_overlay.transform, false);
            _title = MakeText("Title");
            _stats = MakeText("Stats");
            _next = MakeButton("Next Level Button");
            _select = MakeButton("Level Select Button");

            _screen = holder.AddComponent<LevelCompleteScreen>();
            Set(_screen, "overlay", _overlay);
            Set(_screen, "panel", _panel);
            Set(_screen, "title", _title);
            Set(_screen, "stats", _stats);
            Set(_screen, "nextLevelButton", _next);
            Set(_screen, "levelSelectButton", _select);
            Set(_screen, "levelSelectScene", "NoSuchSceneForTests"); // SceneFader only warns.
            holder.SetActive(true);

            _goal = new GameObject("Goal").AddComponent<LevelGoal>();
            Set(_goal, "levelIndex", 0);
            Set(_goal, "nextLevelScene", "Level2");
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            Time.timeScale = 1f;
            IsolatedScene.Close();
            if (_hadProgress)
                PlayerPrefs.SetInt(ProgressKey, _savedProgress);
            else
                PlayerPrefs.DeleteKey(ProgressKey);
            if (_hadTeddies)
                PlayerPrefs.SetInt(LevelProgress.CollectiblesKey(0), _savedTeddies);
            else
                PlayerPrefs.DeleteKey(LevelProgress.CollectiblesKey(0));
        }

        private TMP_Text MakeText(string name)
        {
            var text = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(_panel.transform, false);
            return text;
        }

        private Button MakeButton(string name)
        {
            var button = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
            button.transform.SetParent(_panel.transform, false);
            return button;
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        [UnityTest]
        public IEnumerator StartsHidden()
        {
            Assert.IsFalse(_screen.IsShowing);
            Assert.IsFalse(_overlay.activeSelf);
            yield break;
        }

        [UnityTest]
        public IEnumerator ReachingTheGoal_SavesProgressAndShowsTheScreen()
        {
            _goal.Reach();
            yield return null;

            Assert.AreEqual(1, LevelProgress.HighestUnlocked, "The next level should unlock.");
            Assert.IsTrue(_screen.IsShowing);
            Assert.IsTrue(LevelCompleteScreen.IsAnyShowing, "The pause menu checks this to stay shut.");
            Assert.IsTrue(_overlay.activeSelf);
            Assert.AreEqual(1f, Time.timeScale, "The game runs on while Nicholas walks off and the screen fades.");
            Assert.AreEqual(LevelCompleteScreen.Title, _title.text);
            StringAssert.Contains("Time: ", _stats.text);
            StringAssert.Contains("Collectibles: 0", _stats.text);
            Assert.IsNull(EventSystem.current.currentSelectedGameObject, "Mouse-only, like the other menus.");
        }

        [UnityTest]
        public IEnumerator Screen_GoesBlackThenThePanelFadesIn()
        {
            _goal.Reach();
            yield return null;
            Assert.Less(_screen.Blackness, 1f, "The screen should fade to black, not cut.");
            Assert.AreEqual(0f, _screen.PanelVisibility, 0.001f, "The panel waits for the black.");
            Assert.IsFalse(_panel.GetComponent<CanvasGroup>().blocksRaycasts, "Hidden buttons can't be clicked.");

            yield return new WaitForSecondsRealtime(LevelCompleteScreen.BlackFadeSeconds + 0.05f);
            Assert.AreEqual(1f, _screen.Blackness, 0.001f, "Fully black once crossed.");

            yield return new WaitForSecondsRealtime(LevelCompleteScreen.FadeInSeconds + 0.1f);
            Assert.AreEqual(1f, _screen.PanelVisibility, 0.001f);
            Assert.IsTrue(_panel.GetComponent<CanvasGroup>().blocksRaycasts);
        }

        [UnityTest]
        public IEnumerator NextLevel_ShowsWhenThereIsANextLevel()
        {
            _goal.Reach();
            yield return null;

            Assert.IsTrue(_next.gameObject.activeSelf);
            Assert.AreEqual("Level2", _screen.NextScene);
            Assert.IsTrue(_select.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator NextLevel_IsHiddenOnTheLastLevel()
        {
            Set(_goal, "nextLevelScene", "");
            _goal.Reach();
            yield return null;

            Assert.IsFalse(_next.gameObject.activeSelf);
            Assert.IsNull(_screen.NextScene);
            Assert.IsTrue(_select.gameObject.activeSelf, "Level Select is always there.");
        }

        [UnityTest]
        public IEnumerator NextLevel_IsHiddenForASceneThatIsNotInTheBuild()
        {
            Set(_goal, "nextLevelScene", "NoSuchSceneForTests");
            _goal.Reach();
            yield return null;

            Assert.IsFalse(_next.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator Goal_OnlyFiresOnce()
        {
            _goal.Reach();
            yield return null;
            var firstStats = _stats.text;

            yield return new WaitForSecondsRealtime(1.1f);
            _goal.Reach();
            yield return null;

            Assert.IsTrue(_goal.Reached);
            Assert.AreEqual(firstStats, _stats.text, "A second touch shouldn't show it again.");
        }


        [UnityTest]
        public IEnumerator Game_FreezesOnceTheScreenIsBlack()
        {
            _goal.Reach();
            yield return new WaitForSecondsRealtime(LevelCompleteScreen.BlackFadeSeconds * 0.5f);
            Assert.AreEqual(1f, Time.timeScale, "Still running partway through the fade.");

            yield return new WaitForSecondsRealtime(LevelCompleteScreen.BlackFadeSeconds * 0.5f + 0.1f);
            Assert.AreEqual(0f, Time.timeScale, "Frozen once black.");
        }

        [UnityTest]
        public IEnumerator NothingCanKillThePlayer_AfterTheGoal()
        {
            _goal.Reach();
            yield return null;

            var respawn = Object.FindAnyObjectByType<PlayerRespawn>();
            respawn.Kill();

            Assert.IsFalse(respawn.IsRespawning, "A hazard during the walk-off shouldn't start a death.");
        }


        [UnityTest]
        public IEnumerator ClearingTheLevel_SavesTheTeddiesFound()
        {
            var collectibles = Object.FindAnyObjectByType<PlayerCollectibles>();
            collectibles.Add();
            collectibles.Add();
            Assert.AreEqual(0, LevelProgress.BestCollectibles(0), "Nothing is saved before the level is cleared.");

            _goal.Reach();
            yield return null;

            Assert.AreEqual(2, LevelProgress.BestCollectibles(0));
            StringAssert.Contains("Collectibles: 2", _stats.text);
        }


        private TMP_Text LabelNextButton()
        {
            var label = new GameObject("Text (TMP)", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            label.transform.SetParent(_next.transform, false);
            label.text = LevelCompleteScreen.NextLevelLabel;
            return label;
        }

        [UnityTest]
        public IEnumerator LastLevel_OffersOnlyContinue()
        {
            var label = LabelNextButton();
            Set(_goal, "endsDemo", true);
            Set(_goal, "demoEndScene", "Level2"); // Any scene in the build stands in for the ending.
            _goal.Reach();
            yield return null;

            Assert.IsTrue(_screen.EndsDemo);
            Assert.IsTrue(_next.gameObject.activeSelf);
            Assert.AreEqual(LevelCompleteScreen.ContinueLabel, label.text);
            Assert.IsFalse(_select.gameObject.activeSelf, "Continue is the only way on from the last level.");
            Assert.AreEqual("Level2", _screen.NextScene);
        }

        [UnityTest]
        public IEnumerator LastLevel_WithoutTheEnding_StillOffersLevelSelect()
        {
            Set(_goal, "endsDemo", true);
            Set(_goal, "demoEndScene", "NoSuchSceneForTests");
            _goal.Reach();
            yield return null;

            Assert.IsFalse(_next.gameObject.activeSelf);
            Assert.IsTrue(_select.gameObject.activeSelf, "The player must never be stuck.");
        }

        [UnityTest]
        public IEnumerator OtherLevels_SayNextLevel()
        {
            var label = LabelNextButton();
            _goal.Reach();
            yield return null;

            Assert.IsFalse(_screen.EndsDemo);
            Assert.AreEqual(LevelCompleteScreen.NextLevelLabel, label.text);
            Assert.IsTrue(_select.gameObject.activeSelf);
        }
    }
}
