using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Nicholas's eye on the ending screen: it looks at one friend, holds, then turns to the next,
    /// round the group. Real time.
    /// </summary>
    public class EyeGlance : MonoBehaviour
    {
        [Tooltip("The things to look at, in turn.")]
        [SerializeField] private RectTransform[] targets;
        [SerializeField] private float holdSeconds = 1.2f;
        [SerializeField] private float turnDegreesPerSecond = 360f;

        private int _current;
        private float _nextAt;

        /// <summary>The eye's angle, in degrees, to look from one point at another.</summary>
        public static float AngleTo(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private void Update()
        {
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            if (Time.unscaledTime >= _nextAt)
            {
                _current = (_current + 1) % targets.Length;
                _nextAt = Time.unscaledTime + holdSeconds;
            }

            var target = targets[_current];
            if (target == null)
            {
                return;
            }

            var wanted = AngleTo(transform.position, target.position);
            var angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, wanted, turnDegreesPerSecond * Time.unscaledDeltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
