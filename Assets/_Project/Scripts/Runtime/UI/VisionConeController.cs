using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    public class VisionConeController : MonoBehaviour
    {
        [SerializeField] private float maxAngle = 60f;
        [SerializeField] private float followSpeed = 6f;
        private float currentAngle;
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (Mouse.current == null)
                return;

            Vector2 mouseOnScreen = Mouse.current.position.ReadValue();

            Vector3 mouseInWorld = mainCamera.ScreenToWorldPoint(mouseOnScreen);

            Vector3 direction = mouseInWorld - transform.position;

            float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;

            float target = Mathf.Clamp(angle, -maxAngle, maxAngle);

            currentAngle = Mathf.LerpAngle(currentAngle, target, followSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, 0f, -currentAngle);
        }
    }
}
