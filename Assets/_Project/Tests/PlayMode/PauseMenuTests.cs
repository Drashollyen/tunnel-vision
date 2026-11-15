using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Builds a pause menu around the real shared OptionsPanel prefab in a scene of its own.
    /// </summary>
    public class PauseMenuTests
    {
        private const string OptionsPrefab = "Assets/_Project/Prefabs/UI/OptionsPanel.prefab";

        private PauseMenu _menu;
        private GameObject _overlay;
        private GameObject _pausePanel;
        private OptionsMenu _options;
        private Button _resume;
        private Button _levelSelect;
        private GameObject _confirm;
        private TMP_Text _question;
        private Button _leave;
        private Button _stay;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("PauseMenuTest");
            Time.timeScale = 1f;

            new GameObject("EventSystem", typeof(EventSystem));
            var canvas = new GameObject("HUD", typeof(Canvas)).transform;

            var holder = new GameObject("Pause Menu", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            holder.SetActive(false); // So Awake waits until the fields are filled in.

            _overlay = new GameObject("Overlay", typeof(RectTransform), typeof(Image));
            _overlay.transform.SetParent(holder.transform, false);
            _pausePanel = new GameObject("Pause Panel", typeof(RectTransform));
            _pausePanel.transform.SetParent(_overlay.transform, false);
            _resume = MakeButton("Resume Button");
            var optionsButton = MakeButton("Options Button");
            _levelSelect = MakeButton("Level Select Button");

            _confirm = new GameObject("Confirm Panel", typeof(RectTransform));
            _confirm.transform.SetParent(_overlay.transform, false);
            _question = new GameObject("Title", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            _question.transform.SetParent(_confirm.transform, false);
            _leave = MakeButton("Leave Button", _confirm.transform);
            _stay = MakeButton("Stay Button", _confirm.transform);

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(OptionsPrefab);
            Assert.IsNotNull(prefab, "The shared options page should exist.");
            _options = Object.Instantiate(prefab, _overlay.transform).GetComponent<OptionsMenu>();

            _menu = holder.AddComponent<PauseMenu>();
            Set("overlay", _overlay);
            Set("pausePanel", _pausePanel);
            Set("optionsPanel", _options);
            Set("resumeButton", _resume);
            Set("optionsButton", optionsButton);
            Set("levelSelectButton", _levelSelect);
            Set("levelSelectScene", "NoSuchSceneForTests"); // SceneFader only warns, so no scene change.
            Set("confirmPanel", _confirm);
            Set("confirmTitle", _question);
            Set("leaveButton", _leave);
            Set("stayButton", _stay);
            holder.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            Time.timeScale = 1f;
            IsolatedScene.Close();
        }

        private Button MakeButton(string name, Transform parent = null)
        {
            var button = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
            button.transform.SetParent(parent != null ? parent : _pausePanel.transform, false);
            return button;
        }

        private void Set(string field, object value)
        {
            typeof(PauseMenu).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_menu, value);
        }

        [UnityTest]
        public IEnumerator StartsHidden_AndOptionsPageIsShared()
        {
            Assert.IsFalse(_menu.IsPaused);
            Assert.IsFalse(_overlay.activeSelf);
            Assert.IsFalse(_options.gameObject.activeSelf, "The shared options page should start hidden.");
            yield break;
        }

        [UnityTest]
        public IEnumerator Escape_PausesAndShowsThePausePage()
        {
            _menu.PressEscape();
            yield return null;

            Assert.IsTrue(_menu.IsPaused);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.IsTrue(_overlay.activeSelf);
            Assert.IsTrue(_pausePanel.activeSelf);
            Assert.AreEqual(_resume.gameObject, EventSystem.current.currentSelectedGameObject);
        }

        [UnityTest]
        public IEnumerator EscapeAgain_Resumes()
        {
            _menu.PressEscape();
            yield return null;
            _menu.PressEscape();

            Assert.IsFalse(_menu.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(_overlay.activeSelf);
        }

        [UnityTest]
        public IEnumerator ResumeButton_Resumes()
        {
            _menu.Pause();
            yield return null;
            _resume.onClick.Invoke();

            Assert.IsFalse(_menu.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Options_OpensTheSharedPage_AndItsBackButtonReturns()
        {
            _menu.Pause();
            yield return null;

            _menu.OpenOptions();
            yield return new WaitForSecondsRealtime(0.4f); // The cross-fade, on unscaled time.
            Assert.IsTrue(_menu.IsOnOptions);
            Assert.IsFalse(_pausePanel.activeSelf);

            _options.PressBack();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.IsFalse(_menu.IsOnOptions);
            Assert.IsTrue(_pausePanel.activeSelf);
            Assert.IsTrue(_menu.IsPaused, "Going back from Options shouldn't resume.");
        }

        [UnityTest]
        public IEnumerator EscapeOnOptions_GoesBackInsteadOfResuming()
        {
            _menu.Pause();
            yield return null;
            _menu.OpenOptions();
            yield return new WaitForSecondsRealtime(0.4f);

            _menu.PressEscape();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsTrue(_menu.IsPaused);
            Assert.IsFalse(_menu.IsOnOptions);
        }

        [UnityTest]
        public IEnumerator WontPause_WhenSomethingElseHasPausedTheGame()
        {
            // Out Of Lives pauses the game itself and owns the screen.
            Time.timeScale = 0f;
            _menu.PressEscape();
            yield return null;

            Assert.IsFalse(_menu.IsPaused);
            Assert.IsFalse(_overlay.activeSelf);
        }


        [UnityTest]
        public IEnumerator LevelSelect_AsksFirst_WithStaySelected()
        {
            _menu.Pause();
            yield return null;

            _levelSelect.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsTrue(_menu.IsConfirmingLeave);
            Assert.IsTrue(_menu.IsPaused, "Asking shouldn't leave or resume yet.");
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(PauseMenu.LevelSelectQuestion, _question.text);
            Assert.AreEqual(_stay.gameObject, EventSystem.current.currentSelectedGameObject,
                "Stay should be selected so a stray Enter is safe.");
        }

        [UnityTest]
        public IEnumerator Stay_GoesBackToThePausePage()
        {
            _menu.Pause();
            yield return null;
            _levelSelect.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            _stay.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsFalse(_menu.IsConfirmingLeave);
            Assert.IsTrue(_pausePanel.activeSelf);
            Assert.IsTrue(_menu.IsPaused);
        }

        [UnityTest]
        public IEnumerator EscapeOnTheQuestion_Stays()
        {
            _menu.Pause();
            yield return null;
            _levelSelect.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            _menu.PressEscape();
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsFalse(_menu.IsConfirmingLeave);
            Assert.IsTrue(_menu.IsPaused);
        }

        [UnityTest]
        public IEnumerator Leave_UnpausesAndLeaves()
        {
            _menu.Pause();
            yield return null;
            _levelSelect.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.4f);

            _leave.onClick.Invoke();

            Assert.IsFalse(_menu.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
