using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TunnelVision.Level;

namespace TunnelVision.Tests
{
    public class CollectibleSpawnerPickTests
    {
        [Test]
        public void Pick_GivesTheRightNumberOfDifferentSpots()
        {
            var picked = CollectibleSpawner.Pick(8, 3, new System.Random(1));

            Assert.AreEqual(3, picked.Count);
            Assert.AreEqual(3, picked.Distinct().Count(), "No spot twice.");
            Assert.That(picked, Is.All.InRange(0, 7));
        }

        [Test]
        public void Pick_AskingForMoreThanThereAre_GivesThemAll()
        {
            var picked = CollectibleSpawner.Pick(2, 3, new System.Random(1));

            CollectionAssert.AreEquivalent(new[] { 0, 1 }, picked);
        }

        [Test]
        public void Pick_NoSpots_GivesNone()
        {
            Assert.IsEmpty(CollectibleSpawner.Pick(0, 3, new System.Random(1)));
        }

        [Test]
        public void Pick_OverManyRuns_UsesEverySpot()
        {
            var seen = new HashSet<int>();
            for (var seed = 0; seed < 200; seed++)
            {
                seen.UnionWith(CollectibleSpawner.Pick(8, 3, new System.Random(seed)));
            }

            Assert.AreEqual(8, seen.Count, "Every spot should come up sometimes.");
        }

        [Test]
        public void Pick_DiffersBetweenRuns()
        {
            var layouts = new HashSet<string>();
            for (var seed = 0; seed < 20; seed++)
            {
                var picked = CollectibleSpawner.Pick(8, 3, new System.Random(seed));
                picked.Sort();
                layouts.Add(string.Join(",", picked));
            }

            Assert.Greater(layouts.Count, 5, "Runs should get different layouts.");
        }
    }
}
