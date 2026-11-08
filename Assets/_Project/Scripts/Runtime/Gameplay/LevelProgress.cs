using UnityEngine;

namespace TunnelVision.Gameplay
{
    /// <summary>Remembers how far the player has got. Levels are numbered from 0.</summary>
    public static class LevelProgress
    {
        private const string Key = "HighestLevelUnlocked";

        public static int HighestUnlocked => PlayerPrefs.GetInt(Key, 0);

        public static bool IsUnlocked(int level) => level <= HighestUnlocked;

        public static void CompleteLevel(int level)
        {
            if (level + 1 > HighestUnlocked)
            {
                PlayerPrefs.SetInt(Key, level + 1);
                PlayerPrefs.Save();
            }
        }
    }
}
