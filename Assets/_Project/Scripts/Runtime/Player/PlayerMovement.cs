using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public Rigidbody2D rb;
        public float moveSpeed = 5f;

        private float horizontalMovement;

        [Header("Jumping")]
        public float jumpPower = 10f;
        [SerializeField] private AudioSource jumpSound;

        [Header("GroundCheck")]
        public Transform groundCheckPos;
        public Vector2 groundCheckSize = new Vector2(0.5f, 0.5f);
        public LayerMask groundLayer;

        [Header("Facing")]
        [SerializeField] private SpriteRenderer bodySprite;

        [Header("Animation")]
        [Tooltip("Optional. Without one the body just doesn't animate.")]
        [SerializeField] private Animator bodyAnimator;

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");
        private static readonly int DeadParam = Animator.StringToHash("Dead");

        [Header("Looking")]
        [SerializeField] private Transform eyeball;
        [SerializeField] private float eyeTurnSpeed = 360f;
        private float targetAngle;

        private bool facingRight = true;
        private Vector2 lookInput;
        private bool controlsEnabled = true;
        private Quaternion _startEyeRotation = Quaternion.identity;
        private float _autoWalk;

        // How far ahead of the feet auto-walking checks for ground before taking a step.
        private const float EdgeLookAhead = 0.6f;

        // How far from flat (in degrees) a surface can be and still count as standing on it.
        private const float MaxStandingSlope = 45f;

        // True while the player walks off by themselves at the end of a level.
        public bool IsAutoWalking => _autoWalk != 0f;

        public bool FacingRight => facingRight;

        private void Awake()
        {
            // Remember where the eye points at the start, to reset it on every respawn.
            if (eyeball != null)
            {
                _startEyeRotation = eyeball.localRotation;
            }

            UpdateSmoothing();
        }

        private void Update()
        {
            UpdateSmoothing();
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            if (bodyAnimator == null || bodyAnimator.runtimeAnimatorController == null || rb == null)
            {
                return;
            }

            var speed = rb.simulated ? Mathf.Abs(rb.linearVelocity.x) : 0f;
            bodyAnimator.SetFloat(SpeedParam, speed);
            bodyAnimator.SetBool(GroundedParam, !rb.simulated || groundCheckPos == null || IsGrounded());
        }

        // Shows the death pose (true) or puts the body back to normal (false).
        public void SetDead(bool dead)
        {
            if (bodyAnimator != null && bodyAnimator.runtimeAnimatorController != null)
            {
                bodyAnimator.SetBool(DeadParam, dead);
            }
        }

        // Normally the Rigidbody smooths its movement between physics steps so the player doesn't
        // shudder. While riding something (a wheel seat or moving platform makes the player its
        // child) smoothing is turned off, because the ride already moves them and the two would fight.
        private void UpdateSmoothing()
        {
            if (rb == null)
            {
                return;
            }

            var wanted = transform.parent != null
                ? RigidbodyInterpolation2D.None
                : RigidbodyInterpolation2D.Interpolate;
            if (rb.interpolation != wanted)
            {
                rb.interpolation = wanted;
            }
        }

        private void FixedUpdate()
        {
            if (_autoWalk != 0f)
            {
                StepAutoWalk();
                return;
            }

            if (!controlsEnabled)
            { return; }

            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            UpdateFacing();
            UpdateLooking();
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (!controlsEnabled)
            { return; }
            horizontalMovement = context.ReadValue<Vector2>().x;
        }

        public void Jump(InputAction.CallbackContext context)
        {
            if (!controlsEnabled)
            { return; }

            if (context.performed && IsGrounded())
            {
                jumpSound.PlayIfSet();
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            }
            // Letting go of jump early cuts the jump short.
            else if (context.canceled && rb.linearVelocityY > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
            }
        }

        public void Look(InputAction.CallbackContext context)
        {
            if (!controlsEnabled)
            { return; }
            lookInput = context.ReadValue<Vector2>();
        }

        private bool IsGrounded()
        {
            return Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer) != null
                || IsRestingOnGround();
        }

        // The ground check box is narrower than the body, so on the very corner of a platform it can
        // miss. This also counts any ground pushing up on the body as standing (walls push sideways,
        // so they don't count).
        private bool IsRestingOnGround()
        {
            if (rb == null || !rb.simulated)
            {
                return false;
            }

            var filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = groundLayer,
                useNormalAngle = true,
                minNormalAngle = 90f - MaxStandingSlope,
                maxNormalAngle = 90f + MaxStandingSlope,
            };
            return rb.IsTouching(filter);
        }

        private void UpdateFacing()
        {
            if (horizontalMovement > 0.01f)
            {
                facingRight = true;
            }
            else if (horizontalMovement < -0.01f)
            {
                facingRight = false;
            }

            if (bodySprite != null)
            {
                bodySprite.flipX = !facingRight;
            }
        }

        // Turns the eye smoothly towards the direction the look stick is pointing.
        private void UpdateLooking()
        {
            if (eyeball == null)
            {
                return;
            }

            if (lookInput.sqrMagnitude > 0.01f)
            {
                targetAngle = Mathf.Atan2(lookInput.y, lookInput.x) * Mathf.Rad2Deg;
            }

            float currentAngle = eyeball.localEulerAngles.z;
            targetAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, eyeTurnSpeed * Time.deltaTime);
            eyeball.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPos == null)
            {
                return;
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }

        // Faces the body right and the eye back to how it started, and forgets any held input.
        // Used on every respawn.
        public void FaceStartDirection()
        {
            facingRight = true;
            horizontalMovement = 0f;
            lookInput = Vector2.zero;

            if (bodySprite != null)
            {
                bodySprite.flipX = false;
            }

            if (eyeball != null)
            {
                eyeball.localRotation = _startEyeRotation;
                targetAngle = _startEyeRotation.eulerAngles.z;
            }
        }

        // Makes the player walk by themselves at the end of a level (1 = right, -1 = left, 0 = stop).
        // Input is ignored, and they stop at the edge of whatever they're standing on.
        public void AutoWalk(float direction)
        {
            _autoWalk = Mathf.Sign(direction) * (direction == 0f ? 0f : 1f);
            horizontalMovement = 0f;
            lookInput = Vector2.zero;

            if (_autoWalk == 0f && rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        private bool HasGroundAhead(float direction)
        {
            if (groundCheckPos == null)
            {
                return false;
            }

            var ahead = (Vector2)groundCheckPos.position + Vector2.right * Mathf.Sign(direction) * EdgeLookAhead;
            return Physics2D.OverlapBox(ahead, groundCheckSize, 0f, groundLayer) != null;
        }

        private void StepAutoWalk()
        {
            facingRight = _autoWalk > 0f;
            if (bodySprite != null)
            {
                bodySprite.flipX = !facingRight;
            }

            // At the edge, stand still rather than walk off.
            var speed = HasGroundAhead(_autoWalk) ? _autoWalk * moveSpeed : 0f;
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }

        public void EnableControls(bool value)
        {
            controlsEnabled = value;

            horizontalMovement = 0f;
            lookInput = Vector2.zero;
        }
    }
}
