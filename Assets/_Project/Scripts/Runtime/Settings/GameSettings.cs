using UnityEngine;

namespace TunnelVision.Settings
{
    /// <summary>
    /// Player-facing settings (volume, interface sounds, display mode) with load/save.
    /// Plain C# so it can be unit tested without a scene.
    /// </summary>
    public sealed class GameSettings
    {
        public const string MasterVolumeKey = "TunnelVision.Settings.MasterVolume";
        public const string InterfaceVolumeKey = "TunnelVision.Settings.InterfaceVolume";
        public const string InterfaceSoundsKey = "TunnelVision.Settings.InterfaceSounds";
        public const string FullscreenKey = "TunnelVision.Settings.Fullscreen";

        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultInterfaceVolume = 0.8f;
        public const bool DefaultInterfaceSounds = true;
        public const bool DefaultFullscreen = true;

        private readonly ISettingsStore _store;
        private float _masterVolume = DefaultMasterVolume;
        private float _interfaceVolume = DefaultInterfaceVolume;

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

        /// <summary>Volume of menu hover/click sounds in the range 0..1, on top of the master volume.</summary>
        public float InterfaceVolume
        {
            get => _interfaceVolume;
            set => _interfaceVolume = Mathf.Clamp01(value);
        }

        public bool InterfaceSounds { get; set; } = DefaultInterfaceSounds;

        public bool Fullscreen { get; set; } = DefaultFullscreen;

        /// <summary>Volume to play interface sounds at: 0 when they are switched off.</summary>
        public float EffectiveInterfaceVolume => InterfaceSounds ? InterfaceVolume : 0f;

        public void Load()
        {
            MasterVolume = _store.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            InterfaceVolume = _store.GetFloat(InterfaceVolumeKey, DefaultInterfaceVolume);
            InterfaceSounds = _store.GetInt(InterfaceSoundsKey, DefaultInterfaceSounds ? 1 : 0) == 1;
            Fullscreen = _store.GetInt(FullscreenKey, DefaultFullscreen ? 1 : 0) == 1;
        }

        public void Save()
        {
            _store.SetFloat(MasterVolumeKey, MasterVolume);
            _store.SetFloat(InterfaceVolumeKey, InterfaceVolume);
            _store.SetInt(InterfaceSoundsKey, InterfaceSounds ? 1 : 0);
            _store.SetInt(FullscreenKey, Fullscreen ? 1 : 0);
            _store.Save();
        }

        public void ResetToDefaults()
        {
            MasterVolume = DefaultMasterVolume;
            InterfaceVolume = DefaultInterfaceVolume;
            InterfaceSounds = DefaultInterfaceSounds;
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
