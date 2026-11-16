using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Collectible Found" when the player picks up a teddy. Works the same way as the checkpoint
    /// message: it fades in, stays a moment, and fades out. Goes in the HUD on the text.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class CollectibleMessage : MonoBehaviour
    {
        public const string Message = "Collectible Found";

        private TMP_Text _text;
        private PlayerCollectibles _collectibles;
        private float _shownAt = float.NegativeInfinity;

        /// <summary>How visible the message is, from 0 to 1.</summary>
        public float Visibility => _text != null ? _text.alpha : 0f;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = Message;
            _text.alpha = 0f;

            // Find or add, not just find: the Player has none until something adds one, and the
            // counter beside this may not have woken up yet.
            _collectibles = PlayerCollectibles.FindOrAdd();
            if (_collectibles != null)
            {
                _collectibles.CountChanged += OnCountChanged;
            }
        }

        private void OnDestroy()
        {
            if (_collectibles != null)
            {
                _collectibles.CountChanged -= OnCountChanged;
            }
        }

        private void OnCountChanged(int count) => Show();

        public void Show()
        {
            _shownAt = Time.unscaledTime;
            _text.alpha = 0f;
        }

        private void Update()
        {
            // Same timing as the checkpoint message, unscaled so pausing doesn't leave it on screen.
            _text.alpha = CheckpointMessage.AlphaAt(Time.unscaledTime - _shownAt);
        }
    }
}
