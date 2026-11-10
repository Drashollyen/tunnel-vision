using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    /// <summary>
    /// Points the intro screen's cone at the mouse. The cone in levels is driven by PlayerMovement.
    /// </summary>
    public class VisionConeController : MonoBehaviour
    {
        [SerializeField] private float maxAngle = 60f;
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

            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

            float target = Mathf.Clamp(angle, -maxAngle, maxAngle);

            _currentAngle = Mathf.LerpAngle(_currentAngle, target, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

            transform.rotation = Quaternion.Euler(0f, 0f, -_currentAngle);
        }
    }
}
