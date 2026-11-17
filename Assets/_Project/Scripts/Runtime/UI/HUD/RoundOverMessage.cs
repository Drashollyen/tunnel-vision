using TMPro;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Shows "Game Over" with the lives left each time the player loses a life. When the last life
    // goes it shows "Out Of Lives" instead, pauses the game and offers a way back to Level Select.
    // In the code, running out of lives is called "game over".
    public class RoundOverMessage : MonoBehaviour
    {
        public const string PressEnterPrompt = "Press Enter For Level Select";
        public const string LifeLostTitle = "Game Over";
        public const string OutOfLivesTitle = "Out Of Lives";
        public const float DefaultButtonFadeSeconds = 0.25f;

        // When out of lives the game stops, but looping sounds (like the wheels) would keep playing,
        // so they fade out once the death sound has had time to be heard.
        public const float SoundsHoldSeconds = 0.8f;
        public const float SoundsFadeSeconds = 1f;

        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [Tooltip("Only used when there's no death fade. Otherwise the message fades out as the screen goes black.")]
        [SerializeField] private float showSeconds = 2f;

        [Header("Game Over")]
        [Tooltip("Optional. Appears when out of lives. If left empty, the player presses Enter instead.")]
        [SerializeField] private Button levelSelectButton;
        [Tooltip("Only used when there's no death fade. Otherwise the button fades in as the screen goes black.")]
        [SerializeField] private float buttonDelay = 1.5f;
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private PlayerLives _playerLives;
        private PlayerRespawn _respawn;
        private int _lastLives;
        private bool _pausedTheGame;
        private NumberDrop _numberDrop;
        private CanvasGroup _panelGroup;
        private float _hideAt = float.PositiveInfinity;
        private float _fadeStartsAt = float.PositiveInfinity;
        private float _fadeSeconds;
        private CanvasGroup _buttonGroup;
        private float _showButtonAt = float.PositiveInfinity;
        private float _buttonFadeSeconds = DefaultButtonFadeSeconds;
        private float _buttonFadeStartedAt = float.PositiveInfinity;

        public bool IsGameOver { get; private set; }

        // From 0 (hidden) to 1 (fully shown).
        public float ButtonVisibility =>
            levelSelectButton != null && levelSelectButton.gameObject.activeSelf && _buttonGroup != null ? _buttonGroup.alpha : 0f;

        // From 0 (hidden) to 1 (fully shown).
        public float Visibility => _panelGroup != null && panel.activeSelf ? _panelGroup.alpha : 0f;

        // True when there's no button, so the player is being asked to press Enter instead.
        public bool IsWaitingForKey { get; private set; }

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
            // Never leave the game paused if this is removed some other way.
            Unpause();
        }

        private void Start()
        {
            // The player may not have set up its lives yet when OnEnable ran, so read them again here.
            _lastLives = _playerLives.CurrentLives;
        }

        private void Update()
        {
            // Real time rather than game time, because running out of lives pauses the game.
            if (Time.unscaledTime >= _hideAt)
            {
                Hide();
            }

            // Fade out along with the screen going black.
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

            // With no button, Enter does the button's job so the player is never stuck.
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
                // Stay up while the player sees the lives drop, then fade out as the screen goes black.
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
                // Fade the button in while the screen goes black, so it's fully there once it's black.
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

            // Don't select the button for the keyboard: a selected button stays highlighted, which
            // would hide the mouse hover effect.
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
