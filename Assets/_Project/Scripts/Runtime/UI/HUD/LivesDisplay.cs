using TunnelVision.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// One eye per life. When a life goes, its eye swells a little and then pops: it shrinks
    /// away and disappears. A life coming back pops its eye back in. Uses unscaled time, so it
    /// still plays while the game is paused.
    /// </summary>
    public class LivesDisplay : MonoBehaviour
    {
        public const float SwellSeconds = 0.12f;
        public const float PopSeconds = 0.18f;
        public const float AppearSeconds = 0.25f;
        private const float SwellScale = 1.35f;
        private const float AppearOvershoot = 1.15f;

        [SerializeField] private Image[] eyes;
        [SerializeField] private Color aliveColor = Color.white;

        private PlayerLives _playerLives;
        private bool[] _alive;
        private float[] _changedAt;
        private bool _ready;

        public static float LoseSeconds => SwellSeconds + PopSeconds;

        private void Awake()
        {
            _playerLives = FindAnyObjectByType<PlayerLives>();

            if (_playerLives == null)
            {
                Debug.LogError("LivesDisplay: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _alive = new bool[eyes.Length];
            _changedAt = new float[eyes.Length];
        }

        private void OnEnable()
        {
            if (_playerLives == null)
            {
                return;
            }

            _playerLives.LivesChanged += ShowLives;
            ShowLives(_playerLives.CurrentLives); // Catches up if lives changed while the HUD was off.
        }

        private void OnDisable()
        {
            if (_playerLives != null)
            {
                _playerLives.LivesChanged -= ShowLives;
            }
        }

        private void Start()
        {
            if (eyes.Length != _playerLives.MaxLives)
            {
                Debug.LogWarning($"LivesDisplay: has {eyes.Length} eyes but the player has {_playerLives.MaxLives} max lives.");
            }

            // The player's Awake may run after this display's OnEnable, so set the eyes again here,
            // straight away rather than animated, since this is how the level starts.
            _ready = false;
            ShowLives(_playerLives.CurrentLives);
        }

        private void ShowLives(int lives)
        {
            for (int i = 0; i < eyes.Length; i++)
            {
                var alive = i < lives;
                if (_ready && alive == _alive[i])
                {
                    continue;
                }

                _alive[i] = alive;
                // Before the level has started, eyes are set at once instead of animating.
                _changedAt[i] = _ready ? Time.unscaledTime : float.NegativeInfinity;
                Draw(i);
            }

            _ready = true;
        }

        private void Update()
        {
            for (int i = 0; i < eyes.Length; i++)
            {
                if (Time.unscaledTime - _changedAt[i] <= Mathf.Max(LoseSeconds, AppearSeconds) + 0.05f)
                {
                    Draw(i);
                }
            }
        }

        private void Draw(int i)
        {
            var elapsed = Time.unscaledTime - _changedAt[i];
            var scale = _alive[i] ? AppearScale(elapsed) : LoseScale(elapsed);
            eyes[i].rectTransform.localScale = Vector3.one * scale;

            var color = aliveColor;
            color.a *= _alive[i] ? Mathf.Clamp01(elapsed / (AppearSeconds * 0.5f)) : LoseAlpha(elapsed);
            eyes[i].color = color;
        }

        /// <summary>A losing eye's size: it swells, then shrinks to nothing.</summary>
        public static float LoseScale(float elapsed)
        {
            if (elapsed < SwellSeconds)
            {
                return Mathf.Lerp(1f, SwellScale, Ease(elapsed / SwellSeconds));
            }

            var pop = Mathf.Clamp01((elapsed - SwellSeconds) / PopSeconds);
            return Mathf.Lerp(SwellScale, 0f, Ease(pop));
        }

        /// <summary>A losing eye's opacity: solid while it swells, gone once it has popped.</summary>
        public static float LoseAlpha(float elapsed) =>
            1f - Mathf.Clamp01((elapsed - SwellSeconds) / PopSeconds);

        /// <summary>A returning eye's size: it grows past full size, then settles.</summary>
        public static float AppearScale(float elapsed)
        {
            var t = Mathf.Clamp01(elapsed / AppearSeconds);
            if (t < 0.6f)
            {
                return Mathf.Lerp(0f, AppearOvershoot, Ease(t / 0.6f));
            }

            return Mathf.Lerp(AppearOvershoot, 1f, Ease((t - 0.6f) / 0.4f));
        }

        private static float Ease(float t) => t * t * (3f - 2f * t);
    }
}
