using TunnelVision.Player;
using TunnelVision.Settings;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Audio
{
    /// <summary>
    /// A level's background music. When the player dies it fades out smoothly over the death
    /// sequence, reaching silence as the screen goes black. On respawn it starts the track again
    /// from the beginning and fades back in with the screen. Out of lives, it stays silent.
    /// Leaving the level fades it out with the scene fader. Follows the Music Volume setting,
    /// on top of the volume set on the AudioSource. Goes on the object with the music's
    /// AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LevelMusic : MonoBehaviour
    {
        private AudioSource _source;
        private PlayerRespawn _respawn;
        private PlayerLives _lives;
        private float _baseVolume;
        private float _fade = 1f;
        private float _fadeTarget = 1f;
        private float _fadeSpeed = float.PositiveInfinity;

        public float Fade => _fade;
        public bool IsSilenced => _fadeTarget <= 0f;

        /// <summary>The volume played at for the source's own volume, the fade and the setting.</summary>
        public static float VolumeFor(float baseVolume, float fade, float musicVolume) =>
            baseVolume * Mathf.Clamp01(fade) * Mathf.Clamp01(musicVolume);

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _baseVolume = _source.volume;
            SoundEffectsVolume.Exclude(_source); // Music has its own setting.

            _respawn = FindAnyObjectByType<PlayerRespawn>();
            if (_respawn != null)
            {
                _lives = _respawn.GetComponent<PlayerLives>();
                _respawn.Died += OnDied;
                _respawn.Respawning += OnRespawning;
            }

            SceneFader.LoadStarted += OnLoadStarted;
            ApplyVolume();
        }

        private void OnDestroy()
        {
            if (_respawn != null)
            {
                _respawn.Died -= OnDied;
                _respawn.Respawning -= OnRespawning;
            }

            SceneFader.LoadStarted -= OnLoadStarted;
        }

        private void OnDied()
        {
            FadeTo(0f, _respawn.SecondsUntilBlack);
        }

        private void OnRespawning()
        {
            if (_lives != null && _lives.IsDead)
            {
                return;
            }

            // From the top, rising with the screen.
            _source.Stop();
            _source.time = 0f;
            _fade = 0f;
            ApplyVolume();
            _source.Play();
            FadeTo(1f, _respawn.FadeInSeconds);
        }

        private void OnLoadStarted(string sceneName)
        {
            FadeTo(0f, SceneFader.FadeSeconds);
        }

        public void FadeTo(float target, float seconds)
        {
            _fadeTarget = Mathf.Clamp01(target);
            _fadeSpeed = seconds > 0f ? Mathf.Abs(_fadeTarget - _fade) / seconds : float.PositiveInfinity;
        }

        private void Update()
        {
            // Unscaled, since Out Of Lives and the pause menu stop game time.
            _fade = Mathf.MoveTowards(_fade, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);
            ApplyVolume();

            if (IsSilenced && _fade <= 0f && _source.isPlaying)
            {
                _source.Stop();
            }
        }

        private void ApplyVolume()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.MusicVolume : 1f;
            _source.volume = VolumeFor(_baseVolume, _fade, setting);
        }
    }
}
