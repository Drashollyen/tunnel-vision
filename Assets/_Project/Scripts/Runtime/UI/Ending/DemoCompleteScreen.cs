using System.Collections.Generic;
using System.Text;
using TMPro;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// The end of the demo: "Congratulations!" pops in over the cast celebrating together, with
    /// this run's stats for each level and in total. Main Menu and Level Select buttons, plus
    /// Escape and Enter, as the key hints say.
    /// </summary>
    public class DemoCompleteScreen : MonoBehaviour
    {
        public const string Title = "Congratulations!";
        public const string Subtitle = "You've Completed The Tunnel Vision Demo";
        public const string NoStats = "No Levels Cleared This Run";
        public const float TitleDelay = 0.3f;
        public const float TitlePopSeconds = 0.5f;

        [SerializeField] private RectTransform title;
        [SerializeField] private TMP_Text stats;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private string mainMenuScene = "IntroScreen";
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private float _startedAt;

        /// <summary>One line per cleared level, then the totals.</summary>
        public static string StatsText(IReadOnlyList<RunStats.LevelResult> results)
        {
            if (results == null || results.Count == 0)
            {
                return NoStats;
            }

            var text = new StringBuilder();
            float seconds = 0f;
            int teddies = 0, livesLost = 0;
            foreach (var result in results)
            {
                text.AppendLine(Line($"Level {result.Level + 1}", result.Seconds, result.Teddies, result.LivesLost));
                seconds += result.Seconds;
                teddies += result.Teddies;
                livesLost += result.LivesLost;
            }

            text.Append(Line("Total", seconds, teddies, livesLost));
            return text.ToString();
        }

        private static string Line(string label, float seconds, int teddies, int livesLost) =>
            $"{label}:  {LevelCompleteScreen.FormatTime(seconds)}  ·  " +
            $"{teddies} {(teddies == 1 ? "Teddy" : "Teddies")}  ·  " +
            $"{livesLost} {(livesLost == 1 ? "Life" : "Lives")} Lost";

        /// <summary>The title's size this long after the screen opens: it pops past full size, then settles.</summary>
        public static float TitleScaleAt(float elapsed)
        {
            var t = Mathf.Clamp01((elapsed - TitleDelay) / TitlePopSeconds);
            if (t < 0.7f)
            {
                return Mathf.Lerp(0f, 1.15f, t / 0.7f);
            }

            return Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
        }

        private void Awake()
        {
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(GoToLevelSelect);
        }

        private void Start()
        {
            _startedAt = Time.unscaledTime;
            if (stats != null)
                stats.text = StatsText(RunStats.All);
            if (title != null)
                title.localScale = Vector3.zero;
        }

        private void Update()
        {
            if (title != null)
            {
                title.localScale = Vector3.one * TitleScaleAt(Time.unscaledTime - _startedAt);
            }

            var keyboard = Keyboard.current;
            if (keyboard == null || SceneFader.IsFading)
            {
                return;
            }

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                GoToMainMenu();
            }
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
            {
                GoToLevelSelect();
            }
        }

        public void GoToMainMenu() => SceneFader.Load(mainMenuScene);

        public void GoToLevelSelect() => SceneFader.Load(levelSelectScene);
    }
}
