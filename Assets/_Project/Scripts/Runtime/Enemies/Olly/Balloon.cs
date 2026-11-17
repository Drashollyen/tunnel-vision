using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Balloon : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 1f;

        private Vector2 _a;
        private Vector2 _b;
        private Vector2 _currentTarget;
        private bool _hasPath;

        private void Start()
        {
            // Remember where the points are now, so they stay put when they're children of the balloon.
            _hasPath = pointA != null && pointB != null;
            if (_hasPath)
            {
                _a = pointA.position;
                _b = pointB.position;
                _currentTarget = _b;
            }
        }

        private void Update()
        {
            if (!_hasPath)
            {
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position, _currentTarget, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, _currentTarget) < ArriveDistance)
            {
                _currentTarget = _currentTarget == _a ? _b : _a;
            }
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
