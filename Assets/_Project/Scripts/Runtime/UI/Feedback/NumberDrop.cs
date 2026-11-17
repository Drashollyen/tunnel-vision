using TMPro;
using UnityEngine;

namespace TunnelVision.UI
{
    // Shows a line that starts with a number ("2 Lives Remaining") and animates a change: the
    // old number drops away and fades, then the new one drops into place, while the rest of the
    // line stays still. Goes on the text's object, or is added by code.
    [RequireComponent(typeof(TMP_Text))]
    public class NumberDrop : MonoBehaviour
    {
        public const float Pause = 0.25f;
        public const float FallSeconds = 0.3f;
        public const float Overlap = 0.1f;
        private const float FallDistance = 40f;

        // TextMeshPro tag that makes the text after it invisible (but still takes up space).
        private const string Hidden = "<alpha=#00>";

        private TMP_Text _textCache;
        private TMP_Text _oldNumber;
        private TMP_Text _newNumber;
        private float _startedAt = float.NegativeInfinity;

        // The full line, as it reads once the animation has finished.
        public string Line { get; private set; } = string.Empty;

        public bool IsAnimating => Time.unscaledTime - _startedAt < TotalSeconds;

        public static float TotalSeconds => Pause + FallSeconds * 2f - Overlap;

        // Looked up when first needed, since this may be added to an object that's switched off.
        private TMP_Text Text => _textCache != null ? _textCache : _textCache = GetComponent<TMP_Text>();

        public void Play(string oldLine, string newLine)
        {
            Line = newLine;
            var (oldNumber, _) = Split(oldLine);
            var (newNumber, rest) = Split(newLine);

            // The line itself, with its number there but invisible, so nothing else shifts.
            Text.text = Hidden + newNumber + "<alpha=#FF>" + rest;

            _oldNumber = _oldNumber != null ? _oldNumber : MakeNumber("Old Number");
            _newNumber = _newNumber != null ? _newNumber : MakeNumber("New Number");

            // Each overlay shows only its number, laid out exactly like the full line.
            _oldNumber.text = oldNumber + Hidden + Split(oldLine).rest;
            _newNumber.text = newNumber + Hidden + rest;

            _startedAt = Time.unscaledTime;
            Animate(0f);
        }

        // Shows a line straight away, with no number animation.
        public void ShowPlain(string line)
        {
            Line = line;
            Text.text = line;
            _startedAt = float.NegativeInfinity;
            if (_oldNumber != null)
                _oldNumber.gameObject.SetActive(false);
            if (_newNumber != null)
                _newNumber.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_oldNumber == null || !_oldNumber.gameObject.activeSelf && !_newNumber.gameObject.activeSelf)
            {
                return;
            }

            Animate(Time.unscaledTime - _startedAt);
        }

        private void Animate(float elapsed)
        {
            // The old number waits, then falls and fades.
            var fall = Mathf.Clamp01((elapsed - Pause) / FallSeconds);
            Place(_oldNumber, -FallDistance * Ease(fall), 1f - fall, fall < 1f);

            // The new number starts just before the old one has gone, from above.
            var arrive = Mathf.Clamp01((elapsed - Pause - FallSeconds + Overlap) / FallSeconds);
            Place(_newNumber, FallDistance * (1f - Ease(arrive)), arrive, true);

            if (arrive >= 1f)
            {
                // Settled: the line shows its own number again and the overlays go.
                Text.text = Line;
                _oldNumber.gameObject.SetActive(false);
                _newNumber.gameObject.SetActive(false);
            }
        }

        private static float Ease(float t) => t * t * (3f - 2f * t);

        private static void Place(TMP_Text number, float y, float alpha, bool visible)
        {
            number.gameObject.SetActive(visible);
            number.rectTransform.anchoredPosition = new Vector2(0f, y);
            number.alpha = alpha;
        }

        // Splits "2 Lives Remaining" into "2" and " Lives Remaining".
        public static (string number, string rest) Split(string line)
        {
            var space = line.IndexOf(' ');
            return space < 0 ? (line, string.Empty) : (line.Substring(0, space), line.Substring(space));
        }

        private TMP_Text MakeNumber(string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var number = go.AddComponent<TextMeshProUGUI>();
            number.font = Text.font;
            number.fontSharedMaterial = Text.fontSharedMaterial; // Brings the line's outline with it.
            number.fontSize = Text.fontSize;
            number.fontStyle = Text.fontStyle;
            number.alignment = Text.alignment;
            number.textWrappingMode = Text.textWrappingMode;
            number.color = Text.color;
            number.raycastTarget = false;
            return number;
        }
    }
}
