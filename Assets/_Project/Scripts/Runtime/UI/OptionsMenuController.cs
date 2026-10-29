using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Binds the options panel controls to <see cref="SettingsService.Current"/>.
    /// Changes apply immediately and are saved when the panel closes.
    /// UI click sounds (volume and on/off) are handled by the Bloodlines UI SoundManager.
    /// </summary>
    public sealed class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Toggle fullscreenToggle;

        private GameSettings Settings => SettingsService.Current;

        private void OnEnable()
        {
            RefreshControls();
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        private void OnDisable()
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
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
            fullscreenToggle.SetIsOnWithoutNotify(Settings.Fullscreen);
        }

        private void OnMasterVolumeChanged(float value)
        {
            Settings.MasterVolume = value;
            Settings.Apply();
        }

        private void OnFullscreenChanged(bool isOn)
        {
            Settings.Fullscreen = isOn;
            Settings.Apply();
        }
    }
}
