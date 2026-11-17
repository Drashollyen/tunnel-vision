using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class LevelClockTests
    {
        [TearDown]
        public void Clean()
        {
            LevelClock.Release();
        }

        [UnityTest]
        public IEnumerator Hold_StopsTheRoundTime_AndReleaseCarriesOn()
        {
            yield return new WaitForSeconds(0.2f);
            var before = LevelClock.Seconds;

            LevelClock.Hold();
            Assert.IsTrue(LevelClock.IsHeld);
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(before, LevelClock.Seconds, 0.05f, "Held, the round time shouldn't move.");

            LevelClock.Release();
            Assert.IsFalse(LevelClock.IsHeld);
            yield return new WaitForSeconds(0.2f);
            Assert.AreEqual(before + 0.2f, LevelClock.Seconds, 0.1f, "Released, it carries on from where it stopped.");
        }
    }
}
