using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Moves a background layer by part of the camera's movement. A factor of 0 stays
    /// fixed in the world, and 1 sticks to the camera, so far layers use higher values.
    /// </summary>
    // Runs after CinemachineBrain (order 100), so it reads this frame's camera position.
    [DefaultExecutionOrder(200)]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float factor = 0.5f;

        private Transform _camera;
        private Vector3 _lastCameraPosition;

        public static Vector3 OffsetFor(Vector3 cameraDelta, float factor)
        {
            return new Vector3(cameraDelta.x * factor, cameraDelta.y * factor, 0f);
        }

        private void Start()
        {
            _camera = Camera.main != null ? Camera.main.transform : null;
            if (_camera != null)
            {
                _lastCameraPosition = _camera.position;
            }
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            transform.position += OffsetFor(_camera.position - _lastCameraPosition, factor);
            _lastCameraPosition = _camera.position;
        }
    }
}
