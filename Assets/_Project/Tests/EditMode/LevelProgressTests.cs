using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class LevelProgressTests
    {
        private const string Key = "HighestLevelUnlocked";

        private int _savedValue;
        private bool _hadValue;

        [SetUp]
        public void ClearProgress()
        {
            _hadValue = PlayerPrefs.HasKey(Key);
            _savedValue = PlayerPrefs.GetInt(Key, 0);
            PlayerPrefs.DeleteKey(Key);
        }

        [TearDown]
        public void RestoreProgress()
        {
            if (_hadValue)
            {
                PlayerPrefs.SetInt(Key, _savedValue);
            }
            else
            {
                PlayerPrefs.DeleteKey(Key);
            }

            PlayerPrefs.Save();
        }

        [Test]
        public void OnlyFirstLevelIsUnlockedAtTheStart()
        {
            Assert.IsTrue(LevelProgress.IsUnlocked(0));
            Assert.IsFalse(LevelProgress.IsUnlocked(1));
        }

        [Test]
        public void CompletingALevelUnlocksTheNextOnly()
        {
            LevelProgress.CompleteLevel(0);

            Assert.IsTrue(LevelProgress.IsUnlocked(1));
            Assert.IsFalse(LevelProgress.IsUnlocked(2));
        }

        [Test]
        public void ReplayingAnEarlierLevelDoesNotLoseProgress()
        {
            LevelProgress.CompleteLevel(2);
            LevelProgress.CompleteLevel(0);

            Assert.AreEqual(3, LevelProgress.HighestUnlocked);
        }
    }
}
