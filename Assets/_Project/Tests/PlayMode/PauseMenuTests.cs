using System.Collections;
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
        private Button _restart;
        private TMP_Text _leaveLabel;

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
            _restart = MakeButton("Restart Button");
            var optionsButton = MakeButton("Options Button");
            _levelSelect = MakeButton("Level Select Button");

            _confirm = new GameObject("Confirm Panel", typeof(RectTransform));
            _confirm.transform.SetParent(_overlay.transform, false);
            _question = new GameObject("Title", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            _question.transform.SetParent(_confirm.transform, false);
            _leave = MakeButton("Leave Button", _confirm.transform);
            _stay = MakeButton("Stay Button", _confirm.transform);
            _leaveLabel = new GameObject("Leave Text", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            _leaveLabel.transform.SetParent(_leave.transform, false);

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
            Set("restartButton", _restart);
            Set("leaveButtonLabel", _leaveLabel);
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

        // Waits for the page cross-fade to finish, rather than a fixed time.
        private IEnumerator WaitForPanelSwitch()
        {
            var switcher = _menu.GetComponent<PanelSwitcher>();
            yield return null;
            yield return new WaitWhile(() => switcher != null && switcher.IsSwitching);
        }

        private void Set(string field, object value)
        {
            Private.Set(_menu, field, value);
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
            Assert.IsNull(EventSystem.current.currentSelectedGameObject,
                "Mouse-only, like the main menu: nothing should be highlighted on its own.");
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
            yield return WaitForPanelSwitch();
            Assert.IsTrue(_menu.IsOnOptions);
            Assert.IsFalse(_pausePanel.activeSelf);

            _options.PressBack();
            yield return WaitForPanelSwitch();
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
            yield return WaitForPanelSwitch();

            _menu.PressEscape();
            yield return WaitForPanelSwitch();

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
        public IEnumerator LevelSelect_AsksFirst()
        {
            _menu.Pause();
            yield return null;

            _levelSelect.onClick.Invoke();
            yield return WaitForPanelSwitch();

            Assert.IsTrue(_menu.IsConfirmingLeave);
            Assert.IsTrue(_menu.IsPaused, "Asking shouldn't leave or resume yet.");
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(PauseMenu.LevelSelectQuestion, _question.text);
            Assert.IsNull(EventSystem.current.currentSelectedGameObject,
                "Nothing should stay highlighted from the button that was clicked.");
        }

        [UnityTest]
        public IEnumerator Restart_WithNoCheckpoint_AsksToRestartTheLevel()
        {
            _menu.Pause();
            yield return null;

            _restart.onClick.Invoke();
            yield return WaitForPanelSwitch();

            Assert.IsTrue(_menu.IsConfirmingLeave, "With no checkpoint, it should ask before restarting.");
            Assert.AreEqual(PauseMenu.RestartQuestion, _question.text);
            Assert.AreEqual(PauseMenu.RestartLabel, _leaveLabel.text);
        }

        [UnityTest]
        public IEnumerator LevelSelect_AfterRestartQuestion_SaysLeaveAgain()
        {
            _menu.Pause();
            yield return null;

            _restart.onClick.Invoke();
            yield return WaitForPanelSwitch();
            _stay.onClick.Invoke();
            yield return WaitForPanelSwitch();

            _levelSelect.onClick.Invoke();
            yield return WaitForPanelSwitch();
            Assert.AreEqual(PauseMenu.LeaveLabel, _leaveLabel.text);
        }

        [UnityTest]
        public IEnumerator Stay_GoesBackToThePausePage()
        {
            _menu.Pause();
            yield return null;
            _levelSelect.onClick.Invoke();
            yield return WaitForPanelSwitch();

            _stay.onClick.Invoke();
            yield return WaitForPanelSwitch();

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
            yield return WaitForPanelSwitch();

            _menu.PressEscape();
            yield return WaitForPanelSwitch();

            Assert.IsFalse(_menu.IsConfirmingLeave);
            Assert.IsTrue(_menu.IsPaused);
        }

        [UnityTest]
        public IEnumerator Leave_UnpausesAndLeaves()
        {
            _menu.Pause();
            yield return null;
            _levelSelect.onClick.Invoke();
            yield return WaitForPanelSwitch();

            _leave.onClick.Invoke();

            Assert.IsFalse(_menu.IsPaused);
            Assert.AreEqual(1f, Time.timeScale);
        }


        [UnityTest]
        public IEnumerator Pausing_QuietsGameSounds_AndResumingBringsThemBack()
        {
            _menu.Pause();
            yield return new WaitForSecondsRealtime(PauseMenu.SoundFadeSeconds + 0.15f);
            Assert.IsTrue(Audio.SoundEffectsVolume.IsGameplaySilenced);

            _menu.Resume();
            yield return new WaitForSecondsRealtime(PauseMenu.SoundFadeSeconds + 0.15f);
            Assert.AreEqual(1f, Audio.SoundEffectsVolume.GameplayLevel, 0.001f);
        }
    }
}
