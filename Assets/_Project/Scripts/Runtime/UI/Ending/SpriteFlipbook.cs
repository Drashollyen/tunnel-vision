using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Plays a list of sprites on a UI Image, round and round: a character's idle animation on a
    /// screen, without an Animator. Uses real time, so it plays whatever the game's time scale.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float framesPerSecond = 10f;

        private Image _image;

        /// <summary>Which frame shows this long into the loop.</summary>
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
