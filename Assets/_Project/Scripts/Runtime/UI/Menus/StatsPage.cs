using System.Text;
using TMPro;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// The main menu's Stats page: each level's best clear, then totals over every playthrough.
    /// Uses the same lines as the demo's ending. Fills its text each time the page opens.
    /// </summary>
    public class StatsPage : MonoBehaviour
    {
        public const string BestHeading = "Best Clears";
        public const string AllTimeHeading = "All Time";
        public const string NotCleared = "Not Cleared Yet";

        [SerializeField] private TMP_Text stats;
        [Tooltip("How many levels the prototype has. Each gets a line, cleared or not.")]
        [SerializeField] private int levelCount = 3;

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.text = StatsText(levelCount);
            }
        }

        /// <summary>The page's text, read from the saved all-time stats.</summary>
        public static string StatsText(int levelCount)
        {
            var text = new StringBuilder();
            text.AppendLine($"<b>{BestHeading}</b>");
            for (var level = 0; level < levelCount; level++)
            {
                var label = $"Level {level + 1}";
                text.AppendLine(AllTimeStats.HasCleared(level)
                    ? DemoCompleteScreen.Line(label, AllTimeStats.BestTime(level),
                        LevelProgress.BestCollectibles(level), AllTimeStats.FewestLivesLost(level))
                    : $"{label}:  {NotCleared}");
            }

            text.AppendLine();
            text.AppendLine($"<b>{AllTimeHeading}</b>");
            text.AppendLine($"Levels Cleared: {AllTimeStats.Clears}  ·  Demo Completed: {AllTimeStats.DemoCompletions}");
            text.Append(DemoCompleteScreen.Line("Total", AllTimeStats.TotalSeconds,
                AllTimeStats.TotalTeddies, AllTimeStats.TotalLivesLost));
            return text.ToString();
        }
    }
}
