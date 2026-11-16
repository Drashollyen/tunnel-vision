using System;
using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// The Sword Swallower's body: plays his animations and throws his sword. The AI decides when;
    /// this makes it happen, the same way <see cref="ClownThrower"/> does for the Thrower Clown:
    /// <list type="bullet">
    /// <item><see cref="ThrowAt(Vector2)"/> turns to face the target and throws the sword level, straight ahead.</item>
    /// <item><see cref="CanThrow"/> says whether a throw would start right now (not mid-throw, not cooling down).</item>
    /// <item><see cref="Face"/> turns him without throwing.</item>
    /// </list>
    /// He stands in one place: there's no walking art.
    /// </summary>
    public class SwordSwallower : MonoBehaviour
    {
        public static readonly int ThrowParam = Animator.StringToHash("Throw");

        // How long a throw can go without reaching its release frame before it's given up on.
        private const float MaxThrowSeconds = 2f;

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer body;

        [Tooltip("True if the sprites are drawn facing right. The sword swallower art faces left.")]
        [SerializeField] private bool artFacesRight;

        [Header("Throwing")]
        [SerializeField] private SwordProjectile swordPrefab;

        [Tooltip("Where the sword leaves his hand, set up for the way the art faces. Mirrored when he turns.")]
        [SerializeField] private Transform throwPoint;

        [Tooltip("How fast the sword flies, in units per second.")]
        [SerializeField] private float swordSpeed = 10f;

        [Tooltip("Seconds after a sword leaves his hand before he can throw again. The throw animation runs about 2.2 s after the release.")]
        [SerializeField] private float cooldown = 2.5f;

        private float _throwStartedAt;
        private float _nextThrowAt;

        /// <summary>Raised with each sword as it leaves his hand.</summary>
        public event Action<SwordProjectile> Thrown;

        /// <summary>True from the start of a throw until the sword leaves his hand.</summary>
        public bool IsThrowing { get; private set; }

        /// <summary>True when <see cref="ThrowAt(Vector2)"/> would start a throw right now.</summary>
        public bool CanThrow => !IsThrowing && Time.time >= _nextThrowAt;

        /// <summary>True while he faces right.</summary>
        public bool FacingRight => body == null ? artFacesRight : artFacesRight != body.flipX;

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
            if (IsThrowing && Time.time - _throwStartedAt > MaxThrowSeconds)
            {
                IsThrowing = false;
            }
        }

        /// <summary>
        /// Turns to face <paramref name="target"/> and plays the throw. The sword leaves on the
        /// release frame and flies level, straight ahead. Returns false, and does nothing, when
        /// <see cref="CanThrow"/> is false.
        /// </summary>
        public bool ThrowAt(Vector2 target)
        {
            if (!CanThrow)
            {
                return false;
            }

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

            var faceRight = direction > 0f;
            body.flipX = faceRight != artFacesRight;
        }

        /// <summary>
        /// Called by the Throw clip on its release frame. Spawns the sword at the throw point and
        /// sends it at the target.
        /// </summary>
        public void ReleaseSword()
        {
            if (!IsThrowing)
            {
                return;
            }

            IsThrowing = false;
            _nextThrowAt = Time.time + cooldown;

            if (swordPrefab == null)
            {
                Debug.LogWarning($"{name} has no sword prefab to throw.", this);
                return;
            }

            var sword = Instantiate(swordPrefab, ReleasePoint(), Quaternion.identity);
            sword.Launch(LaunchVelocity(FacingRight, swordSpeed));
            Thrown?.Invoke(sword);
        }

        /// <summary>Where the sword leaves his hand, mirrored to whichever way he faces.</summary>
        public Vector2 ReleasePoint()
        {
            if (throwPoint == null)
            {
                return transform.position;
            }

            var local = throwPoint.localPosition;
            if (FacingRight != artFacesRight)
            {
                local.x = -local.x;
            }

            return transform.TransformPoint(local);
        }

        /// <summary>
        /// The sword flies level in a straight line, the way he faces, so it's easy to read and dodge.
        /// </summary>
        public static Vector2 LaunchVelocity(bool facingRight, float speed) =>
            new Vector2(facingRight ? Mathf.Abs(speed) : -Mathf.Abs(speed), 0f);
    }
}
