using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    /// <summary>
    /// The menu's cone of vision: a dark overlay with a cone of light coming from Nicholas's eye.
    /// The cone follows the pointer while it moves and sways gently when it is idle; the pupil looks along the cone.
    /// </summary>
    public sealed class VisionConeBackground : MonoBehaviour
    {
        [Tooltip("Dark overlay with the cone cut out. Rotates around its center, which sits on the eye.")]
        [SerializeField] private RectTransform cone;

        [Tooltip("Iris and pupil, moved inside the eyeball towards where the cone points.")]
        [SerializeField] private RectTransform iris;

        [Tooltip("Furthest the cone turns from straight up, in degrees.")]
        [SerializeField] private float maxAngle = 45f;

        [SerializeField] private float idleSwayAngle = 30f;
        [SerializeField] private float idleSwaySpeed = 0.5f;

        [Tooltip("Seconds after the pointer stops moving before the idle sway resumes.")]
        [SerializeField] private float idleDelay = 2f;

        [SerializeField] private float followSpeed = 6f;
        [SerializeField] private float irisTravel = 30f;

        private float _angle;
        private float _lastPointerMove = float.NegativeInfinity;
        private Vector2 _lastPointer;

        private void Start()
        {
            _angle = -cone.localEulerAngles.z;
            _lastPointer = PointerPosition() ?? Vector2.zero;
        }

        private void Update()
        {
            var target = IdleAngle(Time.unscaledTime);
            var pointer = PointerPosition();
            if (pointer.HasValue)
            {
                if ((pointer.Value - _lastPointer).sqrMagnitude > 1f)
                {
                    _lastPointer = pointer.Value;
                    _lastPointerMove = Time.unscaledTime;
                }

                if (Time.unscaledTime - _lastPointerMove < idleDelay)
                {
                    var apex = RectTransformUtility.WorldToScreenPoint(null, cone.position);
                    target = AngleTowards(apex, pointer.Value, maxAngle);
                }
            }

            _angle = Mathf.LerpAngle(_angle, target, 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime));
            Apply(_angle);
        }

        /// <summary>
        /// Angle in degrees from straight up to the target, clockwise positive, clamped to ±maxAngle.
        /// Targets below the apex clamp to the nearest side.
        /// </summary>
        public static float AngleTowards(Vector2 apex, Vector2 target, float maxAngle)
        {
            var direction = target - apex;
            if (direction.sqrMagnitude < 0.0001f)
            {
                return 0f;
            }

            var angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            return Mathf.Clamp(angle, -maxAngle, maxAngle);
        }

        private float IdleAngle(float time) => idleSwayAngle * Mathf.Sin(time * idleSwaySpeed);

        private void Apply(float angle)
        {
            cone.localRotation = Quaternion.Euler(0f, 0f, -angle);
            var radians = angle * Mathf.Deg2Rad;
            iris.anchoredPosition = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)) * irisTravel;
        }

        private static Vector2? PointerPosition()
        {
            var pointer = Pointer.current;
            return pointer != null ? pointer.position.ReadValue() : (Vector2?)null;
        }
    }
}
