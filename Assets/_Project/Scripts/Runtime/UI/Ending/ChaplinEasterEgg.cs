using System;
using System.Collections;
using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// The hidden Chaplin on the Demo Complete screen. Press F or click him: the music winds down
    /// like a dying tape while the confetti turns black, then it cuts hard to a silent black
    /// screen, his "Tunnel Vision" clip plays, and the game closes (in the Editor, Play mode stops).
    /// <para>
    /// Goes on Chaplin's Image, which must be a raycast target. The black screen is made by code
    /// on its own canvas above everything else. Real time throughout.
    /// </para>
    /// </summary>
    public class ChaplinEasterEgg : MonoBehaviour, IPointerClickHandler
    {
        public enum Stage
        {
            Waiting,
            WindingDown,
            CutToBlack,
            Voice,
            Closing,
        }

        [Header("Wiring")]
        [SerializeField] private AudioClip voice;
        [SerializeField] private ConfettiRain confetti;

        [Header("Timings (Seconds)")]
        [FormerlySerializedAs("musicFadeSeconds")]
        [SerializeField] private float windDownSeconds = 1.5f;
        [Tooltip("Silence on the black screen before his clip.")]
        [SerializeField] private float darkBeforeVoice = 0.4f;
        [SerializeField] private float darkAfterVoice = 0.8f;

        [Header("Sound")]
        [Tooltip("How low the music's pitch drags before the cut (1 = normal).")]
        [Range(0.05f, 1f)]
        [SerializeField] private float windDownEndPitch = 0.15f;

        [Header("Look")]
        [SerializeField] private Color confettiColour = Color.black;

        /// <summary>What closes the game at the end. Tests swap it so the test run isn't stopped.</summary>
        public static Action QuitAction = QuitGame;

        private Image _black;
        private AudioSource _audio;

        public Stage Current { get; private set; } = Stage.Waiting;
        public AudioClip Voice => voice;
        public ConfettiRain Confetti => confetti;

        /// <summary>The black screen, once it's up.</summary>
        public Image Black => _black;

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

            Current = Stage.WindingDown;
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            BuildOverlay();

            // The happy music winds down and the confetti turns black.
            var music = MusicPlayer.Current;
            if (music != null)
            {
                music.WindDown(windDownSeconds, windDownEndPitch);
            }

            if (confetti != null)
            {
                confetti.FadeTo(confettiColour, windDownSeconds);
            }

            yield return new WaitForSecondsRealtime(windDownSeconds);

            // Then a hard cut: black screen, no sound at all.
            Current = Stage.CutToBlack;
            if (music != null)
            {
                music.Cut();
            }

            SoundEffectsVolume.FadeOutGameplay(0f); // Any other sound too. His clip is left out.
            SetBlack(1f);
            yield return new WaitForSecondsRealtime(darkBeforeVoice);

            // His clip, in the dark.
            Current = Stage.Voice;
            if (voice != null)
            {
                _audio.PlayOneShot(voice);
                yield return new WaitForSecondsRealtime(voice.length);
            }

            yield return new WaitForSecondsRealtime(darkAfterVoice);

            Current = Stage.Closing;
            QuitAction?.Invoke();
        }

        private void SetBlack(float alpha)
        {
            _black.color = new Color(0f, 0f, 0f, alpha);
        }

        private void BuildOverlay()
        {
            var root = new GameObject("Easter Egg Overlay", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
            root.layer = gameObject.layer;
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            _audio = root.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            _audio.spatialBlend = 0f;
            SoundEffectsVolume.Exclude(_audio);

            // Catches clicks from the start, so the Main Menu button can't cut it short.
            var go = new GameObject("Black", typeof(RectTransform), typeof(Image));
            go.layer = root.layer;
            go.transform.SetParent(root.transform, false);
            _black = go.GetComponent<Image>();
            _black.color = new Color(0f, 0f, 0f, 0f);
            _black.raycastTarget = true;
            var rect = _black.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
