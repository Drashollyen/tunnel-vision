using System;
using System.Collections;
using TMPro;
using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// The hidden Chaplin on the Demo Complete screen. Clicking him fades the music out, turns the
    /// confetti black, cuts the lights and plays his clip. Then a dark message from him counts
    /// down, and at zero the game closes (in the Editor, Play mode stops).
    /// <para>
    /// Goes on Chaplin's Image, which must be a raycast target. The black screen and the message
    /// are made by code on their own canvas, above everything else. Their colours, font and
    /// timings are fields here, so they can be tuned in the Inspector. Real time throughout.
    /// </para>
    /// </summary>
    public class ChaplinEasterEgg : MonoBehaviour, IPointerClickHandler
    {
        public enum Stage
        {
            Waiting,
            MusicFading,
            LightsOut,
            Voice,
            Countdown,
            Closing,
        }

        public const string HeaderText = "Chaplin, The Sword Swallower";

        [Header("Wiring")]
        [SerializeField] private AudioClip voice;
        [SerializeField] private ConfettiRain confetti;
        [SerializeField] private TMP_FontAsset font;

        [Header("Timings (Seconds)")]
        [SerializeField] private float musicFadeSeconds = 1.5f;
        [SerializeField] private float lightsOutSeconds = 0.8f;
        [SerializeField] private float darkBeforeVoice = 0.6f;
        [SerializeField] private float darkAfterVoice = 0.8f;
        [SerializeField] private int countdownFrom = 10;

        [Header("Look")]
        [SerializeField] private Color confettiColour = Color.black;
        [SerializeField] private Color boxColour = new Color(0.07f, 0f, 0f, 0.98f);
        [SerializeField] private Color borderColour = new Color(0.45f, 0f, 0.02f, 1f);
        [SerializeField] private Color headerColour = new Color(0.62f, 0.04f, 0.04f, 1f);
        [SerializeField] private Color messageColour = new Color(0.66f, 0.58f, 0.58f, 1f);
        [SerializeField] private Color countdownColour = new Color(0.8f, 0f, 0f, 1f);
        [SerializeField] private float shakePixels = 3f;

        /// <summary>What closes the game at zero. Tests swap it so the test run isn't stopped.</summary>
        public static Action QuitAction = QuitGame;

        private Image _lightsOut;
        private GameObject _box;
        private TMP_Text _message;
        private TMP_Text _number;
        private AudioSource _voiceSource;

        public Stage Current { get; private set; } = Stage.Waiting;
        public AudioClip Voice => voice;
        public ConfettiRain Confetti => confetti;
        public TMP_FontAsset FontAsset => font;
        public int CountdownFrom => countdownFrom;

        /// <summary>The message box, once it's showing.</summary>
        public GameObject MessageBox => _box;

        /// <summary>The message text, once it's showing.</summary>
        public TMP_Text MessageText => _message;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetQuit()
        {
            // Fast Enter Play Mode keeps statics, so a swapped quit from a test can't linger.
            QuitAction = QuitGame;
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>The whole number shown on the countdown, <paramref name="elapsed"/> seconds in. Never below zero.</summary>
        public static int CountdownAt(float elapsed, int from)
        {
            return Mathf.Max(0, from - Mathf.FloorToInt(Mathf.Max(0f, elapsed)));
        }

        /// <summary>Chaplin's message. The countdown itself is the big red number underneath.</summary>
        public const string Message =
            "You Laughed. You Clapped. You Thought The Show Was Over.\n\n"
            + "I Have Swallowed Every Blade In This Tent, Nicholas.\n"
            + "I Swallowed The Lights. I Swallowed The Music.\n"
            + "And Now I Will Swallow You.\n\n"
            + "The Tent Comes Down In...";

        /// <summary>
        /// How dark the lights-out screen is, <paramref name="t"/> seconds into a cut lasting
        /// <paramref name="seconds"/>: the lights flicker a few times, then stay off.
        /// </summary>
        public static float LightsOutAlpha(float t, float seconds)
        {
            if (seconds <= 0f || t >= seconds)
            {
                return 1f;
            }

            if (t <= 0f)
            {
                return 0f;
            }

            // Off and on in uneven steps, like a failing bulb, ending off.
            var step = Mathf.FloorToInt(t / seconds * 7f);
            return step == 1 || step == 3 || step >= 5 ? 1f : 0f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Play();
        }

        /// <summary>Starts the sequence. Does nothing if it's already running.</summary>
        public void Play()
        {
            if (Current != Stage.Waiting)
            {
                return;
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            BuildOverlay();

            Current = Stage.MusicFading;
            if (MusicPlayer.Current != null)
            {
                MusicPlayer.Current.FadeOut(musicFadeSeconds);
            }

            if (confetti != null)
            {
                confetti.FadeTo(confettiColour, musicFadeSeconds);
            }

            yield return new WaitForSecondsRealtime(musicFadeSeconds);

            Current = Stage.LightsOut;
            for (var t = 0f; t < lightsOutSeconds; t += Time.unscaledDeltaTime)
            {
                SetDark(LightsOutAlpha(t, lightsOutSeconds));
                yield return null;
            }

            SetDark(1f);
            yield return new WaitForSecondsRealtime(darkBeforeVoice);

            Current = Stage.Voice;
            if (voice != null)
            {
                _voiceSource.clip = voice;
                _voiceSource.Play();
                yield return new WaitForSecondsRealtime(voice.length);
            }

            yield return new WaitForSecondsRealtime(darkAfterVoice);

            Current = Stage.Countdown;
            _message.text = Message;
            _box.SetActive(true);
            var shown = -1;
            for (var elapsed = 0f; ; elapsed += Time.unscaledDeltaTime)
            {
                var left = CountdownAt(elapsed, countdownFrom);
                if (left != shown)
                {
                    shown = left;
                    _number.text = left.ToString();
                }

                Shake();
                if (left <= 0)
                {
                    break;
                }

                yield return null;
            }

            // Let the zero land before it all goes.
            yield return new WaitForSecondsRealtime(0.4f);
            Current = Stage.Closing;
            QuitAction?.Invoke();
        }

        private void SetDark(float alpha)
        {
            var colour = _lightsOut.color;
            colour.a = alpha;
            _lightsOut.color = colour;
        }

        private void Shake()
        {
            var box = (RectTransform)_box.transform;
            box.anchoredPosition = UnityEngine.Random.insideUnitCircle * shakePixels;
        }

        private void BuildOverlay()
        {
            var root = new GameObject("Easter Egg Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = gameObject.layer;
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            _voiceSource = root.AddComponent<AudioSource>();
            _voiceSource.playOnAwake = false;
            _voiceSource.loop = false;
            _voiceSource.spatialBlend = 0f;
            SoundEffectsVolume.Exclude(_voiceSource);

            // Blocks clicks from here on, so the Main Menu button can't cut the show short.
            _lightsOut = MakeImage("Lights Out", root.transform, new Color(0f, 0f, 0f, 0f));
            Stretch((RectTransform)_lightsOut.transform);
            _lightsOut.raycastTarget = true;

            var border = MakeImage("Scary Message", root.transform, borderColour);
            _box = border.gameObject;
            var boxRect = (RectTransform)_box.transform;
            boxRect.sizeDelta = new Vector2(1180f, 620f);

            var inside = MakeImage("Inside", boxRect, boxColour);
            Stretch((RectTransform)inside.transform, 8f);

            var header = MakeText("Header", boxRect, 54f, headerColour);
            header.text = HeaderText;
            Place(header.rectTransform, new Vector2(0f, 240f), new Vector2(1080f, 80f));

            _message = MakeText("Message", boxRect, 36f, messageColour);
            Place(_message.rectTransform, new Vector2(0f, 10f), new Vector2(1060f, 330f));

            _number = MakeText("Countdown", boxRect, 110f, countdownColour);
            Place(_number.rectTransform, new Vector2(0f, -225f), new Vector2(400f, 130f));

            _box.SetActive(false);
        }

        private static Image MakeImage(string name, Transform parent, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = colour;
            image.raycastTarget = false;
            return image;
        }

        private TMP_Text MakeText(string name, Transform parent, float size, Color colour)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<TextMeshProUGUI>();
            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = size;
            text.color = colour;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect, float inset = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
