using UnityEngine;

namespace TunnelVision.Settings
{
    /// <summary>
    /// Holds the game's single <see cref="GameSettings"/> instance and applies the saved
    /// values before the first scene loads, so every scene starts with the player's settings.
    /// </summary>
    public static class SettingsService
    {
        public static GameSettings Current { get; private set; }

        // Runs on every Play session, including with Fast Enter Play Mode (no domain reload).
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Current = new GameSettings(new PlayerPrefsSettingsStore());
            Current.Load();
            Current.Apply();
        }
    }
}
