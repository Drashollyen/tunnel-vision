using System;
using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// The Thrower Clown's body: plays its animations and throws balls. The AI decides when to move
    /// and when to throw, and calls into this to make it happen:
    /// <list type="bullet">
    /// <item><see cref="ThrowAt(Vector2)"/> turns to face the target and throws a ball that lands on it.</item>
    /// <item><see cref="CanThrow"/> says whether a throw would start right now (not mid-throw, not cooling down, not dead).</item>
    /// <item><see cref="SetSpeed"/>, <see cref="SetGrounded"/>, <see cref="Face"/>, <see cref="Hurt"/> and <see cref="Die"/> drive the rest.</item>
    /// </list>
    /// </summary>
    public class ClownThrower : MonoBehaviour
    {
        // Animator parameter names, shared with the normal clown's controller (plus Throw).
        public static readonly int SpeedParam = Animator.StringToHash("Speed");
        public static readonly int GroundedParam = Animator.StringToHash("Grounded");
        public static readonly int HurtParam = Animator.StringToHash("Hurt");
        public static readonly int DieParam = Animator.StringToHash("Die");
        public static readonly int ThrowParam = Animator.StringToHash("Throw");

        // How long a throw can go without reaching its release frame before it's given up on.
        private const float MaxThrowSeconds = 2f;

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer body;

        [Header("Throwing")]
        [SerializeField] private ClownBall ballPrefab;

        [Tooltip("Where the ball leaves his hand, set up for facing right. Mirrored when he faces left.")]
        [SerializeField] private Transform throwPoint;

        [Tooltip("How fast the ball travels sideways, in units per second. Faster means a flatter throw.")]
        [SerializeField] private float throwSpeed = 8f;

        [Tooltip("Seconds after a ball leaves his hand before he can throw again.")]
        [SerializeField] private float cooldown = 1.5f;

        private Vector2 _target;
        private float _throwStartedAt;
        private float _nextThrowAt;
        private bool _dead;

        /// <summary>Raised with each ball as it leaves his hand.</summary>
        public event Action<ClownBall> Thrown;

        /// <summary>True from the start of a throw until the ball leaves his hand.</summary>
        public bool IsThrowing { get; private set; }

        /// <summary>True when <see cref="ThrowAt(Vector2)"/> would start a throw right now.</summary>
        public bool CanThrow => !_dead && !IsThrowing && Time.time >= _nextThrowAt;

        /// <summary>True while he faces right (the way the sprites are drawn).</summary>
        public bool FacingRight => body == null || !body.flipX;

        public bool IsDead => _dead;

        private void Reset()
        {
            animator = GetComponent<Animator>();
            body = GetComponent<SpriteRenderer>();
        }

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
        }

        private void Update()
        {
            // If something cut the throw short before the release frame, don't stay stuck mid-throw.
            if (IsThrowing && Time.time - _throwStartedAt > MaxThrowSeconds)
            {
                IsThrowing = false;
            }
        }

        /// <summary>
        /// Turns to face <paramref name="target"/> and plays the throw. The ball leaves on the release
        /// frame and is aimed to land on <paramref name="target"/>. Returns false, and does nothing,
        /// when <see cref="CanThrow"/> is false.
        /// </summary>
        public bool ThrowAt(Vector2 target)
        {
            if (!CanThrow)
            {
                return false;
            }

            _target = target;
            Face(target.x - transform.position.x);
            IsThrowing = true;
            _throwStartedAt = Time.time;
            animator.SetTrigger(ThrowParam);
            return true;
        }

        /// <summary>Throws at <paramref name="target"/>'s current position.</summary>
        public bool ThrowAt(Transform target) => target != null && ThrowAt((Vector2)target.position);

        /// <summary>Faces right for a positive <paramref name="direction"/>, left for a negative one; 0 leaves him as he is.</summary>
        public void Face(float direction)
        {
            if (body == null || direction == 0f)
            {
                return;
            }

            body.flipX = direction < 0f;
        }

        /// <summary>How fast he's moving; above 0.1 plays Run, otherwise Idle. Sign doesn't matter.</summary>
        public void SetSpeed(float speed) => animator.SetFloat(SpeedParam, Mathf.Abs(speed));

        /// <summary>False plays Jump until he's grounded again.</summary>
        public void SetGrounded(bool grounded) => animator.SetBool(GroundedParam, grounded);

        /// <summary>Plays the hurt flinch. Cancels a throw that hasn't let go of the ball yet.</summary>
        public void Hurt()
        {
            IsThrowing = false;
            animator.SetTrigger(HurtParam);
        }

        /// <summary>Plays the death animation. He won't throw again.</summary>
        public void Die()
        {
            _dead = true;
            IsThrowing = false;
            animator.ResetTrigger(ThrowParam);
            animator.SetTrigger(DieParam);
        }

        /// <summary>
        /// Called by the Throw clip on its release frame. Spawns the ball at the throw point and sends
        /// it at the target.
        /// </summary>
        public void ReleaseBall()
        {
            if (!IsThrowing)
            {
                return;
            }

            IsThrowing = false;
            _nextThrowAt = Time.time + cooldown;

            if (ballPrefab == null)
            {
                Debug.LogWarning($"{name} has no ball prefab to throw.", this);
                return;
            }

            var from = ReleasePoint();
            var ball = Instantiate(ballPrefab, from, Quaternion.identity);
            ball.Launch(LaunchVelocity(from, _target, throwSpeed, ball.Gravity));
            Thrown?.Invoke(ball);
        }

        /// <summary>Where the ball leaves his hand, mirrored to whichever way he faces.</summary>
        public Vector2 ReleasePoint()
        {
            if (throwPoint == null)
            {
                return transform.position;
            }

            var local = throwPoint.localPosition;
            if (!FacingRight)
            {
                local.x = -local.x;
            }

            return transform.TransformPoint(local);
        }

        /// <summary>
        /// The velocity that carries a ball from <paramref name="from"/> to <paramref name="to"/>,
        /// travelling sideways at <paramref name="horizontalSpeed"/> under <paramref name="gravity"/>
        /// (units per second squared, pulling down). A target straight above or below gets a short lob.
        /// </summary>
        public static Vector2 LaunchVelocity(Vector2 from, Vector2 to, float horizontalSpeed, float gravity)
        {
            const float MinFlightSeconds = 0.25f;

            var dx = to.x - from.x;
            var dy = to.y - from.y;
            var speed = Mathf.Max(0.01f, Mathf.Abs(horizontalSpeed));
            var time = Mathf.Max(MinFlightSeconds, Mathf.Abs(dx) / speed);

            return new Vector2(dx / time, dy / time + 0.5f * gravity * time);
        }
    }
}
