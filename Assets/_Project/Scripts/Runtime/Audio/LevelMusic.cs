using TunnelVision.Level;
using TunnelVision.Player;
using TunnelVision.Settings;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Audio
{
    // A level's background music. It fades out when the player dies and starts again from the
    // beginning on respawn (but stays silent once out of lives). It also fades out when leaving
    // the level, and gets quieter as the player nears the goal so it doesn't clash with the
    // level complete jingle. Follows the Music Volume setting.
    [RequireComponent(typeof(AudioSource))]
    public class LevelMusic : MonoBehaviour
    {
        [Tooltip("How far before the goal the music starts getting quieter, in world units.")]
        [SerializeField] private float approachDistance = 30f;

        // How fast the near-the-goal volume can change, so a respawn doesn't make it jump.
        public const float ApproachChangePerSecond = 1.5f;

        private AudioSource _source;
        private LevelGoal _goal;
        private float _approach = 1f;
        private PlayerRespawn _respawn;
        private PlayerLives _lives;
        private float _baseVolume;
        private float _fade = 1f;
        private float _fadeTarget = 1f;
        private float _fadeSpeed = float.PositiveInfinity;

        public float Fade => _fade;
        public float Approach => _approach;
        public bool IsSilenced => _fadeTarget <= 0f;

        // 1 when the goal is far away, easing smoothly down to 0 at the goal.
        public static float ApproachLevel(float distanceToGoal, float range)
        {
            if (range <= 0f)
            {
                return distanceToGoal > 0f ? 1f : 0f;
            }

            var t = Mathf.Clamp01(distanceToGoal / range);
            return t * t * (3f - 2f * t);
        }

        public static float VolumeFor(float baseVolume, float fade, float musicVolume) =>
            baseVolume * Mathf.Clamp01(fade) * Mathf.Clamp01(musicVolume);

        public static float VolumeFor(float baseVolume, float fade, float approach, float musicVolume) =>
            VolumeFor(baseVolume, fade, musicVolume) * Mathf.Clamp01(approach);

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _baseVolume = _source.volume;
            SoundEffectsVolume.Exclude(_source); // Music has its own volume setting.

            _respawn = FindAnyObjectByType<PlayerRespawn>();
            if (_respawn != null)
            {
                _lives = _respawn.GetComponent<PlayerLives>();
                _respawn.Died += OnDied;
                _respawn.Respawning += OnRespawning;
            }

            _goal = FindAnyObjectByType<LevelGoal>();
            _approach = TargetApproach();

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

            // Start the track from the beginning and fade it in as the screen comes back.
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
            // Real time, because Out Of Lives and the pause menu stop game time.
            _fade = Mathf.MoveTowards(_fade, _fadeTarget, _fadeSpeed * Time.unscaledDeltaTime);
            _approach = Mathf.MoveTowards(_approach, TargetApproach(), ApproachChangePerSecond * Time.unscaledDeltaTime);
            ApplyVolume();

            if (IsSilenced && _fade <= 0f && _source.isPlaying)
            {
                _source.Stop();
            }
        }

        private float TargetApproach()
        {
            if (_goal == null || _respawn == null)
            {
                return 1f;
            }

            return ApproachLevel(_goal.CrossingX - _respawn.transform.position.x, approachDistance);
        }

        private void ApplyVolume()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.MusicVolume : 1f;
            _source.volume = VolumeFor(_baseVolume, _fade, _approach, setting);
        }
    }
}
