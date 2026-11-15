using TMPro;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Gives a TextMeshPro text a dark outline so it stays readable over any background.
    /// Goes on the text's object. The text gets its own copy of the font material.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TextOutline : MonoBehaviour
    {
        [Tooltip("Outline thickness. Above about 0.3 the letters start to clip with this font.")]
        [SerializeField, Range(0f, 0.3f)] private float width = 0.2f;
        [SerializeField] private Color color = Color.black;

        public float Width => width;
        public Color Color => color;

        private void Awake()
        {
            Apply();
        }

        private void OnValidate()
        {
            // Only while playing: in the editor, this would save a material copy into the asset.
            if (Application.isPlaying)
            {
                Apply();
            }
        }

        private void Apply()
        {
            var text = GetComponent<TMP_Text>();
            text.outlineWidth = width;
            text.outlineColor = color;
        }
    }
}
