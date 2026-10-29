using TMPro;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Binds the options panel controls to <see cref="SettingsService.Current"/>.
    /// Changes apply immediately and are saved when the panel closes.
    /// </summary>
    public sealed class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeLabel;
        [SerializeField] private Slider interfaceVolumeSlider;
        [SerializeField] private TMP_Text interfaceVolumeLabel;
        [SerializeField] private Toggle interfaceSoundsToggle;
        [SerializeField] private Toggle fullscreenToggle;

        [Tooltip("Label text for the volume sliders. {0} is the label name, {1} the percentage.")]
        [SerializeField] private string volumeFormat = "{0}: {1}%";

        private GameSettings Settings => SettingsService.Current;

        private void OnEnable()
        {
            RefreshControls();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            interfaceVolumeSlider.onValueChanged.AddListener(OnInterfaceVolumeChanged);
            interfaceSoundsToggle.onValueChanged.AddListener(OnInterfaceSoundsChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void OnDisable()
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            interfaceVolumeSlider.onValueChanged.RemoveListener(OnInterfaceVolumeChanged);
            interfaceSoundsToggle.onValueChanged.RemoveListener(OnInterfaceSoundsChanged);
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            Settings?.Save();
        }

        public void ResetToDefaults()
        {
            Settings.ResetToDefaults();
            Settings.Apply();
            RefreshControls();
        }

        private void RefreshControls()
        {
            if (Settings == null)
            {
                return;
            }

            masterVolumeSlider.SetValueWithoutNotify(Settings.MasterVolume);
            interfaceVolumeSlider.SetValueWithoutNotify(Settings.InterfaceVolume);
            interfaceSoundsToggle.SetIsOnWithoutNotify(Settings.InterfaceSounds);
            fullscreenToggle.SetIsOnWithoutNotify(Settings.Fullscreen);
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            masterVolumeLabel.text = string.Format(volumeFormat, "Master volume", Percent(Settings.MasterVolume));
            interfaceVolumeLabel.text = string.Format(volumeFormat, "Interface sounds", Percent(Settings.InterfaceVolume));
        }

        private static int Percent(float value) => Mathf.RoundToInt(value * 100f);

        private void OnMasterVolumeChanged(float value)
        {
            Settings.MasterVolume = value;
            Settings.Apply();
            RefreshLabels();
        }

        private void OnInterfaceVolumeChanged(float value)
        {
            Settings.InterfaceVolume = value;
            RefreshLabels();
        }

        private void OnInterfaceSoundsChanged(bool isOn)
        {
            Settings.InterfaceSounds = isOn;
        }

        private void OnFullscreenChanged(bool isOn)
        {
            Settings.Fullscreen = isOn;
            Settings.Apply();
        }
    }
}
