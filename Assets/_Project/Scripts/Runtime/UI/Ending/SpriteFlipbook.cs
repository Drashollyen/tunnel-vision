using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Plays a list of sprites on a UI Image, round and round, like a simple idle animation
    // without an Animator. Uses real time, so it keeps playing while the game is paused.
    [RequireComponent(typeof(Image))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 10f;

        private Image _image;

        public static int FrameAt(float time, int frameCount, float framesPerSecond)
        {
            if (frameCount <= 0 || framesPerSecond <= 0f)
            {
                return 0;
            }

            return Mathf.FloorToInt(time * framesPerSecond) % frameCount;
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            var frame = frames[FrameAt(Time.unscaledTime, frames.Length, framesPerSecond)];
            if (_image.sprite != frame)
            {
                _image.sprite = frame;
            }
        }
    }
}
