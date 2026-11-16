using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Stats kept across every playthrough, for the main menu's Stats page: each level's best
    /// clear (fastest time, fewest lives lost; the most teddies is <see cref="LevelProgress"/>'s),
    /// plus totals over every clear. Saved in PlayerPrefs, and only when a level is cleared, the
    /// same as the teddies. Levels are numbered from 0.
    /// </summary>
    public static class AllTimeStats
    {
        public const string ClearsKey = "AllTime.Clears";
        public const string SecondsKey = "AllTime.Seconds";
        public const string TeddiesKey = "AllTime.Teddies";
        public const string LivesLostKey = "AllTime.LivesLost";
        public const string DemoCompletionsKey = "AllTime.DemoCompletions";
        private const string BestTimeKeyPrefix = "AllTime.BestTime.Level";
        private const string FewestLivesLostKeyPrefix = "AllTime.FewestLivesLost.Level";

        /// <summary>The PlayerPrefs key holding a level's fastest clear, in seconds.</summary>
        public static string BestTimeKey(int level) => BestTimeKeyPrefix + level;

        /// <summary>The PlayerPrefs key holding the fewest lives lost in one clear of a level.</summary>
        public static string FewestLivesLostKey(int level) => FewestLivesLostKeyPrefix + level;

        /// <summary>How many times any level has been cleared.</summary>
        public static int Clears => PlayerPrefs.GetInt(ClearsKey, 0);

        /// <summary>The time taken over every clear, added up.</summary>
        public static float TotalSeconds => PlayerPrefs.GetFloat(SecondsKey, 0f);

        /// <summary>The teddies found over every clear, added up.</summary>
        public static int TotalTeddies => PlayerPrefs.GetInt(TeddiesKey, 0);

        /// <summary>The lives lost over every clear, added up.</summary>
        public static int TotalLivesLost => PlayerPrefs.GetInt(LivesLostKey, 0);

        /// <summary>How many times the demo's last level has been cleared.</summary>
        public static int DemoCompletions => PlayerPrefs.GetInt(DemoCompletionsKey, 0);

        /// <summary>True once the level has a clear saved here.</summary>
        public static bool HasCleared(int level) => PlayerPrefs.HasKey(BestTimeKey(level));

        /// <summary>The level's fastest clear in seconds, or 0 if it's never been cleared.</summary>
        public static float BestTime(int level) => PlayerPrefs.GetFloat(BestTimeKey(level), 0f);

        /// <summary>The fewest lives lost in one clear of the level, or 0 if it's never been cleared.</summary>
        public static int FewestLivesLost(int level) => PlayerPrefs.GetInt(FewestLivesLostKey(level), 0);

        /// <summary>Saves a cleared level: adds it to the totals and keeps any new bests.</summary>
        public static void RecordClear(int level, float seconds, int teddies, int livesLost, bool endsDemo)
        {
            PlayerPrefs.SetInt(ClearsKey, Clears + 1);
            PlayerPrefs.SetFloat(SecondsKey, TotalSeconds + seconds);
            PlayerPrefs.SetInt(TeddiesKey, TotalTeddies + teddies);
            PlayerPrefs.SetInt(LivesLostKey, TotalLivesLost + livesLost);

            if (endsDemo)
            {
                PlayerPrefs.SetInt(DemoCompletionsKey, DemoCompletions + 1);
            }

            var cleared = HasCleared(level);
            if (!cleared || seconds < BestTime(level))
            {
                PlayerPrefs.SetFloat(BestTimeKey(level), seconds);
            }

            if (!cleared || livesLost < FewestLivesLost(level))
            {
                PlayerPrefs.SetInt(FewestLivesLostKey(level), livesLost);
            }

            PlayerPrefs.Save();
        }
    }
}
