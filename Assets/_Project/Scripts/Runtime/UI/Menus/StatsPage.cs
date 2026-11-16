using System.Text;
using TMPro;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// The main menu's Stats page: a table of each level's best clear, then totals over every
    /// playthrough. Fills its text each time the page opens.
    /// </summary>
    public class StatsPage : MonoBehaviour
    {
        public const string AllTimeHeading = "All Time";
        public const string NotCleared = "Not Cleared Yet";
        public static readonly string[] Columns = { "Best Time", "Collectibles", "Lives Lost" };

        // Where each column starts, across the text box. The first column holds the row's name.
        private static readonly string[] ColumnStarts = { "32%", "54%", "78%" };

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

        /// <summary>
        /// The page's text, read from the saved all-time stats, as a table: a header row, each level's
        /// best clear, then the totals over every clear. The text box should be left-aligned.
        /// </summary>
        public static string StatsText(int levelCount)
        {
            var text = new StringBuilder();
            text.AppendLine("<size=80%><alpha=#AA>" + Row("", Columns) + "<alpha=#FF></size>");

            for (var level = 0; level < levelCount; level++)
            {
                var label = $"Level {level + 1}";
                text.AppendLine(AllTimeStats.HasCleared(level)
                    ? Row(label, LevelCompleteScreen.FormatTime(AllTimeStats.BestTime(level)),
                        LevelProgress.BestCollectibles(level).ToString(), AllTimeStats.FewestLivesLost(level).ToString())
                    : Row(label, NotCleared));
            }

            text.AppendLine("<size=40%> </size>");
            text.AppendLine("<b>" + Row(AllTimeHeading, LevelCompleteScreen.FormatTime(AllTimeStats.TotalSeconds),
                AllTimeStats.TotalTeddies.ToString(), AllTimeStats.TotalLivesLost.ToString()) + "</b>");
            text.Append(Row($"Levels Cleared  {AllTimeStats.Clears}", "", $"Demo Completed  {AllTimeStats.DemoCompletions}"));
            return text.ToString();
        }

        /// <summary>One table row: the name, then each value at its column. Empty values are skipped.</summary>
        public static string Row(string label, params string[] values)
        {
            var row = new StringBuilder(label);
            for (var i = 0; i < values.Length && i < ColumnStarts.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    row.Append($"<pos={ColumnStarts[i]}>{values[i]}");
                }
            }

            return row.ToString();
        }
    }
}
