using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    // Shows how many teddies the player has. Fades out when the player runs out of lives.
    public class CollectibleCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;

        public const float FadeSeconds = 0.4f;

        private PlayerCollectibles _collectibles;
        private PlayerLives _lives;
        private CanvasGroup _group;
        private float _fadeStartsAt = float.PositiveInfinity;
        private bool _hidden;

        // From 0 (gone) to 1 (fully shown).
        public float Visibility => _group != null ? _group.alpha : 1f;

        private void Awake()
        {
            // Adds the teddy counter to the Player if it doesn't have one yet.
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
            ShowCount(_collectibles.Count); // Catches up in case the count changed while the HUD was hidden.

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
                // Wait for the last eye to finish popping so the two animations don't overlap.
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
            // Real time rather than game time, because running out of lives pauses the game.
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
