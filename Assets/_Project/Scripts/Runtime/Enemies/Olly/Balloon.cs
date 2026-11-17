using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Balloon : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [Tooltip("One end of his walk. Only the sideways position counts; he floats at the height he's placed.")]
        [SerializeField] private Transform pointA;
        [Tooltip("The other end of his walk.")]
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
        private float _floatY;
        private float _a;
        private float _b;
        private float _currentTarget;
        private float _idleUntil;
        private bool _hasPath;

        private void Start()
        {
            _body = GetComponent<SpriteRenderer>();
            _floatY = transform.position.y;

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

            // Bops up and back down, walking or idling.
            var bop = Mathf.Abs(Mathf.Sin(Time.time * bopsPerSecond * Mathf.PI)) * bopHeight;
            transform.position = new Vector3(x, _floatY + bop, transform.position.z);
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
