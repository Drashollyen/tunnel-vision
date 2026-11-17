using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    // Shows "Collectible Found" for a moment when the player picks up a teddy, timed like the checkpoint message.
    [RequireComponent(typeof(TMP_Text))]
    public class CollectibleMessage : MonoBehaviour
    {
        public const string Message = "Collectible Found";

        private TMP_Text _text;
        private PlayerCollectibles _collectibles;
        private float _shownAt = float.NegativeInfinity;

        // From 0 (hidden) to 1 (fully shown).
        public float Visibility => _text != null ? _text.alpha : 0f;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.text = Message;
            _text.alpha = 0f;

            // "Or add" because the Player may not have a teddy counter yet if this wakes up first.
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
            // Real time rather than game time, so pausing doesn't leave the message stuck on screen.
            _text.alpha = CheckpointMessage.AlphaAt(Time.unscaledTime - _shownAt);
        }
    }
}
