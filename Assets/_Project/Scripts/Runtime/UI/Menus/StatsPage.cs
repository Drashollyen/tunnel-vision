using System.Collections.Generic;
using System.Text;
using TMPro;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// The main menu's Stats page, in two columns: each level's best clear on the left, totals over
    /// every playthrough on the right, each under a slightly larger title. Fills its text each time
    /// the page opens.
    /// </summary>
    public class StatsPage : MonoBehaviour
    {
        public const string BestHeading = "Best Clears";
        public const string AllTimeHeading = "All Time";
        public const string NotCleared = "Not Cleared Yet";

        // Where the right column starts, across the text box.
        private const string RightColumn = "54%";
        private const string TitleSize = "120%";
        private const string DetailSize = "80%";

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

        /// <summary>The page's text, read from the saved all-time stats. The text box should be left-aligned.</summary>
        public static string StatsText(int levelCount) => Columns(BestLines(levelCount), AllTimeLines());

        /// <summary>The left column: the title, then each level's fastest time with its other bests below.</summary>
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

        /// <summary>The right column: the title, then the totals over every clear.</summary>
        public static List<string> AllTimeLines() => new List<string>
        {
            Title(AllTimeHeading),
            $"Levels Cleared:  {AllTimeStats.Clears}",
            $"Demo Completed:  {AllTimeStats.DemoCompletions}",
            $"Time Played:  {LevelCompleteScreen.FormatTime(AllTimeStats.TotalSeconds)}",
            $"Collectibles:  {AllTimeStats.TotalTeddies}",
            $"Lives Lost:  {AllTimeStats.TotalLivesLost}",
        };

        /// <summary>Puts two columns side by side, line for line; the shorter one just ends early.</summary>
        public static string Columns(IReadOnlyList<string> left, IReadOnlyList<string> right)
        {
            var text = new StringBuilder();
            var count = Mathf.Max(left.Count, right.Count);
            for (var i = 0; i < count; i++)
            {
                if (i > 0)
                {
                    text.Append('\n');
                }

                if (i < left.Count)
                {
                    text.Append(left[i]);
                }

                if (i < right.Count)
                {
                    text.Append($"<pos={RightColumn}>{right[i]}");
                }
            }

            return text.ToString();
        }

        private static string Title(string title) => $"<size={TitleSize}><b>{title}</b></size>";

        private static string Count(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";
    }
}
