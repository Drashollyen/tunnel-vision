using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Balloon : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        public enum MoveDirection
        {
            Horizontal,
            Vertical,
        }

        [Tooltip("Horizontal floats side to side between the points' x; Vertical floats up and down between their y.")]
        [SerializeField] private MoveDirection moveDirection = MoveDirection.Horizontal;
        [Tooltip("One end of his path. Drag it in the scene; it stays put once the level starts.")]
        [SerializeField] private Transform pointA;
        [Tooltip("The other end of his path.")]
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 1f;

        [Header("Bop")]
        [Tooltip("How high he bops, in units.")]
        [SerializeField] private float bopHeight = 0.25f;
        [Tooltip("Bops per second.")]
        [SerializeField] private float bopsPerSecond = 2f;
        [Tooltip("Seconds he idles at each end before turning back.")]
        [SerializeField] private float idleTime = 1f;

        private SpriteRenderer _body;
        private Vector2 _home;
        private float _a;
        private float _b;
        private float _along;
        private float _currentTarget;
        private float _idleUntil;
        private bool _hasPath;

        private bool Horizontal => moveDirection == MoveDirection.Horizontal;

        private void Start()
        {
            _body = GetComponent<SpriteRenderer>();
            _home = transform.position;
            _along = Horizontal ? _home.x : _home.y;

            // Remember where the points are now, so they stay put when they're children of the balloon.
            _hasPath = TryGetPath(_home, out _a, out _b);
            _currentTarget = _b;
        }

        private void Update()
        {
            if (_hasPath && Time.time >= _idleUntil)
            {
                var next = Mathf.MoveTowards(_along, _currentTarget, speed * Time.deltaTime);

                if (Horizontal && _body != null && !Mathf.Approximately(next, _along))
                {
                    // The art faces right.
                    _body.flipX = next < _along;
                }

                _along = next;

                if (Mathf.Abs(_along - _currentTarget) < ArriveDistance)
                {
                    _currentTarget = Mathf.Approximately(_currentTarget, _a) ? _b : _a;
                    _idleUntil = Time.time + idleTime;
                }
            }

            // Bops up and back down, moving or idling.
            var bop = Mathf.Abs(Mathf.Sin(Time.time * bopsPerSecond * Mathf.PI)) * bopHeight;
            var x = Horizontal ? _along : _home.x;
            var y = Horizontal ? _home.y : _along;
            transform.position = new Vector3(x, y + bop, transform.position.z);
        }

        /// <summary>
        /// The two ends of his path along the chosen direction. If the points line up the other way
        /// (side by side when he's set to Vertical), the gap between them is used as his path instead,
        /// centred on where he's placed, so switching direction just works.
        /// </summary>
        private bool TryGetPath(Vector2 home, out float a, out float b)
        {
            a = b = 0f;
            if (pointA == null || pointB == null)
            {
                return false;
            }

            Vector2 pa = pointA.position;
            Vector2 pb = pointB.position;
            a = Horizontal ? pa.x : pa.y;
            b = Horizontal ? pb.x : pb.y;

            if (Mathf.Abs(a - b) < ArriveDistance)
            {
                var half = Mathf.Abs(Horizontal ? pa.y - pb.y : pa.x - pb.x) / 2f;
                var centre = Horizontal ? home.x : home.y;
                a = centre - half;
                b = centre + half;
            }

            return Mathf.Abs(a - b) >= ArriveDistance;
        }

        private void OnDrawGizmos()
        {
            Vector2 home = Application.isPlaying ? _home : (Vector2)transform.position;
            if (!TryGetPath(home, out var a, out var b))
            {
                return;
            }

            Gizmos.color = Color.cyan;
            if (Horizontal)
            {
                Gizmos.DrawLine(new Vector2(a, home.y), new Vector2(b, home.y));
            }
            else
            {
                Gizmos.DrawLine(new Vector2(home.x, a), new Vector2(home.x, b));
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerRespawn>() != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
