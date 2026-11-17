using System.Collections.Generic;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Audio
{
    // Scales every in-game sound (jumps, platforms, hazards and so on) by the Effects Volume
    // setting, so no sound script has to do it itself. Each sound keeps the volume it was given
    // in the scene, and the setting multiplies that. Music and menu sounds are left out.
    // It can also fade all gameplay sounds out and pause them (for Out Of Lives and the pause
    // menu, so looping sounds like the wheels stop too), then bring them back.
    public static class SoundEffectsVolume
    {
        private static readonly Dictionary<AudioSource, float> BaseVolumes = new Dictionary<AudioSource, float>();
        private static readonly HashSet<AudioSource> NotEffects = new HashSet<AudioSource>();
        private static readonly List<AudioSource> PausedByUs = new List<AudioSource>();

        private static float _gameplayLevel = 1f;
        private static float _gameplayTarget = 1f;
        private static float _gameplaySpeed;
        private static float _fadeStartsAt;
        private static Fader _fader;

        // The most the fade can move in one frame, so one slow frame can't skip it.
        private const float MaxStepSeconds = 0.1f;

        // How loud gameplay sounds are allowed to be right now, from 0 (silent) to 1.
        public static float GameplayLevel => _gameplayLevel;

        public static bool IsGameplaySilenced => _gameplayTarget <= 0f && _gameplayLevel <= 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            // Fast Enter Play Mode keeps static values between Play sessions, so clear them here.
            BaseVolumes.Clear();
            NotEffects.Clear();
            PausedByUs.Clear();
            _gameplayLevel = _gameplayTarget = 1f;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameSettings.AudioApplied -= Apply;
            GameSettings.AudioApplied += Apply;
        }

        // Leaves this sound alone. For music and menu sounds, which have their own volume settings.
        public static void Exclude(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            NotEffects.Add(source);
            if (BaseVolumes.TryGetValue(source, out var original))
            {
                source.volume = original;
                BaseVolumes.Remove(source);
            }
        }

        public static float Scaled(float baseVolume, float effectsVolume) => baseVolume * Mathf.Clamp01(effectsVolume);

        // Waits for the delay, fades every gameplay sound out, then pauses them.
        // Uses real time, so it works while the game is paused.
        public static void FadeOutGameplay(float seconds, float delay = 0f)
        {
            StartFade(0f, seconds, delay);
        }

        public static void ResumeGameplay(float seconds)
        {
            foreach (var source in PausedByUs)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }

            PausedByUs.Clear();
            StartFade(1f, seconds, 0f);
        }

        // Turns gameplay sound straight back on, with no fade.
        public static void RestoreGameplayNow()
        {
            foreach (var source in PausedByUs)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }

            PausedByUs.Clear();
            _gameplayLevel = _gameplayTarget = 1f;
            Apply();
        }

        private static void StartFade(float target, float seconds, float delay)
        {
            _gameplayTarget = target;
            _gameplaySpeed = seconds > 0f ? 1f / seconds : float.PositiveInfinity;
            _fadeStartsAt = Time.unscaledTime + Mathf.Max(0f, delay);

            if (_fader == null)
            {
                var go = new GameObject("Gameplay Sound Fader") { hideFlags = HideFlags.HideAndDontSave };
                Object.DontDestroyOnLoad(go);
                _fader = go.AddComponent<Fader>();
            }

            if (float.IsPositiveInfinity(_gameplaySpeed) && delay <= 0f)
            {
                Step();
            }
        }

        private static void Step()
        {
            // Still waiting for the delay, or already there.
            if (Time.unscaledTime < _fadeStartsAt || _gameplayLevel == _gameplayTarget)
            {
                return;
            }

            _gameplayLevel = float.IsPositiveInfinity(_gameplaySpeed)
                ? _gameplayTarget
                : Mathf.MoveTowards(_gameplayLevel, _gameplayTarget,
                    _gameplaySpeed * Mathf.Min(Time.unscaledDeltaTime, MaxStepSeconds));
            Apply();

            if (_gameplayTarget <= 0f && _gameplayLevel <= 0f)
            {
                PauseGameplaySources();
            }
        }

        private static void PauseGameplaySources()
        {
            foreach (var source in BaseVolumes.Keys)
            {
                if (source != null && source.isPlaying)
                {
                    source.Pause();
                    PausedByUs.Add(source);
                }
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Whatever silenced the last scene, a new one starts with its sounds on.
            if (mode == LoadSceneMode.Single)
            {
                PausedByUs.Clear();
                _gameplayLevel = _gameplayTarget = 1f;
            }

            Apply();
        }

        // Applies the current Effects Volume to every sound in the loaded scenes.
        public static void Apply()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.EffectsVolume : 1f;
            setting *= _gameplayLevel;

            // Forget sounds that have been destroyed.
            var gone = new List<AudioSource>();
            foreach (var source in BaseVolumes.Keys)
            {
                if (source == null)
                    gone.Add(source);
            }

            foreach (var source in gone)
            {
                BaseVolumes.Remove(source);
            }

            NotEffects.RemoveWhere(source => source == null);
            PausedByUs.RemoveAll(source => source == null);

            foreach (var source in Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
            {
                if (NotEffects.Contains(source))
                {
                    continue;
                }

                if (!BaseVolumes.TryGetValue(source, out var baseVolume))
                {
                    baseVolume = source.volume;
                    BaseVolumes[source] = baseVolume;
                }

                source.volume = Scaled(baseVolume, setting);
            }
        }

        // A hidden object that runs the fade every frame, since a static class can't have an Update.
        private class Fader : MonoBehaviour
        {
            private void Update() => Step();
        }
    }
}
