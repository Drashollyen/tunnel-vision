using UnityEngine;

namespace TunnelVision.UI
{
    // Stretches a background sprite (keeping its shape) so it always covers the whole camera view,
    // whatever the screen size. Any extra just hangs off the edges.
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundFill : MonoBehaviour
    {
        [Tooltip("Optional. Uses the main camera if left empty.")]
        [SerializeField] private Camera targetCamera;

        private SpriteRenderer _renderer;

        // How much to scale a sprite so it covers the view. Both sizes are in world units.
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

        // Every frame, in case the window is resized or the camera moves.
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
