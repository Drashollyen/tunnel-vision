using System.Linq;
using TunnelVision.Settings;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Audio
{
    // Menu music. Put one in each menu scene with the same track: the first one keeps playing
    // from menu to menu without restarting, and the later copies remove themselves. When a
    // scene that isn't in Music Scenes (a level) starts loading, the music fades out with the
    // screen. Follows the Music Volume setting.
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip music;
        [Tooltip("Scenes the music keeps playing in. Loading any other scene fades it out.")]
        [SerializeField] private string[] musicScenes = { "IntroScreen", "LevelSelect" };
        [SerializeField] private float fadeInSeconds = 1f;

        private static MusicPlayer _current;

        private AudioSource _source;
        private float _fade;
        private float _fadeTarget = 1f;
        private float _fadeSpeed;
        private bool _windingDown;
        private float _windStartedAt;
        private float _windSeconds;
        private float _windEndPitch;

        public static MusicPlayer Current => _current;

        public AudioClip Music => music;
        public bool IsFadingOut => _fadeTarget <= 0f;
        public float Fade => _fade;
        public bool IsWindingDown => _windingDown;
        public float Pitch => _source != null ? _source.pitch : 1f;

        public static float VolumeFor(float fade, float musicVolume) => Mathf.Clamp01(fade) * Mathf.Clamp01(musicVolume);

        // The pitch t seconds into a wind-down: it drops slowly at first, then faster, until it
        // reaches endPitch, like a tape or record player losing power.
        public static float WindDownPitchAt(float t, float seconds, float endPitch)
        {
            var k = seconds > 0f ? Mathf.Clamp01(t / seconds) : 1f;
            return Mathf.Lerp(1f, endPitch, k * k);
        }

        private void Awake()
        {
            // This track is already playing from the last menu, so let it carry on and remove this copy.
            if (_current != null && _current != this && _current.music == music && !_current.IsFadingOut)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            if (_current != null && _current != this)
            {
                Destroy(_current.gameObject);
            }

            _current = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _source = GetComponent<AudioSource>();
            _source.clip = music;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            SoundEffectsVolume.Exclude(_source);

            _fade = 0f;
            _fadeTarget = 1f;
            _fadeSpeed = fadeInSeconds > 0f ? 1f / fadeInSeconds : float.PositiveInfinity;
            ApplyVolume();
            if (music != null)
            {
                _source.Play();
            }

            SceneFader.LoadStarted += OnLoadStarted;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneFader.LoadStarted -= OnLoadStarted;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_current == this)
            {
                _current = null;
            }
        }

        public bool PlaysIn(string sceneName) => musicScenes != null && musicScenes.Contains(sceneName);

        public void OnLoadStarted(string sceneName)
        {
            if (!PlaysIn(sceneName))
            {
                FadeOut(SceneFader.FadeSeconds);
            }
        }

        public void FadeOut(float seconds)
        {
            _fadeTarget = 0f;
            _fadeSpeed = seconds > 0f ? 1f / seconds : float.PositiveInfinity;
        }

        // Slows the music down (dropping its pitch) over some seconds. It keeps playing slowly until Cut is called.
        public void WindDown(float seconds, float endPitch)
        {
            _windingDown = true;
            _windStartedAt = Time.unscaledTime;
            _windSeconds = seconds;
            _windEndPitch = endPitch;
        }

        // Stops the music instantly, with no fade.
        public void Cut()
        {
            if (_source != null)
            {
                _source.Stop();
            }

            if (_current == this)
            {
                _current = null;
            }

            Destroy(gameObject);
        }

        // Safety net for scene loads that skip the SceneFader: menu music must never carry on into a level.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && !PlaysIn(scene.name))
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Real time, so pausing the game doesn't freeze the fade.
            _fade = Mathf.MoveTowards(_fade, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);
            ApplyVolume();

            if (_windingDown)
            {
                _source.pitch = WindDownPitchAt(Time.unscaledTime - _windStartedAt, _windSeconds, _windEndPitch);
            }

            if (IsFadingOut && _fade <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void ApplyVolume()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.MusicVolume : 1f;
            _source.volume = VolumeFor(_fade, setting);
        }
    }
}
