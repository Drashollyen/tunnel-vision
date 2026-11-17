using TunnelVision.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // One eye per life. A lost life's eye swells and pops; a life coming back pops its eye back in.
    // Uses real time rather than game time, so it still animates while the game is paused.
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
            ShowLives(_playerLives.CurrentLives); // Catches up in case lives changed while the HUD was hidden.
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

            // The player may not have set up its lives yet when OnEnable ran, so set the eyes again
            // here, instantly rather than animated, because this is the start of the level.
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
            // Only redraw eyes that are still animating (plus a tiny margin so the last frame lands).
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

        // A lost eye's size over time: it swells, then shrinks to nothing.
        public static float LoseScale(float elapsed)
        {
            if (elapsed < SwellSeconds)
            {
                return Mathf.Lerp(1f, SwellScale, Ease(elapsed / SwellSeconds));
            }

            var pop = Mathf.Clamp01((elapsed - SwellSeconds) / PopSeconds);
            return Mathf.Lerp(SwellScale, 0f, Ease(pop));
        }

        // A lost eye stays solid while it swells, then fades as it pops.
        public static float LoseAlpha(float elapsed) =>
            1f - Mathf.Clamp01((elapsed - SwellSeconds) / PopSeconds);

        // A returning eye's size over time: it grows a bit too big, then settles to normal.
        public static float AppearScale(float elapsed)
        {
            var t = Mathf.Clamp01(elapsed / AppearSeconds);
            if (t < 0.6f)
            {
                return Mathf.Lerp(0f, AppearOvershoot, Ease(t / 0.6f));
            }

            return Mathf.Lerp(AppearOvershoot, 1f, Ease((t - 0.6f) / 0.4f));
        }

        // Smooths a 0-to-1 value so movement starts and ends gently instead of at a constant speed.
        private static float Ease(float t) => t * t * (3f - 2f * t);
    }
}
