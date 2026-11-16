using System.Collections.Generic;
using TMPro;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// The main menu's Stats page, in two columns, each its own centred text box: each level's best
    /// clear on the left, totals over every playthrough on the right, each under a larger title.
    /// Fills its text each time the page opens.
    /// </summary>
    public class StatsPage : MonoBehaviour
    {
        public const string BestHeading = "Best Clears";
        public const string AllTimeHeading = "All Time";
        public const string NotCleared = "Not Cleared Yet";

        private const string TitleSize = "130%";
        private const string DetailSize = "80%";

        [Tooltip("The left column.")]
        [SerializeField] private TMP_Text bestClears;
        [Tooltip("The right column.")]
        [SerializeField] private TMP_Text allTime;
        [Tooltip("How many levels the prototype has. Each gets a line, cleared or not.")]
        [SerializeField] private int levelCount = 3;

        private void OnEnable()
        {
            if (bestClears != null)
            {
                bestClears.text = BestText(levelCount);
            }

            if (allTime != null)
            {
                allTime.text = AllTimeText();
            }
        }

        /// <summary>The left column's text: the title, then each level's best clear.</summary>
        public static string BestText(int levelCount) => string.Join("\n", BestLines(levelCount));

        /// <summary>The right column's text: the title, then the totals.</summary>
        public static string AllTimeText() => string.Join("\n", AllTimeLines());

        /// <summary>The title, then each level's fastest time with its other bests on a smaller line below.</summary>
        public static List<string> BestLines(int levelCount)
        {
            var lines = new List<string> { Title(BestHeading) };
            for (var level = 0; level < levelCount; level++)
            {
                var label = $"Level {level + 1}";
                if (!AllTimeStats.HasCleared(level))
                {
                    lines.Add($"{label}:  {NotCleared}");
                    continue;
                }

                lines.Add($"{label}:  {LevelCompleteScreen.FormatTime(AllTimeStats.BestTime(level))}");
                lines.Add($"<size={DetailSize}>{Count(LevelProgress.BestCollectibles(level), "Collectible", "Collectibles")}  ·  " +
                          $"{Count(AllTimeStats.FewestLivesLost(level), "Life Lost", "Lives Lost")}</size>");
            }

            return lines;
        }

        /// <summary>The title, then the totals over every clear.</summary>
        public static List<string> AllTimeLines() => new List<string>
        {
            Title(AllTimeHeading),
            $"Levels Cleared:  {AllTimeStats.Clears}",
            $"Demo Completed:  {AllTimeStats.DemoCompletions}",
            $"Time Played:  {LevelCompleteScreen.FormatTime(AllTimeStats.TotalSeconds)}",
            $"Collectibles:  {AllTimeStats.TotalTeddies}",
            $"Lives Lost:  {AllTimeStats.TotalLivesLost}",
        };

        private static string Title(string title) => $"<size={TitleSize}><b>{title}</b></size>";

        private static string Count(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";
    }
}
