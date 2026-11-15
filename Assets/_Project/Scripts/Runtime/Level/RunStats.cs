using System.Collections.Generic;
using System.Linq;

namespace TunnelVision.Level
{
    /// <summary>
    /// The current playthrough's results, one per cleared level: time taken, teddies found and
    /// lives lost. The demo's ending shows them. Clearing a level again replaces its result;
    /// pressing Play on the main menu starts a fresh run. Lives only in memory, not saved.
    /// </summary>
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

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() => Results.Clear();

        /// <summary>Records a cleared level, replacing any earlier result for it this run.</summary>
        public static void Record(LevelResult result) => Results[result.Level] = result;

        /// <summary>This run's results, in level order.</summary>
        public static IReadOnlyList<LevelResult> All => Results.Values.OrderBy(r => r.Level).ToList();

        public static float TotalSeconds => Results.Values.Sum(r => r.Seconds);
        public static int TotalTeddies => Results.Values.Sum(r => r.Teddies);
        public static int TotalLivesLost => Results.Values.Sum(r => r.LivesLost);
    }
}
