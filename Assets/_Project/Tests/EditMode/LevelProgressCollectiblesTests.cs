using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class LevelProgressCollectiblesTests
    {
        private const int TestLevel = 97; // A level number no real save uses.

        [SetUp]
        [TearDown]
        public void Clear()
        {
            PlayerPrefs.DeleteKey(LevelProgress.CollectiblesKey(TestLevel));
        }

        [Test]
        public void NeverCleared_HasNoTeddiesSaved()
        {
            Assert.AreEqual(0, LevelProgress.BestCollectibles(TestLevel));
        }

        [Test]
        public void Record_SavesTheCount()
        {
            LevelProgress.RecordCollectibles(TestLevel, 2);

            Assert.AreEqual(2, LevelProgress.BestCollectibles(TestLevel));
        }

        [Test]
        public void Record_KeepsTheBestRun()
        {
            LevelProgress.RecordCollectibles(TestLevel, 3);
            LevelProgress.RecordCollectibles(TestLevel, 1);

            Assert.AreEqual(3, LevelProgress.BestCollectibles(TestLevel), "A worse clear doesn't lower it.");
        }
    }
}
