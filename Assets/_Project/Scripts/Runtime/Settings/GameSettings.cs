using UnityEngine;

namespace TunnelVision.Settings
{
    // The player's settings (volumes, interface sounds, display), with loading and saving.
    // Plain C# rather than a MonoBehaviour, so tests can use it without a scene.
    public sealed class GameSettings
    {
        public const string MasterVolumeKey = "TunnelVision.Settings.MasterVolume";
        public const string InterfaceVolumeKey = "TunnelVision.Settings.InterfaceVolume";
        public const string MusicVolumeKey = "TunnelVision.Settings.MusicVolume";
        public const string EffectsVolumeKey = "TunnelVision.Settings.EffectsVolume";
        public const string InterfaceSoundsKey = "TunnelVision.Settings.InterfaceSounds";
        public const string DisplayModeKey = "TunnelVision.Settings.DisplayMode";
        public const string ResolutionWidthKey = "TunnelVision.Settings.ResolutionWidth";
        public const string ResolutionHeightKey = "TunnelVision.Settings.ResolutionHeight";

        public const float DefaultMasterVolume = 0.8f;
        public const float DefaultInterfaceVolume = 0.8f;
        public const float DefaultMusicVolume = 0.6f;
        public const float DefaultEffectsVolume = 0.8f;
        public const bool DefaultInterfaceSounds = true;
        public const FullScreenMode DefaultDisplayMode = FullScreenMode.FullScreenWindow;

        // A width and height of 0 means "use the size of the player's screen".
        public const int DefaultResolutionSize = 0;

        private readonly ISettingsStore _store;
        private float _masterVolume = DefaultMasterVolume;
        private float _interfaceVolume = DefaultInterfaceVolume;
        private float _musicVolume = DefaultMusicVolume;
        private float _effectsVolume = DefaultEffectsVolume;

        // Fired whenever the audio settings are applied, so sounds can update their volume.
        public static event System.Action AudioApplied;

        public GameSettings(ISettingsStore store)
        {
            _store = store;
        }

        // All volumes go from 0 to 1. Master is the overall volume; the others are on top of it.
        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = Mathf.Clamp01(value);
        }

        public float InterfaceVolume
        {
            get => _interfaceVolume;
            set => _interfaceVolume = Mathf.Clamp01(value);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set => _musicVolume = Mathf.Clamp01(value);
        }

        public float EffectsVolume
        {
            get => _effectsVolume;
            set => _effectsVolume = Mathf.Clamp01(value);
        }

        public bool InterfaceSounds { get; set; } = DefaultInterfaceSounds;

        public FullScreenMode DisplayMode { get; set; } = DefaultDisplayMode;

        public int ResolutionWidth { get; set; } = DefaultResolutionSize;

        public int ResolutionHeight { get; set; } = DefaultResolutionSize;

        // 0 when interface sounds are switched off.
        public float EffectiveInterfaceVolume => InterfaceSounds ? InterfaceVolume : 0f;

        // Borderless windowed always fills the screen, and a saved size of 0 means the screen's own size.
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
            MusicVolume = _store.GetFloat(MusicVolumeKey, DefaultMusicVolume);
            EffectsVolume = _store.GetFloat(EffectsVolumeKey, DefaultEffectsVolume);
            InterfaceSounds = _store.GetInt(InterfaceSoundsKey, DefaultInterfaceSounds ? 1 : 0) == 1;
            DisplayMode = ParseDisplayMode(_store.GetInt(DisplayModeKey, (int)DefaultDisplayMode));
            ResolutionWidth = _store.GetInt(ResolutionWidthKey, DefaultResolutionSize);
            ResolutionHeight = _store.GetInt(ResolutionHeightKey, DefaultResolutionSize);
        }

        public void Save()
        {
            _store.SetFloat(MasterVolumeKey, MasterVolume);
            _store.SetFloat(InterfaceVolumeKey, InterfaceVolume);
            _store.SetFloat(MusicVolumeKey, MusicVolume);
            _store.SetFloat(EffectsVolumeKey, EffectsVolume);
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
            MusicVolume = DefaultMusicVolume;
            EffectsVolume = DefaultEffectsVolume;
            InterfaceSounds = DefaultInterfaceSounds;
            DisplayMode = DefaultDisplayMode;
            ResolutionWidth = DefaultResolutionSize;
            ResolutionHeight = DefaultResolutionSize;
        }

        // Pushes the current values into the running game.
        public void Apply()
        {
            ApplyAudio();
            ApplyDisplay();
        }

        public void ApplyAudio()
        {
            AudioListener.volume = MasterVolume;
            AudioApplied?.Invoke();
        }

        public void ApplyDisplay()
        {
            Vector2Int size = GetWindowSize(new Vector2Int(Display.main.systemWidth, Display.main.systemHeight));
            Screen.SetResolution(size.x, size.y, DisplayMode);
        }

        // Falls back to the default if the saved value isn't one of the three modes the options page offers.
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
