using System.Collections.Generic;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Audio
{
    /// <summary>
    /// Scales every in-game sound (jumps, platforms, hazards and so on) by the Effects Volume
    /// setting, so sounds follow the options page without each script having to. Each source
    /// keeps the volume it was given in its scene, and the setting multiplies that. Runs when a
    /// scene loads and whenever audio settings change. Music and menu sounds are left out.
    /// <para>
    /// It can also silence gameplay: <see cref="FadeOutGameplay"/> fades every game sound down and
    /// pauses it (for Out Of Lives and the pause menu, when the game has stopped but looping
    /// sounds like the wheels would otherwise carry on), and <see cref="ResumeGameplay"/> brings
    /// them back. A new scene always starts with gameplay sound on.
    /// </para>
    /// </summary>
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

        /// <summary>How loud gameplay sounds are allowed to be right now, from 0 (silenced) to 1.</summary>
        public static float GameplayLevel => _gameplayLevel;

        /// <summary>True once gameplay sounds have been faded out and paused.</summary>
        public static bool IsGameplaySilenced => _gameplayTarget <= 0f && _gameplayLevel <= 0f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            // Fast Enter Play Mode keeps statics, so start each session clean.
            BaseVolumes.Clear();
            NotEffects.Clear();
            PausedByUs.Clear();
            _gameplayLevel = _gameplayTarget = 1f;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            GameSettings.AudioApplied -= Apply;
            GameSettings.AudioApplied += Apply;
        }

        /// <summary>Leaves a source alone: for music and menu sounds, which have their own settings.</summary>
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

        /// <summary>The volume a source plays at for a given base volume and setting.</summary>
        public static float Scaled(float baseVolume, float effectsVolume) => baseVolume * Mathf.Clamp01(effectsVolume);

        /// <summary>
        /// Fades every gameplay sound out over <paramref name="seconds"/>, after waiting
        /// <paramref name="delay"/>, then pauses them. Uses real time, so it works while paused.
        /// </summary>
        public static void FadeOutGameplay(float seconds, float delay = 0f)
        {
            StartFade(0f, seconds, delay);
        }

        /// <summary>Unpauses the gameplay sounds that were silenced and fades them back up.</summary>
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

        /// <summary>Turns gameplay sound straight back on, with no fade. For tests and scene changes.</summary>
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
            // Waiting for the delay, or nothing to do.
            if (Time.unscaledTime < _fadeStartsAt || _gameplayLevel == _gameplayTarget)
            {
                return;
            }

            _gameplayLevel = float.IsPositiveInfinity(_gameplaySpeed)
                ? _gameplayTarget
                : Mathf.MoveTowards(_gameplayLevel, _gameplayTarget, _gameplaySpeed * Time.unscaledDeltaTime);
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

        /// <summary>Applies the current Effects Volume to every sound in the loaded scenes.</summary>
        public static void Apply()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.EffectsVolume : 1f;
            setting *= _gameplayLevel;

            // Forget sources that have been destroyed.
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

        /// <summary>Runs the gameplay fade each frame.</summary>
        private class Fader : MonoBehaviour
        {
            private void Update() => Step();
        }
    }
}
