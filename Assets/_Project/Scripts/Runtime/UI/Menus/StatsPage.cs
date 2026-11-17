using System.Collections.Generic;
using TMPro;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.UI
{
    // The main menu's Stats page: each level's best clear on the left, all-time totals on the right.
    // The text is rebuilt every time the page opens.
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
        [Tooltip("How many levels the game has. Each gets a line, cleared or not.")]
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

        public static string BestText(int levelCount) => string.Join("\n", BestLines(levelCount));

        public static string AllTimeText() => string.Join("\n", AllTimeLines());

        // The heading, then each level's fastest time with its other bests on a smaller line below.
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

        public static List<string> AllTimeLines() => new List<string>
        {
            Title(AllTimeHeading),
            $"Levels Cleared:  {AllTimeStats.Clears}",
            $"Demo Completed:  {AllTimeStats.DemoCompletions}",
            $"Time Played:  {LevelCompleteScreen.FormatTime(AllTimeStats.TotalSeconds)}",
            $"Collectibles:  {AllTimeStats.TotalTeddies}",
            $"Lives Lost:  {AllTimeStats.TotalLivesLost}",
        };

        // <size> and <b> are TextMeshPro tags that make the heading bigger and bold.
        private static string Title(string title) => $"<size={TitleSize}><b>{title}</b></size>";

        // "1 Collectible" or "3 Collectibles", picking the right word for the number.
        private static string Count(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";
    }
}
