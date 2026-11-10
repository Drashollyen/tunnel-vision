using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Round Over" with the lives left each time the player loses a life,
    /// and "Game Over" when the last one goes.
    /// </summary>
    public class RoundOverMessage : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private float showSeconds = 2f;

        private PlayerLives _playerLives;
        private int _lastLives;
        private float _hideAt = float.PositiveInfinity;

        private void Awake()
        {
            panel.SetActive(false);
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

        private void Start()
        {
            // The player's Awake may run after this message's OnEnable, so read the lives again here.
            _lastLives = _playerLives.CurrentLives;
        }

        private void Update()
        {
            // Unscaled time, so the message still goes away if something pauses the game.
            if (Time.unscaledTime >= _hideAt)
            {
                Hide();
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
                title.text = "Game Over";
                subtitle.gameObject.SetActive(false);
                _hideAt = float.PositiveInfinity;
                return;
            }

            title.text = "Round Over";
            subtitle.text = lives == 1 ? "1 Life Remaining" : $"{lives} Lives Remaining";
            subtitle.gameObject.SetActive(true);
            _hideAt = Time.unscaledTime + showSeconds;
        }

        private void Hide()
        {
            panel.SetActive(false);
            _hideAt = float.PositiveInfinity;
        }
    }
}
