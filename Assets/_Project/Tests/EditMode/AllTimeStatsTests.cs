using System.Collections.Generic;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class AllTimeStatsTests
    {
        private const int TestLevel = 96; // A level number no real save uses.

        private static readonly string[] TotalKeys =
        {
            AllTimeStats.ClearsKey, AllTimeStats.SecondsKey, AllTimeStats.TeddiesKey,
            AllTimeStats.LivesLostKey, AllTimeStats.DemoCompletionsKey,
        };

        // The totals are shared with real saves, so put back whatever was there.
        private readonly Dictionary<string, string> _saved = new Dictionary<string, string>();

        [SetUp]
        public void SaveTotals()
        {
            _saved.Clear();
            foreach (var key in TotalKeys)
            {
                if (PlayerPrefs.HasKey(key))
                {
                    _saved[key] = key == AllTimeStats.SecondsKey
                        ? PlayerPrefs.GetFloat(key).ToString("R")
                        : PlayerPrefs.GetInt(key).ToString();
                }

                PlayerPrefs.DeleteKey(key);
            }

            ClearTestLevel();
        }

        [TearDown]
        public void RestoreTotals()
        {
            foreach (var key in TotalKeys)
            {
                PlayerPrefs.DeleteKey(key);
                if (_saved.TryGetValue(key, out var value))
                {
                    if (key == AllTimeStats.SecondsKey)
                        PlayerPrefs.SetFloat(key, float.Parse(value));
                    else
                        PlayerPrefs.SetInt(key, int.Parse(value));
                }
            }

            ClearTestLevel();
            PlayerPrefs.Save();
        }

        private static void ClearTestLevel()
        {
            PlayerPrefs.DeleteKey(AllTimeStats.BestTimeKey(TestLevel));
            PlayerPrefs.DeleteKey(AllTimeStats.FewestLivesLostKey(TestLevel));
        }

        [Test]
        public void NothingCleared_EverythingIsZero()
        {
            Assert.That(AllTimeStats.HasCleared(TestLevel), Is.False);
            Assert.That(AllTimeStats.Clears, Is.EqualTo(0));
            Assert.That(AllTimeStats.TotalSeconds, Is.EqualTo(0f));
            Assert.That(AllTimeStats.TotalTeddies, Is.EqualTo(0));
            Assert.That(AllTimeStats.TotalLivesLost, Is.EqualTo(0));
            Assert.That(AllTimeStats.DemoCompletions, Is.EqualTo(0));
        }

        [Test]
        public void RecordClear_AddsUpTheTotals()
        {
            AllTimeStats.RecordClear(TestLevel, 60f, 2, 1, false);
            AllTimeStats.RecordClear(TestLevel, 90f, 3, 2, false);

            Assert.That(AllTimeStats.Clears, Is.EqualTo(2));
            Assert.That(AllTimeStats.TotalSeconds, Is.EqualTo(150f).Within(0.01f));
            Assert.That(AllTimeStats.TotalTeddies, Is.EqualTo(5));
            Assert.That(AllTimeStats.TotalLivesLost, Is.EqualTo(3));
        }

        [Test]
        public void RecordClear_KeepsTheFastestTimeAndFewestLivesLost()
        {
            AllTimeStats.RecordClear(TestLevel, 80f, 0, 0, false);
            AllTimeStats.RecordClear(TestLevel, 50f, 0, 3, false);
            AllTimeStats.RecordClear(TestLevel, 70f, 0, 1, false);

            Assert.That(AllTimeStats.HasCleared(TestLevel), Is.True);
            Assert.That(AllTimeStats.BestTime(TestLevel), Is.EqualTo(50f).Within(0.01f));
            Assert.That(AllTimeStats.FewestLivesLost(TestLevel), Is.EqualTo(0), "A clear with no deaths stays the best.");
        }

        [Test]
        public void RecordClear_CountsDemoCompletionsOnlyOnTheLastLevel()
        {
            AllTimeStats.RecordClear(TestLevel, 10f, 0, 0, false);
            AllTimeStats.RecordClear(TestLevel, 10f, 0, 0, true);

            Assert.That(AllTimeStats.DemoCompletions, Is.EqualTo(1));
        }

        [Test]
        public void StatsText_HasBothTitles_AndAnEmptyRecord()
        {
            var best = StatsPage.BestText(1);
            var allTime = StatsPage.AllTimeText();

            Assert.That(best, Does.StartWith("<size=").And.Contain(StatsPage.BestHeading), "The title comes first, larger.");
            Assert.That(allTime, Does.StartWith("<size=").And.Contain(StatsPage.AllTimeHeading));
            Assert.That(allTime, Does.Contain("Levels Cleared:  0"));
            Assert.That(allTime, Does.Contain("Demo Completed:  0"));
            Assert.That(allTime, Does.Contain("Lives Lost:  0"));
        }

        [Test]
        public void BestLines_ShowEachLevelsBests_OrNotCleared()
        {
            AllTimeStats.RecordClear(TestLevel, 75f, 2, 1, false);

            var lines = StatsPage.BestLines(TestLevel + 1);

            Assert.That(lines, Has.Some.EqualTo($"Level {TestLevel}:  {StatsPage.NotCleared}"));
            var timeLine = lines.IndexOf($"Level {TestLevel + 1}:  {LevelCompleteScreen.FormatTime(75f)}");
            Assert.That(timeLine, Is.GreaterThan(0), "The cleared level shows its best time.");
            Assert.That(lines[timeLine + 1], Does.Contain("1 Life Lost"), "Its other bests go on the line below.");
        }

        [Test]
        public void AllTimeLines_ShowTheTotals()
        {
            AllTimeStats.RecordClear(TestLevel, 75f, 2, 1, true);

            var lines = StatsPage.AllTimeLines();

            Assert.That(lines, Has.Some.EqualTo("Levels Cleared:  1"));
            Assert.That(lines, Has.Some.EqualTo("Demo Completed:  1"));
            Assert.That(lines, Has.Some.EqualTo($"Time Played:  {LevelCompleteScreen.FormatTime(75f)}"));
            Assert.That(lines, Has.Some.EqualTo("Collectibles:  2"));
            Assert.That(lines, Has.Some.EqualTo("Lives Lost:  1"));
        }
    }
}
