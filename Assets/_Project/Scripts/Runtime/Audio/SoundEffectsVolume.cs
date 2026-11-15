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
    /// </summary>
    public static class SoundEffectsVolume
    {
        private static readonly Dictionary<AudioSource, float> BaseVolumes = new Dictionary<AudioSource, float>();
        private static readonly HashSet<AudioSource> NotEffects = new HashSet<AudioSource>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            // Fast Enter Play Mode keeps statics, so start each session clean.
            BaseVolumes.Clear();
            NotEffects.Clear();
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

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply();

        /// <summary>Applies the current Effects Volume to every sound in the loaded scenes.</summary>
        public static void Apply()
        {
            var setting = SettingsService.Current != null ? SettingsService.Current.EffectsVolume : 1f;

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
    }
}
