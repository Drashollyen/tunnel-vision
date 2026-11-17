using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// Walks the Thrower Clown back and forth between his two walking points. He stops to throw
    /// (the ThrowerBrain decides when) and carries on walking between rounds.
    /// Drag the PointA and PointB children in the scene to set where he walks.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(ClownThrower))]
    public class ThrowerPatrol : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [Tooltip("One end of his walk. Drag it in the scene; it stays put once the level starts.")]
        [SerializeField] private Transform pointA;
        [Tooltip("The other end of his walk.")]
        [SerializeField] private Transform pointB;
        [SerializeField] private float walkSpeed = 1.5f;
        [Tooltip("Seconds he stands still after letting go of a ball.")]
        [SerializeField] private float pauseAfterThrow = 0.6f;

        private Rigidbody2D _rigidbody;
        private ClownThrower _thrower;
        private float _leftX;
        private float _rightX;
        private bool _walkingRight = true;
        private float _resumeAt;
        private bool _hasWalk;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _thrower = GetComponent<ClownThrower>();

            // Remember where the points are now, so they don't follow him as he walks.
            _hasWalk = pointA != null && pointB != null;
            if (_hasWalk)
            {
                _leftX = Mathf.Min(pointA.position.x, pointB.position.x);
                _rightX = Mathf.Max(pointA.position.x, pointB.position.x);
            }
        }

        private void OnEnable() => _thrower.Thrown += OnThrown;

        private void OnDisable() => _thrower.Thrown -= OnThrown;

        private void OnThrown(ClownBall ball) => _resumeAt = Time.time + pauseAfterThrow;

        private void FixedUpdate()
        {
            if (!_hasWalk || _thrower.IsDead || _thrower.IsThrowing || Time.time < _resumeAt)
            {
                Walk(0f);
                return;
            }

            var x = _rigidbody.position.x;
            if (_walkingRight && x >= _rightX - ArriveDistance)
            {
                _walkingRight = false;
            }
            else if (!_walkingRight && x <= _leftX + ArriveDistance)
            {
                _walkingRight = true;
            }

            var direction = _walkingRight ? 1f : -1f;
            Walk(direction * walkSpeed);
            _thrower.Face(direction);
        }

        private void Walk(float speed)
        {
            _rigidbody.linearVelocity = new Vector2(speed, _rigidbody.linearVelocity.y);
            _thrower.SetSpeed(speed);
        }

        private void OnDrawGizmos()
        {
            if (pointA == null || pointB == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }
    }
}
