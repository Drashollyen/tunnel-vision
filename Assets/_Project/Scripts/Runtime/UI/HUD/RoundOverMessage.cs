using TMPro;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Round Over" with the lives left each time the player loses a life,
    /// and "Game Over" when the last one goes. On Game Over the game pauses and,
    /// after a short delay, a button offers the way back to Level Select.
    /// </summary>
    public class RoundOverMessage : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private float showSeconds = 2f;

        [Header("Game Over")]
        [Tooltip("Optional. Shown after the delay on Game Over, and selected so Enter works. " +
                 "Left empty, the subtitle says to press Enter instead.")]
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private float buttonDelay = 1.5f;
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private PlayerLives _playerLives;
        private int _lastLives;
        private float _hideAt = float.PositiveInfinity;
        private float _showButtonAt = float.PositiveInfinity;
        private bool _pausedTheGame;

        public const string PressEnterPrompt = "Press Enter For Level Select";

        /// <summary>True once the last life has gone.</summary>
        public bool IsGameOver { get; private set; }

        /// <summary>True when Game Over has no button and is showing the Press Enter prompt instead.</summary>
        public bool IsWaitingForKey { get; private set; }

        private void Awake()
        {
            panel.SetActive(false);

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
            bool lostALife = lives < _lastLives;
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

            title.text = "Round Over";
            subtitle.text = lives == 1 ? "1 Life Remaining" : $"{lives} Lives Remaining";
            subtitle.gameObject.SetActive(true);
            _hideAt = Time.unscaledTime + showSeconds;
        }

        private void ShowGameOver()
        {
            IsGameOver = true;
            title.text = "Game Over";
            subtitle.gameObject.SetActive(false);
            _hideAt = float.PositiveInfinity;
            _showButtonAt = Time.unscaledTime + buttonDelay;

            Time.timeScale = 0f;
            _pausedTheGame = true;
        }

        private void ShowLevelSelectButton()
        {
            if (levelSelectButton == null)
            {
                subtitle.text = PressEnterPrompt;
                subtitle.gameObject.SetActive(true);
                IsWaitingForKey = true;
                return;
            }

            levelSelectButton.gameObject.SetActive(true);

            // Selected straight away so Enter works without touching the mouse.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(levelSelectButton.gameObject);
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
