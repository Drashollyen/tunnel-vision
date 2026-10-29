using TunnelVision.Settings;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Plays menu hover and click sounds at the player's interface volume.
    /// Put one in each menu scene; <see cref="UiSelectableSounds"/> on each control calls it.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class UiSoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private AudioSource _source;

        public static UiSoundPlayer Instance { get; private set; }

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void PlayHover() => Play(hoverSound);

        public void PlayClick() => Play(clickSound);

        private void Play(AudioClip clip)
        {
            var volume = SettingsService.Current?.EffectiveInterfaceVolume ?? GameSettings.DefaultInterfaceVolume;
            if (clip != null && volume > 0f)
            {
                _source.PlayOneShot(clip, volume);
            }
        }
    }
}
