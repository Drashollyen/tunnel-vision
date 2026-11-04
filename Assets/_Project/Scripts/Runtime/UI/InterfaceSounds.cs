using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Goes on a Canvas and gives every button, slider and toggle under it a hover and click sound.
    /// The sounds follow the interface settings on the options page.
    /// </summary>
    public class InterfaceSounds : MonoBehaviour
    {
        public const string PlayerName = "Interface Sounds Player";

        // Kept across scenes, so the Play button's click isn't cut off when the level loads.
        private static AudioSource s_player;

        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [Tooltip("How loud the click is compared with the hover sound, before the player's settings.")]
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.6f;

        private void Awake()
        {
            foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
            {
                selectable.gameObject.AddComponent<InterfaceSoundTrigger>().Initialize(this);
            }
        }

        public void PlayHover() => Play(hoverClip, 1f);

        public void PlayClick() => Play(clickClip, clickVolume);

        private static void Play(AudioClip clip, float volume)
        {
            if (clip == null)
            {
                return;
            }

            if (s_player == null)
            {
                var playerObject = new GameObject(PlayerName);
                DontDestroyOnLoad(playerObject);
                s_player = playerObject.AddComponent<AudioSource>();
                s_player.playOnAwake = false;
            }

            s_player.PlayOneShot(clip, volume * SettingsService.Current.EffectiveInterfaceVolume);
        }
    }
}
