using UnityEngine;

namespace TunnelVision.Settings
{
    /// <summary>
    /// Player-facing settings (volume, display mode) with load/save.
    /// Plain C# so it can be unit tested without a scene.
    /// </summary>
    public sealed class GameSettings
    {
        // Prefixed so they never collide with the Bloodlines UI SoundManager keys ("MasterVolume", ...).
        public const string MasterVolumeKey = "TunnelVision.Settings.MasterVolume";
        public const string FullscreenKey = "TunnelVision.Settings.Fullscreen";

        public const float DefaultMasterVolume = 0.8f;
        public const bool DefaultFullscreen = true;

        private readonly ISettingsStore _store;
        private float _masterVolume = DefaultMasterVolume;

        public GameSettings(ISettingsStore store)
        {
            _store = store;
        }

        /// <summary>Overall game volume in the range 0..1.</summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Mathf.Clamp01(value);
        }

        public bool Fullscreen { get; set; } = DefaultFullscreen;

        public void Load()
        {
            MasterVolume = _store.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            Fullscreen = _store.GetInt(FullscreenKey, DefaultFullscreen ? 1 : 0) == 1;
        }

        public void Save()
        {
            _store.SetFloat(MasterVolumeKey, MasterVolume);
            _store.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            _store.Save();
        }

        public void ResetToDefaults()
        {
            MasterVolume = DefaultMasterVolume;
            Fullscreen = DefaultFullscreen;
        }

        /// <summary>Pushes the current values into the running engine.</summary>
        public void Apply()
        {
            AudioListener.volume = MasterVolume;
            Screen.fullScreen = Fullscreen;
        }
    }
}
