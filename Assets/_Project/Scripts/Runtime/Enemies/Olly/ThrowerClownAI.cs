using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class ThrowerClownAI : MonoBehaviour
    {
        private static readonly int ThrowParam = Animator.StringToHash("Throw");

        private const float MaxThrowSeconds = 2f;
        private const float MinFlightSeconds = 0.25f;

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer body;

        [Tooltip("Tick if the sprites are drawn facing right; untick if they face left.")]
        [SerializeField] private bool artFacesRight = true;

        [Header("Throwing")]
        [SerializeField] private ClownBall ballPrefab;

        [Tooltip("Where the ball leaves his hand, set up for facing right. Mirrored when he faces left.")]
        [SerializeField] private Transform throwPoint;

        [Tooltip("How fast the ball travels sideways. Faster means a flatter throw.")]
        [SerializeField] private float throwSpeed = 8f;

        [Tooltip("Seconds between throws in the same round.")]
        [SerializeField] private float cooldown = 1.5f;

        [Header("Behaviour")]
        [SerializeField] private float range = 10f;
        [SerializeField] private int throwsPerRound = 3;
        [SerializeField] private float reloadTime = 3f;

        private Transform _player;
        private Vector2 _target;
        private bool _isThrowing;
        private float _throwStartedAt;
        private float _nextThrowAt;
        private int _throwsThisRound;

        /// <summary>True from the start of a throw until the ball leaves his hand.</summary>
        public bool IsThrowing => _isThrowing;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (body == null)
            {
                body = GetComponent<SpriteRenderer>();
            }

            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("ThrowerClownAI: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
        }

        private void Update()
        {
            if (_isThrowing && Time.time - _throwStartedAt > MaxThrowSeconds)
            {
                _isThrowing = false;
            }

            if (_isThrowing || Time.time < _nextThrowAt)
            {
                return;
            }

            var distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer > range)
            {
                return;
            }

            StartThrow();
        }

        private void StartThrow()
        {
            _target = _player.position;
            Face(_target.x - transform.position.x);
            _isThrowing = true;
            _throwStartedAt = Time.time;
            animator.SetTrigger(ThrowParam);
        }

        public void ReleaseBall()
        {
            if (!_isThrowing)
            {
                return;
            }

            _isThrowing = false;
            _throwsThisRound++;

            if (_throwsThisRound >= throwsPerRound)
            {
                _throwsThisRound = 0;
                _nextThrowAt = Time.time + reloadTime;
            }
            else
            {
                _nextThrowAt = Time.time + cooldown;
            }

            if (ballPrefab == null)
            {
                Debug.LogWarning("ThrowerClownAI: no ball prefab to throw.", this);
                return;
            }

            var from = ReleasePoint();
            var ball = Instantiate(ballPrefab, from, Quaternion.identity);
            ball.Launch(LaunchVelocity(from, _target, ball.Gravity));
        }

        private bool FacingRight()
        {
            return body == null ? artFacesRight : body.flipX != artFacesRight;
        }

        private void Face(float direction)
        {
            if (body == null || direction == 0f)
            {
                return;
            }

            body.flipX = (direction > 0f) != artFacesRight;
        }

        private Vector2 ReleasePoint()
        {
            if (throwPoint == null)
            {
                return transform.position;
            }

            var local = throwPoint.localPosition;

            if (!FacingRight())
            {
                local.x = -local.x;
            }

            return transform.TransformPoint(local);
        }

        private Vector2 LaunchVelocity(Vector2 from, Vector2 to, float gravity)
        {
            var distanceX = to.x - from.x;
            var distanceY = to.y - from.y;
            var flightTime = Mathf.Max(MinFlightSeconds, Mathf.Abs(distanceX) / throwSpeed);

            return new Vector2(distanceX / flightTime, distanceY / flightTime + 0.5f * gravity * flightTime);
        }
    }
}
