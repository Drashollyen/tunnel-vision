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
            _hasPath = pointA != null && pointB != null;
            if (_hasPath)
            {
                _a = Horizontal ? pointA.position.x : pointA.position.y;
                _b = Horizontal ? pointB.position.x : pointB.position.y;
                _currentTarget = _b;
            }
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

        private void OnDrawGizmos()
        {
            if (pointA == null || pointB == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA.position, pointB.position);
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
