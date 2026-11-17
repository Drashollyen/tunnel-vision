using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    // Turns the main menu's vision cone to follow the mouse. In levels, PlayerMovement turns the cone instead.
    public class VisionConeController : MonoBehaviour
    {
        [Tooltip("How far the cone can turn either side of straight up, in degrees.")]
        [SerializeField] private float maxAngle = 60f;
        [Tooltip("How quickly the cone catches up with the mouse.")]
        [SerializeField] private float followSpeed = 6f;
        private float _currentAngle;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            Vector2 mouseOnScreen = Mouse.current.position.ReadValue();
            Vector3 mouseInWorld = _mainCamera.ScreenToWorldPoint(mouseOnScreen);
            Vector3 direction = mouseInWorld - transform.position;

            // Angle from straight up to the mouse (x and y swapped on purpose, so 0 means up).
            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            float target = Mathf.Clamp(angle, -maxAngle, maxAngle);

            // Move part of the way towards the target each frame, so the cone glides smoothly
            // and at the same speed whatever the frame rate.
            _currentAngle = Mathf.LerpAngle(_currentAngle, target, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

            transform.rotation = Quaternion.Euler(0f, 0f, -_currentAngle);
        }
    }
}
