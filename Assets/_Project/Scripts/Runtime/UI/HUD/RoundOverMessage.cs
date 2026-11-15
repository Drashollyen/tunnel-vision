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
        [SerializeField] private float showSeconds = 2f;

        [Header("Game Over")]
        [Tooltip("Optional. Shown after the delay on Out Of Lives, for the mouse like the menus. " +
                 "Left empty, the subtitle says to press Enter instead.")]
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private float buttonDelay = 1.5f;
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private PlayerLives _playerLives;
        private int _lastLives;
        private float _hideAt = float.PositiveInfinity;
        private float _showButtonAt = float.PositiveInfinity;
        private bool _pausedTheGame;
        private NumberDrop _numberDrop;

        public const string PressEnterPrompt = "Press Enter For Level Select";
        public const string LifeLostTitle = "Game Over";
        public const string OutOfLivesTitle = "Out Of Lives";

        // Out of lives, the game stops but looping sounds (the wheels) would play on, so they fade
        // out, after long enough for the death sound to be heard.
        public const float SoundsHoldSeconds = 0.8f;
        public const float SoundsFadeSeconds = 1f;

        /// <summary>True once the last life has gone.</summary>
        public bool IsGameOver { get; private set; }

        /// <summary>True when Game Over has no button and is showing the Press Enter prompt instead.</summary>
        public bool IsWaitingForKey { get; private set; }

        /// <summary>The lives line, such as "2 Lives Remaining" or "1 Life Remaining".</summary>
        public static string LivesLine(int lives) => lives == 1 ? "1 Life Remaining" : $"{lives} Lives Remaining";

        private void Awake()
        {
            panel.SetActive(false);

            _numberDrop = subtitle.GetComponent<NumberDrop>();
            if (_numberDrop == null)
            {
                _numberDrop = subtitle.gameObject.AddComponent<NumberDrop>();
            }

            if (levelSelectButton != null)
            {
                levelSelectButton.gameObject.SetActive(false);
                levelSelectButton.onClick.AddListener(GoToLevelSelect);
            }

            _playerLives = FindAnyObjectByType<PlayerLives>();

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

            if (Time.unscaledTime >= _showButtonAt)
            {
                _showButtonAt = float.PositiveInfinity;
                ShowLevelSelectButton();
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

            if (lives <= 0)
            {
                ShowGameOver();
                return;
            }

            title.text = LifeLostTitle;
            subtitle.gameObject.SetActive(true);
            _numberDrop.Play(LivesLine(livesBefore), LivesLine(lives));
            _hideAt = Time.unscaledTime + showSeconds;
        }

        private void ShowGameOver()
        {
            IsGameOver = true;
            title.text = OutOfLivesTitle;
            subtitle.gameObject.SetActive(false);
            _hideAt = float.PositiveInfinity;
            _showButtonAt = Time.unscaledTime + buttonDelay;

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
        }
    }
}
