using UnityEngine;

namespace TunnelVision.Level
{
    // Stats kept forever (saved in PlayerPrefs) for the main menu's Stats page: each level's fastest
    // time and fewest lives lost, plus totals over every clear. Only saved when a level is cleared.
    // Most teddies per level is kept in LevelProgress. Levels are numbered from 0.
    public static class AllTimeStats
    {
        public const string ClearsKey = "AllTime.Clears";
        public const string SecondsKey = "AllTime.Seconds";
        public const string TeddiesKey = "AllTime.Teddies";
        public const string LivesLostKey = "AllTime.LivesLost";
        public const string DemoCompletionsKey = "AllTime.DemoCompletions";
        private const string BestTimeKeyPrefix = "AllTime.BestTime.Level";
        private const string FewestLivesLostKeyPrefix = "AllTime.FewestLivesLost.Level";

        public static string BestTimeKey(int level) => BestTimeKeyPrefix + level;

        public static string FewestLivesLostKey(int level) => FewestLivesLostKeyPrefix + level;

        public static int Clears => PlayerPrefs.GetInt(ClearsKey, 0);

        public static float TotalSeconds => PlayerPrefs.GetFloat(SecondsKey, 0f);

        public static int TotalTeddies => PlayerPrefs.GetInt(TeddiesKey, 0);

        public static int TotalLivesLost => PlayerPrefs.GetInt(LivesLostKey, 0);

        public static int DemoCompletions => PlayerPrefs.GetInt(DemoCompletionsKey, 0);

        public static bool HasCleared(int level) => PlayerPrefs.HasKey(BestTimeKey(level));

        // Returns 0 if the level has never been cleared.
        public static float BestTime(int level) => PlayerPrefs.GetFloat(BestTimeKey(level), 0f);

        // Returns 0 if the level has never been cleared.
        public static int FewestLivesLost(int level) => PlayerPrefs.GetInt(FewestLivesLostKey(level), 0);

        // Adds a cleared level to the totals and saves any new best time or fewest lives lost.
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
