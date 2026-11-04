using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Connects the options page controls to the saved settings.
    /// Every change is applied and saved straight away, so there is no Apply button.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle interfaceSoundsToggle;
        [SerializeField] private Slider interfaceVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        private void Awake()
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            interfaceSoundsToggle.onValueChanged.AddListener(SetInterfaceSounds);
            interfaceVolumeSlider.onValueChanged.AddListener(SetInterfaceVolume);
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        private void OnEnable()
        {
            GameSettings settings = SettingsService.Current;
            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            interfaceSoundsToggle.SetIsOnWithoutNotify(settings.InterfaceSounds);
            interfaceVolumeSlider.SetValueWithoutNotify(settings.InterfaceVolume);
            interfaceVolumeSlider.interactable = settings.InterfaceSounds;
            fullscreenToggle.SetIsOnWithoutNotify(settings.Fullscreen);
        }

        private void SetMasterVolume(float value)
        {
            SettingsService.Current.MasterVolume = value;
            ApplyAndSave();
        }

        private void SetInterfaceSounds(bool isOn)
        {
            SettingsService.Current.InterfaceSounds = isOn;
            interfaceVolumeSlider.interactable = isOn;
            ApplyAndSave();
        }

        private void SetInterfaceVolume(float value)
        {
            SettingsService.Current.InterfaceVolume = value;
            ApplyAndSave();
        }

        private void SetFullscreen(bool isOn)
        {
            SettingsService.Current.Fullscreen = isOn;
            ApplyAndSave();
        }

        private static void ApplyAndSave()
        {
            SettingsService.Current.Apply();
            SettingsService.Current.Save();
        }
    }
}
