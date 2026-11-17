using UnityEngine;

namespace TunnelVision.Settings
{
    // Holds the game's one set of settings. It loads and applies the saved values before the
    // first scene loads, so every scene starts with the player's settings.
    public static class SettingsService
    {
        public static GameSettings Current { get; private set; }

        // Runs at the start of every Play session, even with Fast Enter Play Mode on.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Current = new GameSettings(new PlayerPrefsSettingsStore());
            Current.Load();
            Current.Apply();
        }
    }
}
