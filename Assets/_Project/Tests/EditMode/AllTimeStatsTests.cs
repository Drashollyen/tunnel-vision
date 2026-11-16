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
        public void StatsText_ShowsUnclearedLevelsAndTotals()
        {
            var text = StatsPage.StatsText(1);

            Assert.That(text, Does.Contain(StatsPage.BestHeading));
            Assert.That(text, Does.Contain(StatsPage.AllTimeHeading));
            Assert.That(text, Does.Contain("Levels Cleared: 0"));
            Assert.That(text, Does.Contain("Total:"));
        }

        [Test]
        public void StatsText_UsesTheSameLinesAsTheDemoEnding()
        {
            AllTimeStats.RecordClear(TestLevel, 75f, 2, 1, false);

            var text = StatsPage.StatsText(TestLevel + 1);

            Assert.That(text, Does.Contain($"Level {TestLevel}:  {StatsPage.NotCleared}"));
            Assert.That(text, Does.Contain(DemoCompleteScreen.Line($"Level {TestLevel + 1}", 75f,
                LevelProgress.BestCollectibles(TestLevel), 1)));
            Assert.That(text, Does.Contain(DemoCompleteScreen.Line("Total", 75f, 2, 1)));
        }
    }
}
