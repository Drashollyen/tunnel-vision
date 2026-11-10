using UnityEngine;

namespace TunnelVision.Settings
{
    /// <summary>
    /// Player-facing settings (volume, interface sounds, display) with load/save.
    /// Plain C# so it can be unit tested without a scene.
    /// </summary>
    public sealed class GameSettings
    {
        public const string MasterVolumeKey = "TunnelVision.Settings.MasterVolume";
        public const string InterfaceVolumeKey = "TunnelVision.Settings.InterfaceVolume";
        public const string InterfaceSoundsKey = "TunnelVision.Settings.InterfaceSounds";
        public const string DisplayModeKey = "TunnelVision.Settings.DisplayMode";
        public const string ResolutionWidthKey = "TunnelVision.Settings.ResolutionWidth";
        public const string ResolutionHeightKey = "TunnelVision.Settings.ResolutionHeight";

        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultInterfaceVolume = 0.8f;
        public const bool DefaultInterfaceSounds = true;
        public const FullScreenMode DefaultDisplayMode = FullScreenMode.FullScreenWindow;

        /// <summary>A width and height of 0 means "use the size of the player's screen".</summary>
        public const int DefaultResolutionSize = 0;

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

        /// <summary>Fullscreen (exclusive), borderless windowed (full screen window) or windowed.</summary>
        public FullScreenMode DisplayMode { get; set; } = DefaultDisplayMode;

        public int ResolutionWidth { get; set; } = DefaultResolutionSize;

        public int ResolutionHeight { get; set; } = DefaultResolutionSize;

        /// <summary>Volume to play interface sounds at: 0 when they are switched off.</summary>
        public float EffectiveInterfaceVolume => InterfaceSounds ? InterfaceVolume : 0f;

        /// <summary>
        /// The window size for the current settings. Borderless windowed always fills the screen,
        /// and a saved size of 0 means the screen's own size.
        /// </summary>
        public Vector2Int GetWindowSize(Vector2Int screenSize)
        {
            bool useScreenSize = DisplayMode == FullScreenMode.FullScreenWindow
                || ResolutionWidth <= 0
                || ResolutionHeight <= 0;

            return useScreenSize ? screenSize : new Vector2Int(ResolutionWidth, ResolutionHeight);
        }

        public void Load()
        {
            MasterVolume = _store.GetFloat(MasterVolumeKey, DefaultMasterVolume);
            InterfaceVolume = _store.GetFloat(InterfaceVolumeKey, DefaultInterfaceVolume);
            InterfaceSounds = _store.GetInt(InterfaceSoundsKey, DefaultInterfaceSounds ? 1 : 0) == 1;
            DisplayMode = ParseDisplayMode(_store.GetInt(DisplayModeKey, (int)DefaultDisplayMode));
            ResolutionWidth = _store.GetInt(ResolutionWidthKey, DefaultResolutionSize);
            ResolutionHeight = _store.GetInt(ResolutionHeightKey, DefaultResolutionSize);
        }

        public void Save()
        {
            _store.SetFloat(MasterVolumeKey, MasterVolume);
            _store.SetFloat(InterfaceVolumeKey, InterfaceVolume);
            _store.SetInt(InterfaceSoundsKey, InterfaceSounds ? 1 : 0);
            _store.SetInt(DisplayModeKey, (int)DisplayMode);
            _store.SetInt(ResolutionWidthKey, ResolutionWidth);
            _store.SetInt(ResolutionHeightKey, ResolutionHeight);
            _store.Save();
        }

        public void ResetToDefaults()
        {
            MasterVolume = DefaultMasterVolume;
            InterfaceVolume = DefaultInterfaceVolume;
            InterfaceSounds = DefaultInterfaceSounds;
            DisplayMode = DefaultDisplayMode;
            ResolutionWidth = DefaultResolutionSize;
            ResolutionHeight = DefaultResolutionSize;
        }

        /// <summary>Pushes the current values into the running engine.</summary>
        public void Apply()
        {
            ApplyAudio();
            ApplyDisplay();
        }

        public void ApplyAudio()
        {
            AudioListener.volume = MasterVolume;
        }

        public void ApplyDisplay()
        {
            Vector2Int size = GetWindowSize(new Vector2Int(Display.main.systemWidth, Display.main.systemHeight));
            Screen.SetResolution(size.x, size.y, DisplayMode);
        }

        private static FullScreenMode ParseDisplayMode(int value)
        {
            var mode = (FullScreenMode)value;
            bool supported = mode == FullScreenMode.ExclusiveFullScreen
                || mode == FullScreenMode.FullScreenWindow
                || mode == FullScreenMode.Windowed;

            return supported ? mode : DefaultDisplayMode;
        }
    }
}
