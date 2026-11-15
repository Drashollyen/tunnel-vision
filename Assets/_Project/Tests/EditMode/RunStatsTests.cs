using NUnit.Framework;
using TunnelVision.Level;

namespace TunnelVision.Tests
{
    public class RunStatsTests
    {
        [SetUp]
        [TearDown]
        public void Clear() => RunStats.Reset();

        [Test]
        public void NewRun_HasNoResults()
        {
            Assert.IsEmpty(RunStats.All);
            Assert.AreEqual(0f, RunStats.TotalSeconds);
            Assert.AreEqual(0, RunStats.TotalTeddies);
            Assert.AreEqual(0, RunStats.TotalLivesLost);
        }

        [Test]
        public void Results_AreInLevelOrder_AndTotalled()
        {
            RunStats.Record(new RunStats.LevelResult(1, 90f, 2, 1));
            RunStats.Record(new RunStats.LevelResult(0, 60f, 3, 0));

            Assert.AreEqual(0, RunStats.All[0].Level);
            Assert.AreEqual(1, RunStats.All[1].Level);
            Assert.AreEqual(150f, RunStats.TotalSeconds, 0.001f);
            Assert.AreEqual(5, RunStats.TotalTeddies);
            Assert.AreEqual(1, RunStats.TotalLivesLost);
        }

        [Test]
        public void ClearingALevelAgain_ReplacesItsResult()
        {
            RunStats.Record(new RunStats.LevelResult(0, 60f, 1, 2));
            RunStats.Record(new RunStats.LevelResult(0, 45f, 3, 0));

            Assert.AreEqual(1, RunStats.All.Count);
            Assert.AreEqual(45f, RunStats.All[0].Seconds, 0.001f);
            Assert.AreEqual(3, RunStats.TotalTeddies);
        }

        [Test]
        public void Reset_StartsAFreshRun()
        {
            RunStats.Record(new RunStats.LevelResult(0, 60f, 1, 2));
            RunStats.Reset();

            Assert.IsEmpty(RunStats.All);
        }
    }
}
