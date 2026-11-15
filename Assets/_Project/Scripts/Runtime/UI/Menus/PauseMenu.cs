using TMPro;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Escape pauses a level and shows Resume, Options, Level Select and Main Menu. Options is the
    /// same page (the OptionsPanel prefab) as on the main menu. Escape on the options page goes
    /// back, and on the pause page resumes. It won't open on Out Of Lives, during the death
    /// sequence, or while a scene fades in or out. Level Select and Main Menu ask first, since
    /// leaving loses the level's progress.
    /// Goes on an always-on object. The overlay (a full-screen dim image holding both pages)
    /// is a child that starts hidden.
    /// </summary>
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
        [Tooltip("Optional. Says where the player is about to go.")]
        [SerializeField] private TMP_Text confirmTitle;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button stayButton;

        public const string LevelSelectQuestion = "Go To Level Select?";
        public const string MainMenuQuestion = "Go To The Main Menu?";
        public const string ProgressWarning = "Any Progress In This Level Will Be Lost";

        private PanelSwitcher _panels;
        private PlayerMovement _movement;
        private PlayerRespawn _respawn;
        private RoundOverMessage _roundOver;
        private string _leavingTo;
        private Button _askedFrom;

        public bool IsPaused { get; private set; }

        public bool IsOnOptions => optionsPanel != null && optionsPanel.gameObject.activeSelf;

        /// <summary>True while the "progress will be lost" question is showing.</summary>
        public bool IsConfirmingLeave => confirmPanel != null && confirmPanel.activeSelf;

        /// <summary>False while something else has the screen, so the game can't be paused.</summary>
        public bool CanPause =>
            !SceneFader.IsFading
            && (_roundOver == null || !_roundOver.IsGameOver)
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
                levelSelectButton.onClick.AddListener(() => AskToLeave(levelSelectScene, LevelSelectQuestion, levelSelectButton));
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => AskToLeave(mainMenuScene, MainMenuQuestion, mainMenuButton));
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

        /// <summary>What Escape does: pause, go back from Options or the question, or resume.</summary>
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
            if (_movement != null)
                _movement.EnableControls(false);

            overlay.SetActive(true);
            pausePanel.SetActive(true);
            if (optionsPanel != null)
                optionsPanel.gameObject.SetActive(false);
            if (confirmPanel != null)
                confirmPanel.SetActive(false);
            Select(resumeButton);
        }

        public void Resume()
        {
            if (!IsPaused)
            {
                return;
            }

            IsPaused = false;
            Time.timeScale = 1f;
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
                Select(optionsButton);
            }
        }

        /// <summary>Asks before leaving the level. Without a confirmation panel it just leaves.</summary>
        public void AskToLeave(string scene, string question, Button askedFrom = null)
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
            _askedFrom = askedFrom;
            if (confirmTitle != null)
                confirmTitle.text = question;
            _panels.Switch(pausePanel, confirmPanel);

            // Stay is the safe choice, so a stray Enter doesn't throw the level away.
            Select(stayButton);
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
            Select(_askedFrom);
        }

        private void Leave(string scene)
        {
            // SceneFader also clears the pause when the next scene loads.
            IsPaused = false;
            Time.timeScale = 1f;
            SceneFader.Load(scene);
        }

        private void OnDestroy()
        {
            // Never leave the game paused if the menu goes away some other way.
            if (IsPaused)
            {
                Time.timeScale = 1f;
            }
        }

        private static void Select(Selectable selectable)
        {
            if (selectable != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            }
        }
    }
}
