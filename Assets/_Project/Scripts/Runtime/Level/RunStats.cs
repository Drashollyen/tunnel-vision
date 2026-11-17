using System.Collections.Generic;
using System.Linq;

namespace TunnelVision.Level
{
    // This playthrough's results for each cleared level (time, teddies, lives lost), shown on the
    // demo's ending. Only kept in memory, not saved. Pressing Play on the main menu clears them.
    public static class RunStats
    {
        public readonly struct LevelResult
        {
            public readonly int Level;
            public readonly float Seconds;
            public readonly int Teddies;
            public readonly int LivesLost;

            public LevelResult(int level, float seconds, int teddies, int livesLost)
            {
                Level = level;
                Seconds = seconds;
                Teddies = teddies;
                LivesLost = livesLost;
            }
        }

        private static readonly Dictionary<int, LevelResult> Results = new Dictionary<int, LevelResult>();

        // Also runs when Play mode starts, so old results don't carry over when
        // Fast Enter Play Mode keeps static data between runs.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() => Results.Clear();

        // Replaces any earlier result for the same level.
        public static void Record(LevelResult result) => Results[result.Level] = result;

        // Sorted by level.
        public static IReadOnlyList<LevelResult> All => Results.Values.OrderBy(r => r.Level).ToList();

        public static float TotalSeconds => Results.Values.Sum(r => r.Seconds);
        public static int TotalTeddies => Results.Values.Sum(r => r.Teddies);
        public static int TotalLivesLost => Results.Values.Sum(r => r.LivesLost);
    }
}
