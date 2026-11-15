using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Checkpoint Reached" when the player reaches a new checkpoint, alongside the stall's
    /// light and bell: it fades in, stays a moment, and fades out. Goes in the HUD on the text.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class CheckpointMessage : MonoBehaviour
    {
        public const string Message = "Checkpoint Reached";
        public const float FadeInSeconds = 0.3f;
        public const float HoldSeconds = 1.5f;
        public const float FadeOutSeconds = 0.6f;

        private TMP_Text _text;
        private PlayerRespawn _respawn;
        private float _shownAt = float.NegativeInfinity;

        public static float TotalSeconds => FadeInSeconds + HoldSeconds + FadeOutSeconds;

        /// <summary>How visible the message is, from 0 to 1.</summary>
        public float Visibility => _text != null ? _text.alpha : 0f;

        /// <summary>The message's opacity this long after a checkpoint is reached.</summary>
        public static float AlphaAt(float elapsed)
        {
            if (elapsed < 0f || elapsed >= TotalSeconds)
            {
                return 0f;
            }

            if (elapsed < FadeInSeconds)
            {
                return elapsed / FadeInSeconds;
            }

            if (elapsed < FadeInSeconds + HoldSeconds)
            {
                return 1f;
            }

            return 1f - (elapsed - FadeInSeconds - HoldSeconds) / FadeOutSeconds;
        }

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = Message;
            _text.alpha = 0f;

            _respawn = FindAnyObjectByType<PlayerRespawn>();
            if (_respawn != null)
            {
                _respawn.CheckpointReached += OnCheckpointReached;
            }
        }

        private void OnDestroy()
        {
            if (_respawn != null)
            {
                _respawn.CheckpointReached -= OnCheckpointReached;
            }
        }

        private void OnCheckpointReached(Level.Checkpoint checkpoint) => Show();

        public void Show()
        {
            _shownAt = Time.unscaledTime;
            _text.alpha = 0f;
        }

        private void Update()
        {
            // Unscaled, so the pause menu doesn't leave it hanging on screen.
            _text.alpha = AlphaAt(Time.unscaledTime - _shownAt);
        }
    }
}
