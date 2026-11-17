using UnityEngine;

namespace TunnelVision.Level
{
    // Saves which levels are unlocked and the most teddies found in each level.
    // Levels are numbered from 0. Teddies are only saved when a level is cleared.
    public static class LevelProgress
    {
        private const string Key = "HighestLevelUnlocked";
        private const string CollectiblesKeyPrefix = "CollectiblesFound.Level";

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

        public static string CollectiblesKey(int level) => CollectiblesKeyPrefix + level;

        public static int BestCollectibles(int level) => PlayerPrefs.GetInt(CollectiblesKey(level), 0);

        // Only saves the count if it beats the level's best.
        public static void RecordCollectibles(int level, int found)
        {
            if (found > BestCollectibles(level))
            {
                PlayerPrefs.SetInt(CollectiblesKey(level), found);
                PlayerPrefs.Save();
            }
        }
    }
}
