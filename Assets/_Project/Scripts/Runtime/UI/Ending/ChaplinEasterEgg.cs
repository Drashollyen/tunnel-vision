using System;
using System.Collections;
using System.Collections.Generic;
using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TunnelVision.UI
{
    /// <summary>
    /// The hidden Chaplin on the Demo Complete screen. Press F or click him: the music fades and
    /// the confetti turns black, the screen glitches out to black, his "Tunnel Vision" clip plays
    /// in the dark, then a torch searches the screen, finds him, closes in on his face, and the
    /// game closes (in the Editor, Play mode stops).
    /// <para>
    /// Goes on Chaplin's Image, which must be a raycast target. The black screen, glitch, torch
    /// and close-up are made by code on their own canvas above everything else; timings and
    /// sizes are fields here, so they can be tuned in the Inspector. Real time throughout.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ChaplinEasterEgg : MonoBehaviour, IPointerClickHandler
    {
        public enum Stage
        {
            Waiting,
            MusicFading,
            Glitch,
            Voice,
            Torch,
            CloseUp,
            Closing,
        }

        [Header("Wiring")]
        [SerializeField] private AudioClip voice;
        [Tooltip("Optional. Left empty, a glitch sound is made by code.")]
        [SerializeField] private AudioClip glitchSound;
        [SerializeField] private ConfettiRain confetti;
        [Tooltip("The cast line-up. Everyone in it freezes when the lights go.")]
        [SerializeField] private Transform cast;

        [Header("Timings (Seconds)")]
        [SerializeField] private float musicFadeSeconds = 1.5f;
        [SerializeField] private float glitchSeconds = 1.2f;
        [SerializeField] private float darkBeforeVoice = 0.4f;
        [SerializeField] private float darkAfterVoice = 0.5f;
        [SerializeField] private float torchSweepSeconds = 4f;
        [SerializeField] private float torchLockSeconds = 0.8f;
        [SerializeField] private float closeUpSeconds = 1.2f;
        [SerializeField] private float closeUpHoldSeconds = 1.6f;

        [Header("Look")]
        [SerializeField] private Color confettiColour = Color.black;
        [Tooltip("Where his face is on the sprite, from 0 to 1 across and up.")]
        [SerializeField] private Vector2 faceOnSprite = new Vector2(0.45f, 0.81f);
        [Tooltip("How tall his face is, as a share of the sprite's height.")]
        [SerializeField] private float faceHeightOnSprite = 0.15f;
        [Tooltip("How much of the screen's height his face fills in the close-up.")]
        [SerializeField] private float closeUpFaceHeight = 0.6f;
        [SerializeField] private float torchRadius = 170f;

        /// <summary>What closes the game at the end. Tests swap it so the test run isn't stopped.</summary>
        public static Action QuitAction = QuitGame;

        // Torch texture: a soft hole in black. The image is this many hole-widths across, so it
        // still covers the screen wherever the hole goes.
        private const int TorchTexture = 768;
        private const float TorchSpan = 12f;
        private const int GlitchBars = 14;
        private const float GlitchStepSeconds = 0.04f;

        private Image _image;
        private Sprite _idle;
        private RectTransform _overlay;
        private Image _black;
        private RectTransform _torch;
        private RectTransform _closeUp;
        private readonly List<Image> _bars = new List<Image>();
        private AudioSource _audio;
        private Texture2D _torchTexture;

        public Stage Current { get; private set; } = Stage.Waiting;
        public AudioClip Voice => voice;
        public ConfettiRain Confetti => confetti;
        public Transform Cast => cast;

        /// <summary>Chaplin's close-up, once the torch is out.</summary>
        public RectTransform CloseUp => _closeUp;

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

        /// <summary>
        /// How dark the screen is, <paramref name="t"/> seconds into a cut lasting
        /// <paramref name="seconds"/>: it stutters off and on a few times, then stays black.
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

        /// <summary>
        /// Where the torch is <paramref name="t"/> (0 to 1) of the way through its search: it
        /// eases from spot to spot, pausing briefly at each, like someone looking around.
        /// </summary>
        public static Vector2 SweepPoint(float t, IList<Vector2> points)
        {
            if (points == null || points.Count == 0)
            {
                return Vector2.zero;
            }

            if (points.Count == 1 || t <= 0f)
            {
                return points[0];
            }

            if (t >= 1f)
            {
                return points[points.Count - 1];
            }

            var scaled = t * (points.Count - 1);
            var index = Mathf.FloorToInt(scaled);
            var within = Mathf.SmoothStep(0f, 1f, scaled - index);
            return Vector2.Lerp(points[index], points[index + 1], within);
        }

        /// <summary>
        /// The part of <paramref name="rect"/> a sprite of <paramref name="spriteSize"/> fills
        /// with Preserve Aspect on, lined up by <paramref name="pivot"/> like the Image does.
        /// </summary>
        public static Rect DrawnRect(Rect rect, Vector2 pivot, Vector2 spriteSize)
        {
            if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            {
                return rect;
            }

            var scale = Mathf.Min(rect.width / spriteSize.x, rect.height / spriteSize.y);
            var size = spriteSize * scale;
            var x = rect.x + (rect.width - size.x) * pivot.x;
            var y = rect.y + (rect.height - size.y) * pivot.y;
            return new Rect(x, y, size.x, size.y);
        }

        /// <summary>
        /// A burst of digital glitching: short random stretches of noise, crushed buzzing and
        /// dropouts. The same <paramref name="seed"/> always makes the same sound.
        /// </summary>
        public static float[] GlitchSamples(int sampleRate, float seconds, int seed)
        {
            var count = Mathf.Max(0, Mathf.RoundToInt(sampleRate * seconds));
            var samples = new float[count];
            var random = new System.Random(seed);
            var i = 0;
            while (i < count)
            {
                var length = Mathf.Clamp((int)(sampleRate * (0.02 + random.NextDouble() * 0.07)), 1, count - i);
                var kind = random.Next(4);
                var frequency = 80.0 + random.NextDouble() * 1100.0;
                var hold = 1 + random.Next(40);
                var held = 0f;
                var loudness = 0.35f + (float)random.NextDouble() * 0.45f;
                for (var j = 0; j < length; j++, i++)
                {
                    float value;
                    switch (kind)
                    {
                        case 0: // Static.
                            value = (float)(random.NextDouble() * 2.0 - 1.0);
                            break;
                        case 1: // Square buzz.
                            value = Math.Sin(2.0 * Math.PI * frequency * j / sampleRate) >= 0.0 ? 1f : -1f;
                            break;
                        case 2: // Dropout.
                            value = 0f;
                            break;
                        default: // Crushed noise: holds each value for a while.
                            if (j % hold == 0)
                            {
                                held = (float)(random.NextDouble() * 2.0 - 1.0);
                            }

                            value = held;
                            break;
                    }

                    samples[i] = value * loudness;
                }
            }

            return samples;
        }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _idle = _image.sprite;
        }

        private void OnDestroy()
        {
            if (_torchTexture != null)
            {
                Destroy(_torchTexture);
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (Current == Stage.Waiting && keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                Play();
            }
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

            Current = Stage.MusicFading;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            BuildOverlay();

            // The happy music fades and the confetti turns black.
            if (MusicPlayer.Current != null)
            {
                MusicPlayer.Current.FadeOut(musicFadeSeconds);
            }

            if (confetti != null)
            {
                confetti.FadeTo(confettiColour, musicFadeSeconds);
            }

            yield return new WaitForSecondsRealtime(musicFadeSeconds);

            // The screen glitches out to black.
            Current = Stage.Glitch;
            _audio.PlayOneShot(glitchSound != null ? glitchSound : MakeGlitchClip(glitchSeconds));
            var nextGlitch = 0f;
            for (var t = 0f; t < glitchSeconds; t += Time.unscaledDeltaTime)
            {
                SetBlack(LightsOutAlpha(t, glitchSeconds));
                if (t >= nextGlitch)
                {
                    ScrambleBars();
                    nextGlitch = t + GlitchStepSeconds;
                }

                yield return null;
            }

            HideBars();
            SetBlack(1f);
            FreezeCast();

            // His clip, in the dark.
            yield return new WaitForSecondsRealtime(darkBeforeVoice);
            Current = Stage.Voice;
            if (voice != null)
            {
                _audio.PlayOneShot(voice);
                yield return new WaitForSecondsRealtime(voice.length);
            }

            yield return new WaitForSecondsRealtime(darkAfterVoice);

            // A torch searches the screen and finds him.
            Current = Stage.Torch;
            var face = MakeCloseUp(out var faceOffset, out var faceHeight);
            _torch.gameObject.SetActive(true);
            SetBlack(0f);
            var size = _overlay.rect.size;
            var path = new[]
            {
                new Vector2(size.x * 0.42f, size.y * 0.3f),
                new Vector2(size.x * 0.12f, size.y * -0.18f),
                new Vector2(size.x * -0.1f, size.y * 0.26f),
                new Vector2(size.x * 0.3f, size.y * -0.05f),
                new Vector2(size.x * -0.42f, size.y * 0.32f),
                face,
            };
            for (var t = 0f; t < torchSweepSeconds; t += Time.unscaledDeltaTime)
            {
                PlaceTorch(SweepPoint(t / torchSweepSeconds, path), torchRadius);
                yield return null;
            }

            for (var t = 0f; t < torchLockSeconds; t += Time.unscaledDeltaTime)
            {
                PlaceTorch(face, torchRadius);
                yield return null;
            }

            // Closes in on his face.
            Current = Stage.CloseUp;
            var scale = faceHeight > 0f ? closeUpFaceHeight * size.y / faceHeight : 1f;
            var closeRadius = closeUpFaceHeight * size.y * 0.75f;
            for (var t = 0f; ; t += Time.unscaledDeltaTime)
            {
                var e = Mathf.SmoothStep(0f, 1f, closeUpSeconds > 0f ? t / closeUpSeconds : 1f);
                var s = Mathf.Lerp(1f, scale, e);
                var centre = Vector2.Lerp(face, Vector2.zero, e);
                _closeUp.localScale = new Vector3(s, s, 1f);
                _closeUp.anchoredPosition = centre - faceOffset * s;
                PlaceTorch(centre, Mathf.Lerp(torchRadius, closeRadius, e));
                if (e >= 1f)
                {
                    break;
                }

                yield return null;
            }

            for (var t = 0f; t < closeUpHoldSeconds; t += Time.unscaledDeltaTime)
            {
                PlaceTorch(Vector2.zero, closeRadius);
                yield return null;
            }

            Current = Stage.Closing;
            QuitAction?.Invoke();
        }

        private void SetBlack(float alpha)
        {
            _black.color = new Color(0f, 0f, 0f, alpha);
        }

        private void ScrambleBars()
        {
            var size = _overlay.rect.size;
            foreach (var bar in _bars)
            {
                var on = Random.value < 0.6f;
                bar.enabled = on;
                if (!on)
                {
                    continue;
                }

                var rect = bar.rectTransform;
                var width = Random.value < 0.5f ? size.x : size.x * Random.Range(0.2f, 0.8f);
                rect.sizeDelta = new Vector2(width, Random.Range(6f, 90f));
                rect.anchoredPosition = new Vector2(Random.Range(-size.x, size.x) * 0.3f, Random.Range(-size.y, size.y) * 0.5f);
                switch (Random.Range(0, 5))
                {
                    case 0:
                        bar.color = Color.black;
                        break;
                    case 1:
                        bar.color = new Color(1f, 1f, 1f, 0.85f);
                        break;
                    case 2:
                        bar.color = new Color(0.85f, 0f, 0.1f, 0.85f);
                        break;
                    case 3:
                        bar.color = new Color(0f, 0.9f, 0.9f, 0.6f);
                        break;
                    default:
                        bar.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
                        break;
                }
            }
        }

        private void HideBars()
        {
            foreach (var bar in _bars)
            {
                bar.enabled = false;
            }
        }

        // Everyone stops celebrating once the lights are out, and Chaplin goes back to standing.
        private void FreezeCast()
        {
            var root = cast != null ? cast : transform;
            foreach (var loop in root.GetComponentsInChildren<CelebrationLoop>())
            {
                loop.enabled = false;
            }

            foreach (var flipbook in root.GetComponentsInChildren<SpriteFlipbook>())
            {
                flipbook.enabled = false;
            }

            _image.sprite = _idle;
        }

        private void PlaceTorch(Vector2 centre, float radius)
        {
            // A slight flicker, like a real torch.
            var flicker = 1f + (Mathf.PerlinNoise(Time.unscaledTime * 6f, 0.3f) - 0.5f) * 0.06f;
            var across = radius * 2f * TorchSpan * flicker;
            _torch.anchoredPosition = centre;
            _torch.sizeDelta = new Vector2(across, across);
        }

        // A copy of Chaplin on the overlay, exactly over the real one, which hides. Returns where
        // his face is, how far that is from the copy's pivot, and how tall his face is on screen.
        private Vector2 MakeCloseUp(out Vector2 faceOffset, out float faceHeight)
        {
            var source = (RectTransform)transform;
            var go = new GameObject("Chaplin Close-Up", typeof(RectTransform), typeof(Image));
            go.layer = gameObject.layer;
            _closeUp = (RectTransform)go.transform;
            _closeUp.SetParent(_overlay, false);
            _closeUp.SetSiblingIndex(0);
            var image = go.GetComponent<Image>();
            image.sprite = _idle;
            image.preserveAspect = true;
            image.raycastTarget = false;

            var toOverlay = source.lossyScale.x / Mathf.Max(0.0001f, _overlay.lossyScale.x);
            _closeUp.anchorMin = _closeUp.anchorMax = new Vector2(0.5f, 0.5f);
            _closeUp.pivot = source.pivot;
            _closeUp.sizeDelta = source.rect.size * toOverlay;
            _closeUp.anchoredPosition = _overlay.InverseTransformPoint(source.position);
            _closeUp.localScale = Vector3.one;
            _image.enabled = false;

            var spriteSize = _idle != null ? _idle.rect.size : _closeUp.sizeDelta;
            var local = new Rect(-_closeUp.pivot * _closeUp.sizeDelta, _closeUp.sizeDelta);
            var drawn = DrawnRect(local, _closeUp.pivot, spriteSize);
            faceOffset = drawn.position + Vector2.Scale(faceOnSprite, drawn.size);
            faceHeight = drawn.height * faceHeightOnSprite;
            return _closeUp.anchoredPosition + faceOffset;
        }

        private AudioClip MakeGlitchClip(float seconds)
        {
            const int rate = 44100;
            var samples = GlitchSamples(rate, seconds, 1913);
            var clip = AudioClip.Create("Glitch", Mathf.Max(1, samples.Length), 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void BuildOverlay()
        {
            var root = new GameObject("Easter Egg Overlay", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.layer = gameObject.layer;
            _overlay = (RectTransform)root.transform;
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 1f;

            _audio = root.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            _audio.spatialBlend = 0f;
            SoundEffectsVolume.Exclude(_audio);

            // From the bottom up: (the close-up goes here later), the torch, the black, the glitch
            // bars, and an invisible cover so nothing underneath can be clicked any more.
            _torchTexture = MakeTorchTexture();
            var torch = MakeImage("Torch", _overlay, Color.white);
            torch.sprite = Sprite.Create(_torchTexture, new Rect(0f, 0f, TorchTexture, TorchTexture), new Vector2(0.5f, 0.5f));
            _torch = torch.rectTransform;
            _torch.gameObject.SetActive(false);

            _black = MakeImage("Black", _overlay, new Color(0f, 0f, 0f, 0f));
            Stretch(_black.rectTransform);

            for (var i = 0; i < GlitchBars; i++)
            {
                var bar = MakeImage("Glitch Bar", _overlay, Color.black);
                bar.enabled = false;
                _bars.Add(bar);
            }

            var cover = MakeImage("Click Cover", _overlay, new Color(0f, 0f, 0f, 0f));
            Stretch(cover.rectTransform);
            cover.raycastTarget = true;
        }

        private static Texture2D MakeTorchTexture()
        {
            var texture = new Texture2D(TorchTexture, TorchTexture, TextureFormat.RGBA32, false)
            {
                name = "Torch",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color32[TorchTexture * TorchTexture];
            var middle = (TorchTexture - 1) * 0.5f;
            var radius = TorchTexture / TorchSpan * 0.5f;
            for (var y = 0; y < TorchTexture; y++)
            {
                for (var x = 0; x < TorchTexture; x++)
                {
                    var distance = Mathf.Sqrt((x - middle) * (x - middle) + (y - middle) * (y - middle));
                    var dark = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(radius * 0.55f, radius, distance));
                    pixels[y * TorchTexture + x] = new Color32(0, 0, 0, (byte)(dark * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
