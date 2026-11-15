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
        private TMP_Text _title;
        private TMP_Text _stats;
        private Button _next;
        private Button _select;
        private bool _hadProgress;
        private int _savedProgress;

        [UnitySetUp]
        public IEnumerator Build()
        {
            _hadProgress = PlayerPrefs.HasKey(ProgressKey);
            _savedProgress = PlayerPrefs.GetInt(ProgressKey, 0);
            PlayerPrefs.SetInt(ProgressKey, 0);

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
            _title = MakeText("Title");
            _stats = MakeText("Stats");
            _next = MakeButton("Next Level Button");
            _select = MakeButton("Level Select Button");

            _screen = holder.AddComponent<LevelCompleteScreen>();
            Set(_screen, "overlay", _overlay);
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
        }

        private TMP_Text MakeText(string name)
        {
            var text = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(_overlay.transform, false);
            return text;
        }

        private Button MakeButton(string name)
        {
            var button = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
            button.transform.SetParent(_overlay.transform, false);
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
            Assert.AreEqual(0f, Time.timeScale, "The game should freeze.");
            Assert.AreEqual(LevelCompleteScreen.Title, _title.text);
            StringAssert.Contains("Time: ", _stats.text);
            StringAssert.Contains("Collectibles: 0", _stats.text);
            Assert.IsNull(EventSystem.current.currentSelectedGameObject, "Mouse-only, like the other menus.");
        }

        [UnityTest]
        public IEnumerator Screen_FadesIn()
        {
            _goal.Reach();
            yield return null;
            var group = _overlay.GetComponent<CanvasGroup>();
            Assert.Less(group.alpha, 1f);

            yield return new WaitForSecondsRealtime(LevelCompleteScreen.FadeInSeconds + 0.1f);
            Assert.AreEqual(1f, group.alpha, 0.001f);
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
    }
}
