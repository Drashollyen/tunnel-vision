using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows how many collectibles the player has. Goes in the HUD with a text for the number
    /// (and an icon beside it if wanted). If the Player has no PlayerCollectibles yet, one is
    /// added, so the counter works before any pickups exist. On Out Of Lives it fades out just
    /// after the last eye pops, and comes back if lives do.
    /// </summary>
    public class CollectibleCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;

        public const float FadeSeconds = 0.4f;

        private PlayerCollectibles _collectibles;
        private PlayerLives _lives;
        private CanvasGroup _group;
        private float _fadeStartsAt = float.PositiveInfinity;
        private bool _hidden;

        /// <summary>How visible the counter is, from 0 (gone) to 1.</summary>
        public float Visibility => _group != null ? _group.alpha : 1f;

        private void Awake()
        {
            _collectibles = PlayerCollectibles.FindOrAdd();
            _lives = FindAnyObjectByType<PlayerLives>();
            _group = TryGetComponent(out CanvasGroup group) ? group : gameObject.AddComponent<CanvasGroup>();

            if (_collectibles == null)
            {
                Debug.LogError("CollectibleCounter: no Player found in the scene. Add the Player prefab.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_collectibles == null)
            {
                return;
            }

            _collectibles.CountChanged += ShowCount;
            ShowCount(_collectibles.Count); // Catches up if the count changed while the HUD was off.

            if (_lives != null)
            {
                _lives.LivesChanged += OnLivesChanged;
            }
        }

        private void OnDisable()
        {
            if (_collectibles != null)
            {
                _collectibles.CountChanged -= ShowCount;
            }

            if (_lives != null)
            {
                _lives.LivesChanged -= OnLivesChanged;
            }
        }

        private void OnLivesChanged(int lives)
        {
            _hidden = lives <= 0;
            if (_hidden)
            {
                // After the last eye has popped, so the two don't compete.
                _fadeStartsAt = Time.unscaledTime + LivesDisplay.LoseSeconds;
            }
            else
            {
                _fadeStartsAt = float.PositiveInfinity;
                _group.alpha = 1f;
            }
        }

        private void Update()
        {
            // Unscaled time, because Out Of Lives pauses the game.
            if (_hidden && Time.unscaledTime >= _fadeStartsAt)
            {
                _group.alpha = 1f - Mathf.Clamp01((Time.unscaledTime - _fadeStartsAt) / FadeSeconds);
            }
        }

        private void ShowCount(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
