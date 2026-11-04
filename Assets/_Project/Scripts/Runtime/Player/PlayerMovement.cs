using System.Collections;
using System.Collections.Generic;
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

        [Header("GroundCheck")]
        public Transform groundCheckPos;
        public Vector2 groundCheckSize = new Vector2(0.5f, 0.05f);
        public LayerMask groundLayer;

        [Header("Facing")]
        [SerializeField] private SpriteRenderer bodySprite;

        [Header("Looking")]
        [SerializeField] private Transform eyeball;
        [SerializeField] private float eyeTurnSpeed = 360f;
        private float targetAngle = 0f;

        private bool facingRight = true;
        private Vector2 lookInput;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            UpdateFacing();
            UpdateLooking();
        }

        public void Move(InputAction.CallbackContext context)
        {
            horizontalMovement = context.ReadValue<Vector2>().x;
        }

        public void Jump(InputAction.CallbackContext context)
        {
            if (IsGrounded())
            {

                if (context.performed)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
                }
                else if (context.canceled)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * 0.5f);
                }

            }
        }

        public void Look(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        private bool IsGrounded()
        {
            if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
            {
                return true;
            }
            return false;
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

            bodySprite.flipX = !facingRight;
        }

        private void UpdateLooking()
        {

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

    }
}
