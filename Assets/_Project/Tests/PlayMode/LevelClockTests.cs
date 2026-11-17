using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class LevelClockTests
    {
        [UnityTest]
        public IEnumerator Restart_StartsTheRoundTimeAgainFromZero()
        {
            yield return new WaitForSeconds(0.3f);
            Assert.Greater(LevelClock.Seconds, 0.25f, "The round time counts up from the level loading.");

            LevelClock.Restart();
            Assert.Less(LevelClock.Seconds, 0.05f, "A checkpoint restart starts the round from zero.");

            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(0.2f, LevelClock.Seconds, 0.1f, "Then it counts up again.");
        }
    }
}
