using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Remembers how far the player has got, and the most teddies found in each cleared level.
    /// Levels are numbered from 0. Teddies are only saved when a level is cleared: leaving or
    /// running out of lives keeps nothing.
    /// </summary>
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

        /// <summary>The PlayerPrefs key holding a level's best teddy count.</summary>
        public static string CollectiblesKey(int level) => CollectiblesKeyPrefix + level;

        /// <summary>The most teddies found in one clear of the level, or 0 if it's never been cleared.</summary>
        public static int BestCollectibles(int level) => PlayerPrefs.GetInt(CollectiblesKey(level), 0);

        /// <summary>Saves the teddies found on a clear, keeping the best run.</summary>
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
