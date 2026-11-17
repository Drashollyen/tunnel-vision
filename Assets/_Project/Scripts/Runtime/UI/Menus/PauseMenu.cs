using TMPro;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Escape pauses the level and shows Resume, Options, Level Select and Main Menu. Leaving asks
    // "are you sure?" first, because the level's progress is lost. Won't open while another screen
    // has taken over (out of lives, dying, level complete, or a scene fade).
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject overlay;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private OptionsMenu optionsPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [SerializeField] private string mainMenuScene = "IntroScreen";

        [Header("Leave Confirmation")]
        [SerializeField] private GameObject confirmPanel;
        [Tooltip("Optional. Shows where the player is about to go.")]
        [SerializeField] private TMP_Text confirmTitle;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button stayButton;

        public const string LevelSelectQuestion = "Go To Level Select?";
        public const string MainMenuQuestion = "Go To The Main Menu?";
        public const string ProgressWarning = "Any Progress In This Level Will Be Lost";

        // How quickly game sounds (wheels, platforms) go quiet on pause and come back on resume.
        public const float SoundFadeSeconds = 0.15f;

        private PanelSwitcher _panels;
        private PlayerMovement _movement;
        private PlayerRespawn _respawn;
        private RoundOverMessage _roundOver;
        private string _leavingTo;

        public bool IsPaused { get; private set; }

        public bool IsOnOptions => optionsPanel != null && optionsPanel.gameObject.activeSelf;

        public bool IsConfirmingLeave => confirmPanel != null && confirmPanel.activeSelf;

        // False while another screen has taken over, so the game can't be paused.
        public bool CanPause =>
            !SceneFader.IsFading
            && (_roundOver == null || !_roundOver.IsGameOver)
            && !LevelCompleteScreen.IsAnyShowing
            && (_respawn == null || !_respawn.IsRespawning)
            && Time.timeScale > 0f;

        private void Awake()
        {
            _panels = gameObject.AddComponent<PanelSwitcher>();
            _movement = FindAnyObjectByType<PlayerMovement>();
            _respawn = FindAnyObjectByType<PlayerRespawn>();
            _roundOver = FindAnyObjectByType<RoundOverMessage>(FindObjectsInactive.Include);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);
            if (optionsButton != null)
                optionsButton.onClick.AddListener(OpenOptions);
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(() => AskToLeave(levelSelectScene, LevelSelectQuestion));
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => AskToLeave(mainMenuScene, MainMenuQuestion));
            if (leaveButton != null)
                leaveButton.onClick.AddListener(ConfirmLeave);
            if (stayButton != null)
                stayButton.onClick.AddListener(CancelLeave);
            if (optionsPanel != null)
                optionsPanel.BackPressed += CloseOptions;

            overlay.SetActive(false);
            if (confirmPanel != null)
                confirmPanel.SetActive(false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !_panels.IsSwitching)
            {
                PressEscape();
            }
        }

        // Escape pauses, goes back from Options or the "are you sure?" question, or resumes.
        public void PressEscape()
        {
            if (!IsPaused)
            {
                Pause();
            }
            else if (IsOnOptions)
            {
                CloseOptions();
            }
            else if (IsConfirmingLeave)
            {
                CancelLeave();
            }
            else
            {
                Resume();
            }
        }

        public void Pause()
        {
            if (IsPaused || !CanPause)
            {
                return;
            }

            IsPaused = true;
            Time.timeScale = 0f;
            SoundEffectsVolume.FadeOutGameplay(SoundFadeSeconds);
            if (_movement != null)
                _movement.EnableControls(false);

            overlay.SetActive(true);
            pausePanel.SetActive(true);
            if (optionsPanel != null)
                optionsPanel.gameObject.SetActive(false);
            if (confirmPanel != null)
                confirmPanel.SetActive(false);
            ClearSelection();
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
            SoundEffectsVolume.ResumeGameplay(SoundFadeSeconds);
            if (_movement != null)
                _movement.EnableControls(true);
            overlay.SetActive(false);
        }

        public void OpenOptions()
        {
            if (IsPaused && optionsPanel != null && !IsOnOptions)
            {
                _panels.Switch(pausePanel, optionsPanel.gameObject);
            }
        }

        public void CloseOptions()
        {
            if (IsPaused && IsOnOptions)
            {
                _panels.Switch(optionsPanel.gameObject, pausePanel);
                ClearSelection();
            }
        }

        // Asks before leaving the level. If there's no confirmation panel, it just leaves.
        public void AskToLeave(string scene, string question)
        {
            if (!IsPaused || IsConfirmingLeave)
            {
                return;
            }

            if (confirmPanel == null)
            {
                Leave(scene);
                return;
            }

            _leavingTo = scene;
            if (confirmTitle != null)
                confirmTitle.text = question;
            _panels.Switch(pausePanel, confirmPanel);
            ClearSelection();
        }

        public void ConfirmLeave()
        {
            if (IsConfirmingLeave && _leavingTo != null)
            {
                Leave(_leavingTo);
            }
        }

        public void CancelLeave()
        {
            if (!IsConfirmingLeave)
            {
                return;
            }

            _leavingTo = null;
            _panels.Switch(confirmPanel, pausePanel);
            ClearSelection();
        }

        private void Leave(string scene)
        {
            IsPaused = false;
            Time.timeScale = 1f;
            SceneFader.Load(scene);
        }

        private void OnDestroy()
        {
            // Never leave the game paused if the menu is removed some other way.
            if (IsPaused)
            {
                Time.timeScale = 1f;
            }
        }

        // A clicked button stays highlighted, and that highlight would carry over to the next page.
        private static void ClearSelection()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
