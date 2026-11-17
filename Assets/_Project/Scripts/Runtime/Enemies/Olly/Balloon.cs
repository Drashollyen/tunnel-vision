using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Balloon : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [Tooltip("One end of his walk. Only the sideways position counts; he stays on the ground.")]
        [SerializeField] private Transform pointA;
        [Tooltip("The other end of his walk.")]
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 1f;

        [Header("Bop")]
        [Tooltip("How high he bops off the ground, in units.")]
        [SerializeField] private float bopHeight = 0.25f;
        [Tooltip("Bops per second.")]
        [SerializeField] private float bopsPerSecond = 2f;
        [Tooltip("Seconds he idles at each end before turning back.")]
        [SerializeField] private float idleTime = 1f;
        [Tooltip("How far below where he's placed to look for the ground.")]
        [SerializeField] private float maxGroundDrop = 20f;

        private SpriteRenderer _body;
        private float _groundY;
        private float _a;
        private float _b;
        private float _currentTarget;
        private float _idleUntil;
        private bool _hasPath;

        private void Start()
        {
            _body = GetComponent<SpriteRenderer>();
            _groundY = FindGroundY();

            // Remember where the points are now, so they stay put when they're children of the balloon.
            _hasPath = pointA != null && pointB != null;
            if (_hasPath)
            {
                _a = pointA.position.x;
                _b = pointB.position.x;
                _currentTarget = _b;
            }
        }

        private void Update()
        {
            var x = transform.position.x;

            if (_hasPath && Time.time >= _idleUntil)
            {
                x = Mathf.MoveTowards(x, _currentTarget, speed * Time.deltaTime);

                if (_body != null && !Mathf.Approximately(x, transform.position.x))
                {
                    // The art faces right.
                    _body.flipX = x < transform.position.x;
                }

                if (Mathf.Abs(x - _currentTarget) < ArriveDistance)
                {
                    _currentTarget = Mathf.Approximately(_currentTarget, _a) ? _b : _a;
                    _idleUntil = Time.time + idleTime;
                }
            }

            // Bops up off the ground and back down, walking or idling.
            var bop = Mathf.Abs(Mathf.Sin(Time.time * bopsPerSecond * Mathf.PI)) * bopHeight;
            transform.position = new Vector3(x, _groundY + bop, transform.position.z);
        }

        /// <summary>Drops his feet onto the first solid ground below him, so he can be placed anywhere above it.</summary>
        private float FindGroundY()
        {
            var from = (Vector2)transform.position + Vector2.up * 0.5f;
            foreach (var hit in Physics2D.RaycastAll(from, Vector2.down, maxGroundDrop))
            {
                if (!hit.collider.isTrigger && hit.collider.attachedRigidbody == null)
                {
                    return hit.point.y;
                }
            }

            return transform.position.y;
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
