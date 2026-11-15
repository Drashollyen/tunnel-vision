using System.Linq;
using TunnelVision.Settings;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Audio
{
    /// <summary>
    /// Background music for the menus. Put one at the root of each menu scene with the same
    /// track: the first keeps playing from scene to scene without restarting, and the others
    /// step aside. When the scene fader starts loading a scene that isn't a music scene (a
    /// level), the music fades out with the screen, so it never overlaps the level.
    /// Follows the Music Volume setting.
    /// </summary>
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

        /// <summary>The music playing now, if any.</summary>
        public static MusicPlayer Current => _current;

        public AudioClip Music => music;
        public bool IsFadingOut => _fadeTarget <= 0f;
        public float Fade => _fade;

        /// <summary>The volume the music plays at for a fade (0..1) and the Music Volume setting.</summary>
        public static float VolumeFor(float fade, float musicVolume) => Mathf.Clamp01(fade) * Mathf.Clamp01(musicVolume);

        private void Awake()
        {
            // Already playing this track from the last menu: let it carry on.
            if (_current != null && _current != this && _current.music == music && !_current.IsFadingOut)
            {
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

        /// <summary>Called as the fader starts a load. Fades out alongside the screen for a level.</summary>
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

        // Covers loads that skip the fader: never let the music run on into a level.
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single && !PlaysIn(scene.name))
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            // Unscaled, so a paused game doesn't freeze the fade.
            _fade = Mathf.MoveTowards(_fade, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);
            ApplyVolume();

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
