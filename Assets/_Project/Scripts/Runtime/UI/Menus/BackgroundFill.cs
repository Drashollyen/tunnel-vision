using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Scales a background sprite so it always covers the whole camera view, whatever the screen
    /// shape: it grows until both the width and the height are filled, and anything extra spills
    /// off the edges. Keeps the sprite's own proportions. Follows the camera if the window resizes.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundFill : MonoBehaviour
    {
        [Tooltip("The camera to fill. Left empty, the main camera.")]
        [SerializeField] private Camera targetCamera;

        private SpriteRenderer _renderer;

        /// <summary>
        /// The scale that makes a sprite of <paramref name="spriteSize"/> cover a view of
        /// <paramref name="viewSize"/>, both in world units.
        /// </summary>
        public static float CoverScale(Vector2 spriteSize, Vector2 viewSize)
        {
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return 1f;
            }

            return Mathf.Max(viewSize.x / spriteSize.x, viewSize.y / spriteSize.y);
        }

        private void OnEnable()
        {
            _renderer = GetComponent<SpriteRenderer>();
            Fit();
        }

        private void LateUpdate()
        {
            Fit();
        }

        private void Fit()
        {
            var view = targetCamera != null ? targetCamera : Camera.main;
            if (view == null || !view.orthographic || _renderer == null || _renderer.sprite == null)
            {
                return;
            }

            var height = view.orthographicSize * 2f;
            var viewSize = new Vector2(height * view.aspect, height);
            var scale = CoverScale(_renderer.sprite.bounds.size, viewSize);
            transform.localScale = new Vector3(scale, scale, 1f);

            var centre = view.transform.position;
            transform.position = new Vector3(centre.x, centre.y, transform.position.z);
        }
    }
}
