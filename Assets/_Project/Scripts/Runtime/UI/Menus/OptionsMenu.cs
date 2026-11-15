using System.Collections.Generic;
using System.Linq;
using TMPro;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Connects the options page controls to the saved settings.
    /// Every change is applied and saved straight away, so there is no Apply button.
    /// The same page (the OptionsPanel prefab) is used on the main menu and in the pause menu,
    /// so whoever shows it listens to <see cref="BackPressed"/> to go back.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        // Same order as the display mode dropdown's options.
        private static readonly FullScreenMode[] DisplayModes =
        {
            FullScreenMode.ExclusiveFullScreen,
            FullScreenMode.FullScreenWindow,
            FullScreenMode.Windowed,
        };

        private static readonly string[] DisplayModeLabels =
        {
            "Fullscreen",
            "Borderless Windowed",
            "Windowed",
        };

        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider effectsVolumeSlider;
        [SerializeField] private Toggle interfaceSoundsToggle;
        [SerializeField] private Slider interfaceVolumeSlider;
        [SerializeField] private TMP_Dropdown displayModeDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private Button backButton;

        /// <summary>Raised when the Back button is pressed.</summary>
        public event System.Action BackPressed;

        private List<Vector2Int> _resolutions = new List<Vector2Int>();

        private void Awake()
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            if (effectsVolumeSlider != null)
                effectsVolumeSlider.onValueChanged.AddListener(SetEffectsVolume);
            interfaceSoundsToggle.onValueChanged.AddListener(SetInterfaceSounds);
            interfaceVolumeSlider.onValueChanged.AddListener(SetInterfaceVolume);
            displayModeDropdown.onValueChanged.AddListener(SetDisplayMode);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);

            if (backButton != null)
            {
                backButton.onClick.AddListener(PressBack);
            }

            displayModeDropdown.ClearOptions();
            displayModeDropdown.AddOptions(DisplayModeLabels.ToList());
        }

        /// <summary>What the Back button does. Also used for Escape.</summary>
        public void PressBack() => BackPressed?.Invoke();

        private void OnEnable()
        {
            GameSettings settings = SettingsService.Current;
            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            if (musicVolumeSlider != null)
                musicVolumeSlider.SetValueWithoutNotify(settings.MusicVolume);
            if (effectsVolumeSlider != null)
                effectsVolumeSlider.SetValueWithoutNotify(settings.EffectsVolume);
            interfaceSoundsToggle.SetIsOnWithoutNotify(settings.InterfaceSounds);
            interfaceVolumeSlider.SetValueWithoutNotify(settings.InterfaceVolume);
            interfaceVolumeSlider.interactable = settings.InterfaceSounds;
            displayModeDropdown.SetValueWithoutNotify(System.Array.IndexOf(DisplayModes, settings.DisplayMode));
            RefreshResolutions();
        }

        // Sliders change on every tick of a drag, so they're only saved when the page closes.
        private void OnDisable()
        {
            SettingsService.Current.Save();
        }

        private void SetMasterVolume(float value)
        {
            SettingsService.Current.MasterVolume = value;
            SettingsService.Current.ApplyAudio();
        }

        // Music follows its setting every frame. Effects are rescaled when audio is applied.
        private void SetMusicVolume(float value)
        {
            SettingsService.Current.MusicVolume = value;
        }

        private void SetEffectsVolume(float value)
        {
            SettingsService.Current.EffectsVolume = value;
            SettingsService.Current.ApplyAudio();
        }

        private void SetInterfaceSounds(bool isOn)
        {
            SettingsService.Current.InterfaceSounds = isOn;
            interfaceVolumeSlider.interactable = isOn;
            SettingsService.Current.Save();
        }

        private void SetInterfaceVolume(float value)
        {
            SettingsService.Current.InterfaceVolume = value;
        }

        private void SetDisplayMode(int index)
        {
            SettingsService.Current.DisplayMode = DisplayModes[index];
            ApplyAndSaveDisplay();
            RefreshResolutions();
        }

        private void SetResolution(int index)
        {
            SettingsService.Current.ResolutionWidth = _resolutions[index].x;
            SettingsService.Current.ResolutionHeight = _resolutions[index].y;
            ApplyAndSaveDisplay();
        }

        private static void ApplyAndSaveDisplay()
        {
            SettingsService.Current.ApplyDisplay();
            SettingsService.Current.Save();
        }

        private void RefreshResolutions()
        {
            GameSettings settings = SettingsService.Current;
            Vector2Int current = settings.GetWindowSize(ScreenSize());
            _resolutions = ResolutionList.Build(
                Screen.resolutions.Select(resolution => new Vector2Int(resolution.width, resolution.height)),
                current);

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(_resolutions.Select(ResolutionList.Label).ToList());
            resolutionDropdown.SetValueWithoutNotify(_resolutions.IndexOf(current));

            // Borderless windowed always fills the screen, so there's nothing to choose.
            resolutionDropdown.interactable = settings.DisplayMode != FullScreenMode.FullScreenWindow;
        }

        private static Vector2Int ScreenSize()
        {
            return new Vector2Int(Display.main.systemWidth, Display.main.systemHeight);
        }
    }
}
