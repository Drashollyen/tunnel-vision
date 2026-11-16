using System.Collections;
using System.Collections.Generic;
using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public Rigidbody2D rb;
        public float moveSpeed = 5f;

        float horizontalMovement;

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
        [Tooltip("The body's Animator. Optional: without one the body just doesn't animate.")]
        [SerializeField] private Animator bodyAnimator;

        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private static readonly int GroundedParam = Animator.StringToHash("Grounded");
        private static readonly int DeadParam = Animator.StringToHash("Dead");

        [Header("Looking")]
        [SerializeField] private Transform eyeball;
        [SerializeField] private float eyeTurnSpeed = 360f;
        private float targetAngle = 0f;

        private bool facingRight = true;
        private Vector2 lookInput;
        private bool controlsEnabled = true;
        private Quaternion _startEyeRotation = Quaternion.identity;
        private float _autoWalk;

        // How far ahead of the feet auto-walking checks for ground before taking a step.
        private const float EdgeLookAhead = 0.6f;

        /// <summary>True while the player is walking by themselves (at the end of a level).</summary>
        public bool IsAutoWalking => _autoWalk != 0f;

        /// <summary>True while the body faces right.</summary>
        public bool FacingRight => facingRight;

        /// <summary>
        /// The smoothing the player's Rigidbody should use. On their own, Interpolate, so they glide
        /// between physics steps like the camera does instead of shuddering. While riding something
        /// (a wheel seat or moving platform makes the player its child) it's off, since the ride
        /// already moves them smoothly and the two would fight.
        /// </summary>
        public static RigidbodyInterpolation2D SmoothingFor(bool riding) =>
            riding ? RigidbodyInterpolation2D.None : RigidbodyInterpolation2D.Interpolate;

        void Awake()
        {
            // How the player faces at the start of the level, to come back to on every respawn.
            if (eyeball != null)
            {
                _startEyeRotation = eyeball.localRotation;
            }

            UpdateSmoothing();
        }

        void Update()
        {
            UpdateSmoothing();
            UpdateAnimator();
        }

        // Walk while moving sideways, jump pose while off the ground.
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

        /// <summary>Shows the death pose (true) or puts the body back to normal (false).</summary>
        public void SetDead(bool dead)
        {
            if (bodyAnimator != null && bodyAnimator.runtimeAnimatorController != null)
            {
                bodyAnimator.SetBool(DeadParam, dead);
            }
        }

        // Rides parent the player to the moving thing, and stepping off unparents them.
        private void UpdateSmoothing()
        {
            if (rb == null)
            {
                return;
            }

            var wanted = SmoothingFor(transform.parent != null);
            if (rb.interpolation != wanted)
            {
                rb.interpolation = wanted;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
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



            // If the player is on the ground when the jump button is pressed
            if (context.performed && IsGrounded())
            {
                jumpSound.PlayIfSet();
                rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            }

            // If the player releases the jump button while in mid-air
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

        // Improved the bool checks since we were returning both true and false in the if statement
        private bool IsGrounded()
        {
            return Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0f, groundLayer) != null
                || IsRestingOnGround();
        }

        // The ground check box is narrower than the body, so on the very corner of a platform the body
        // can be held up while the box misses. Count any ground pushing up on the body as standing too
        // (walls push sideways, so they don't count).
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

        // How far from flat (in degrees) a surface can be and still count as standing on it.
        private const float MaxStandingSlope = 45f;

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

        private void UpdateLooking()
        {
            if (eyeball == null)
            {
                return;
            }

            if (lookInput.sqrMagnitude > 0.01f)
            {
                // Convert radians to degrees
                targetAngle = Mathf.Atan2(
                    lookInput.y,
                    lookInput.x
                ) * Mathf.Rad2Deg;
            }

            float currentAngle = eyeball.localEulerAngles.z;

            targetAngle = Mathf.MoveTowardsAngle(
                currentAngle,
                targetAngle,
                eyeTurnSpeed * Time.deltaTime
            );

            // Converts angles into Unity-ready values
            eyeball.localRotation = Quaternion.Euler(
                0f, 0f, targetAngle
            );

        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        }

        /// <summary>
        /// Turns the body and the eye back to how they faced at the start of the level (the body
        /// facing right), and forgets any held direction. Used on every respawn.
        /// </summary>
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

        /// <summary>
        /// Walks the player on by themselves in <paramref name="direction"/> (1 right, -1 left),
        /// ignoring input, and stopping at the edge of whatever they're standing on so they never
        /// walk off it. 0 stops. Used as the screen fades out at the end of a level.
        /// </summary>
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

        /// <summary>True if there's ground just ahead of the feet in the given direction.</summary>
        public bool HasGroundAhead(float direction)
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
