using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// Walks the Thrower Clown back and forth between his two walking points. He stops to throw
    /// (ThrowerClownAI decides when) and carries on walking between throws.
    /// Drag the PointA and PointB children in the scene to set where he walks.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(ThrowerClownAI))]
    public class ThrowerPatrol : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        private const float ArriveDistance = 0.1f;

        [Tooltip("One end of his walk. Drag it in the scene; it stays put once the level starts.")]
        [SerializeField] private Transform pointA;
        [Tooltip("The other end of his walk.")]
        [SerializeField] private Transform pointB;
        [SerializeField] private float walkSpeed = 1.5f;
        [Tooltip("Seconds he stands still after letting go of a ball.")]
        [SerializeField] private float pauseAfterThrow = 0.6f;
        [Tooltip("Tick if the sprites are drawn facing right; untick if they face left.")]
        [SerializeField] private bool artFacesRight = true;

        private Rigidbody2D _rigidbody;
        private ThrowerClownAI _thrower;
        private Animator _animator;
        private SpriteRenderer _body;
        private float _leftX;
        private float _rightX;
        private bool _walkingRight = true;
        private bool _wasThrowing;
        private float _resumeAt;
        private bool _hasWalk;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _thrower = GetComponent<ThrowerClownAI>();
            _animator = GetComponent<Animator>();
            _body = GetComponent<SpriteRenderer>();

            // Remember where the points are now, so they don't follow him as he walks.
            _hasWalk = pointA != null && pointB != null;
            if (_hasWalk)
            {
                _leftX = Mathf.Min(pointA.position.x, pointB.position.x);
                _rightX = Mathf.Max(pointA.position.x, pointB.position.x);
            }
        }

        private void FixedUpdate()
        {
            var throwing = _thrower.IsThrowing;
            if (_wasThrowing && !throwing)
            {
                _resumeAt = Time.time + pauseAfterThrow;
            }

            _wasThrowing = throwing;

            if (!_hasWalk || throwing || Time.time < _resumeAt)
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

            if (_body != null)
            {
                _body.flipX = (direction > 0f) != artFacesRight;
            }
        }

        private void Walk(float speed)
        {
            _rigidbody.linearVelocity = new Vector2(speed, _rigidbody.linearVelocity.y);

            if (_animator != null)
            {
                _animator.SetFloat(SpeedParam, Mathf.Abs(speed));
            }
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
