using TMPro;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Game Over" with the lives left each time the player loses a life, and
    /// "Out Of Lives" when the last one goes. Out of lives (called Game Over in the code),
    /// the game pauses and, after a short delay, a button offers the way back to Level Select.
    /// </summary>
    public class RoundOverMessage : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [Tooltip("How long a lost life's message stays up when there's no death sequence to follow. " +
                 "With one, it fades out as the screen goes black instead.")]
        [SerializeField] private float showSeconds = 2f;

        [Header("Game Over")]
        [Tooltip("Optional. Shown after the delay on Out Of Lives, for the mouse like the menus. " +
                 "Left empty, the subtitle says to press Enter instead.")]
        [SerializeField] private Button levelSelectButton;
        [Tooltip("When there's no death sequence to follow. With one, the button fades in as the screen goes black.")]
        [SerializeField] private float buttonDelay = 1.5f;
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private PlayerLives _playerLives;
        private int _lastLives;
        private float _hideAt = float.PositiveInfinity;
        private float _showButtonAt = float.PositiveInfinity;
        private CanvasGroup _buttonGroup;
        private float _buttonFadeSeconds = DefaultButtonFadeSeconds;
        private float _buttonFadeStartedAt = float.PositiveInfinity;

        /// <summary>How long the button takes to fade in when there's no death sequence to match.</summary>
        public const float DefaultButtonFadeSeconds = 0.25f;
        private bool _pausedTheGame;
        private NumberDrop _numberDrop;
        private CanvasGroup _panelGroup;
        private PlayerRespawn _respawn;
        private float _fadeStartsAt = float.PositiveInfinity;
        private float _fadeSeconds;

        public const string PressEnterPrompt = "Press Enter For Level Select";
        public const string LifeLostTitle = "Game Over";
        public const string OutOfLivesTitle = "Out Of Lives";

        // Out of lives, the game stops but looping sounds (the wheels) would play on, so they fade
        // out, after long enough for the death sound to be heard.
        public const float SoundsHoldSeconds = 0.8f;
        public const float SoundsFadeSeconds = 1f;

        /// <summary>True once the last life has gone.</summary>
        public bool IsGameOver { get; private set; }

        /// <summary>How visible the Back To Level Select button is, from 0 to 1.</summary>
        public float ButtonVisibility =>
            levelSelectButton != null && levelSelectButton.gameObject.activeSelf && _buttonGroup != null ? _buttonGroup.alpha : 0f;

        /// <summary>How visible the message is, from 0 to 1. It fades out as the screen goes black.</summary>
        public float Visibility => _panelGroup != null && panel.activeSelf ? _panelGroup.alpha : 0f;

        /// <summary>True when Game Over has no button and is showing the Press Enter prompt instead.</summary>
        public bool IsWaitingForKey { get; private set; }

        /// <summary>The lives line, such as "2 Lives Remaining" or "1 Life Remaining".</summary>
        public static string LivesLine(int lives) => lives == 1 ? "1 Life Remaining" : $"{lives} Lives Remaining";

        private void Awake()
        {
            panel.SetActive(false);
            _panelGroup = panel.TryGetComponent(out CanvasGroup group) ? group : panel.AddComponent<CanvasGroup>();

            _numberDrop = subtitle.GetComponent<NumberDrop>();
            if (_numberDrop == null)
            {
                _numberDrop = subtitle.gameObject.AddComponent<NumberDrop>();
            }

            if (levelSelectButton != null)
            {
                levelSelectButton.gameObject.SetActive(false);
                levelSelectButton.onClick.AddListener(GoToLevelSelect);
                _buttonGroup = levelSelectButton.TryGetComponent(out CanvasGroup buttonGroup)
                    ? buttonGroup
                    : levelSelectButton.gameObject.AddComponent<CanvasGroup>();
            }

            _playerLives = FindAnyObjectByType<PlayerLives>();
            _respawn = _playerLives != null ? _playerLives.GetComponent<PlayerRespawn>() : null;

            if (_playerLives == null)
            {
                Debug.LogError("RoundOverMessage: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_playerLives == null)
            {
                return;
            }

            _playerLives.LivesChanged += OnLivesChanged;
            _lastLives = _playerLives.CurrentLives;
        }

        private void OnDisable()
        {
            if (_playerLives != null)
            {
                _playerLives.LivesChanged -= OnLivesChanged;
            }
        }

        private void OnDestroy()
        {
            // Never leave the game paused if this goes away some other way.
            Unpause();
        }

        private void Start()
        {
            // The player's Awake may run after this message's OnEnable, so read the lives again here.
            _lastLives = _playerLives.CurrentLives;
        }

        private void Update()
        {
            // Unscaled time, because Game Over pauses the game.
            if (Time.unscaledTime >= _hideAt)
            {
                Hide();
            }

            // Fading out with the screen as it goes black.
            if (Time.unscaledTime >= _fadeStartsAt)
            {
                var faded = _fadeSeconds > 0f ? (Time.unscaledTime - _fadeStartsAt) / _fadeSeconds : 1f;
                _panelGroup.alpha = 1f - Mathf.Clamp01(faded);
                if (faded >= 1f)
                {
                    Hide();
                }
            }

            if (Time.unscaledTime >= _showButtonAt)
            {
                _showButtonAt = float.PositiveInfinity;
                ShowLevelSelectButton();
            }

            if (_buttonGroup != null && Time.unscaledTime >= _buttonFadeStartedAt)
            {
                var shown = _buttonFadeSeconds > 0f ? (Time.unscaledTime - _buttonFadeStartedAt) / _buttonFadeSeconds : 1f;
                _buttonGroup.alpha = Mathf.Clamp01(shown);
                if (shown >= 1f)
                {
                    _buttonFadeStartedAt = float.PositiveInfinity;
                }
            }

            // Without a button, Enter does the button's job so the player is never stuck.
            var keyboard = Keyboard.current;
            if (IsWaitingForKey && keyboard != null
                && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                IsWaitingForKey = false;
                GoToLevelSelect();
            }
        }

        private void OnLivesChanged(int lives)
        {
            var livesBefore = _lastLives;
            bool lostALife = lives < livesBefore;
            _lastLives = lives;

            if (!lostALife)
            {
                Hide();
                return;
            }

            panel.SetActive(true);
            _panelGroup.alpha = 1f;
            _fadeStartsAt = float.PositiveInfinity;

            if (lives <= 0)
            {
                ShowGameOver();
                return;
            }

            title.text = LifeLostTitle;
            subtitle.gameObject.SetActive(true);
            _numberDrop.Play(LivesLine(livesBefore), LivesLine(lives));

            if (_respawn != null)
            {
                // Stay up through the reveal, then fade out alongside the screen going black.
                _hideAt = float.PositiveInfinity;
                _fadeSeconds = _respawn.FadeToBlackSeconds;
                _fadeStartsAt = Time.unscaledTime + _respawn.SecondsUntilBlack - _fadeSeconds;
            }
            else
            {
                _hideAt = Time.unscaledTime + showSeconds;
            }
        }

        private void ShowGameOver()
        {
            IsGameOver = true;
            title.text = OutOfLivesTitle;
            subtitle.gameObject.SetActive(false);
            _hideAt = float.PositiveInfinity;
            if (_respawn != null)
            {
                // Fades in while the screen goes black, so it's fully there on the black screen.
                _buttonFadeSeconds = _respawn.FadeToBlackSeconds;
                _showButtonAt = Time.unscaledTime + _respawn.SecondsUntilBlack - _buttonFadeSeconds;
            }
            else
            {
                _buttonFadeSeconds = DefaultButtonFadeSeconds;
                _showButtonAt = Time.unscaledTime + buttonDelay;
            }

            Time.timeScale = 0f;
            _pausedTheGame = true;
            SoundEffectsVolume.FadeOutGameplay(SoundsFadeSeconds, SoundsHoldSeconds);
        }

        private void ShowLevelSelectButton()
        {
            if (levelSelectButton == null)
            {
                subtitle.gameObject.SetActive(true);
                _numberDrop.ShowPlain(PressEnterPrompt);
                IsWaitingForKey = true;
                return;
            }

            levelSelectButton.gameObject.SetActive(true);
            _buttonGroup.alpha = 0f;
            _buttonFadeStartedAt = Time.unscaledTime;

            // Not selected for the keyboard: a selected button stays highlighted and enlarged, so
            // hovering and clicking wouldn't show. Clear anything left selected for the same reason.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void GoToLevelSelect()
        {
            Unpause();
            SceneFader.Load(levelSelectScene);
        }

        private void Unpause()
        {
            if (_pausedTheGame)
            {
                Time.timeScale = 1f;
                _pausedTheGame = false;
            }
        }

        private void Hide()
        {
            panel.SetActive(false);
            _hideAt = float.PositiveInfinity;
            _fadeStartsAt = float.PositiveInfinity;
            _panelGroup.alpha = 1f;
        }
    }
}
