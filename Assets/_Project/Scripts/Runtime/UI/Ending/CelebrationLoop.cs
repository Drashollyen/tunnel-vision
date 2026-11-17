using UnityEngine;

namespace TunnelVision.UI
{
    // Makes a character on a screen hop, sway or bob in place, over and over. It always comes
    // back to where it started, so it never wanders off. Uses real time.
    public class CelebrationLoop : MonoBehaviour
    {
        public enum Move
        {
            Hop,
            Sway,
            Bob,
        }

        [SerializeField] private Move move = Move.Hop;
        [Tooltip("Height of a hop or bob, in UI units, or the sway's angle in degrees.")]
        [SerializeField] private float amount = 30f;
        [Tooltip("Loops per second.")]
        [SerializeField] private float speed = 1.5f;
        [Tooltip("Where in the loop it starts, 0 to 1, so characters don't all move together.")]
        [SerializeField, Range(0f, 1f)] private float offset;

        private RectTransform _rect;
        private Vector2 _rest;

        // How high (Hop, Bob) or how tilted (Sway) the character is at a given time.
        public static float At(Move move, float time, float amount, float speed, float offset)
        {
            var phase = (time * speed + offset) * Mathf.PI * 2f;
            switch (move)
            {
                case Move.Hop:
                    // Bounces off the ground: up and back down, never below where it stands.
                    return Mathf.Abs(Mathf.Sin(phase * 0.5f)) * amount;
                case Move.Sway:
                    return Mathf.Sin(phase) * amount;
                default:
                    return (Mathf.Sin(phase) * 0.5f + 0.5f) * amount;
            }
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
            _rest = _rect.anchoredPosition;
        }

        private void Update()
        {
            var value = At(move, Time.unscaledTime, amount, speed, offset);
            if (move == Move.Sway)
            {
                _rect.localRotation = Quaternion.Euler(0f, 0f, value);
            }
            else
            {
                _rect.anchoredPosition = _rest + Vector2.up * value;
            }
        }
    }
}
