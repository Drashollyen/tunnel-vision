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
        private static AudioSource _player;

        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;
        [Tooltip("How loud the click is compared with the hover sound, before the player's settings.")]
        [SerializeField, Range(0f, 1f)] private float clickVolume = 0.6f;
        [Tooltip("Played when a greyed out control is clicked, at the same volume as the click.")]
        [SerializeField] private AudioClip disabledClickClip;

        private void Awake()
        {
            foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
            {
                selectable.gameObject.AddComponent<InterfaceSoundTrigger>();

                if (selectable is Button)
                {
                    selectable.gameObject.AddComponent<ButtonHover>();
                }
            }
        }

        /// <summary>Raised whenever a clip is played, so tests can tell which sound went off.</summary>
        public static event System.Action<AudioClip> ClipPlayed;

        public void PlayHover() => Play(hoverClip, 1f);

        public void PlayClick() => Play(clickClip, clickVolume);

        public void PlayDisabledClick() => Play(disabledClickClip, clickVolume);

        /// <summary>
        /// Plays any interface sound through the shared player, so it follows the player's
        /// interface settings and isn't cut off by a scene change. Does nothing for a null clip.
        /// </summary>
        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null)
            {
                return;
            }

            ClipPlayed?.Invoke(clip);

            if (_player == null)
            {
                var playerObject = new GameObject(PlayerName);
                DontDestroyOnLoad(playerObject);
                _player = playerObject.AddComponent<AudioSource>();
                _player.playOnAwake = false;
            }

            var settingsVolume = SettingsService.Current != null ? SettingsService.Current.EffectiveInterfaceVolume : 1f;
            _player.PlayOneShot(clip, volume * settingsVolume);
        }
    }
}
