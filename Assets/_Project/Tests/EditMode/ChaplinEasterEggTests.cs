using NUnit.Framework;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class ChaplinEasterEggTests
    {
        [Test]
        public void Lights_StutterThenStayOff()
        {
            Assert.AreEqual(0f, ChaplinEasterEgg.LightsOutAlpha(0f, 1.2f), "Lights on to begin with.");
            Assert.AreEqual(1f, ChaplinEasterEgg.LightsOutAlpha(1.2f, 1.2f), "Black at the end.");
            Assert.AreEqual(1f, ChaplinEasterEgg.LightsOutAlpha(5f, 1.2f), "And it stays black.");

            var changes = 0;
            var last = 0f;
            for (var t = 0f; t < 1.2f; t += 0.01f)
            {
                var alpha = ChaplinEasterEgg.LightsOutAlpha(t, 1.2f);
                if (alpha != last)
                {
                    changes++;
                    last = alpha;
                }
            }

            Assert.GreaterOrEqual(changes, 3, "The screen should stutter before going black.");
        }

        [Test]
        public void Torch_StartsAtTheFirstSpot_AndEndsOnTheLast()
        {
            var points = new[] { new Vector2(500f, 300f), new Vector2(-200f, 100f), new Vector2(-600f, -250f) };
            Assert.AreEqual(points[0], ChaplinEasterEgg.SweepPoint(0f, points));
            Assert.AreEqual(points[1], ChaplinEasterEgg.SweepPoint(0.5f, points), "Stops on each spot on the way.");
            Assert.AreEqual(points[2], ChaplinEasterEgg.SweepPoint(1f, points), "Ends on Chaplin.");
            Assert.AreEqual(points[2], ChaplinEasterEgg.SweepPoint(3f, points), "And stays there.");
            Assert.AreEqual(Vector2.zero, ChaplinEasterEgg.SweepPoint(0.5f, new Vector2[0]), "No spots, no crash.");
        }

        [Test]
        public void Torch_EasesBetweenSpots()
        {
            var points = new[] { Vector2.zero, new Vector2(100f, 0f) };
            var early = ChaplinEasterEgg.SweepPoint(0.1f, points).x;
            var middle = ChaplinEasterEgg.SweepPoint(0.5f, points).x;
            Assert.Less(early, 10f, "Slow to start, like someone looking around.");
            Assert.AreEqual(50f, middle, 0.01f);
        }

        [Test]
        public void DrawnRect_FitsTheSpriteLikePreserveAspect()
        {
            // A tall sprite in a wider box: full height, centred across, sitting on the bottom.
            var drawn = ChaplinEasterEgg.DrawnRect(new Rect(-67f, 0f, 134f, 258f), new Vector2(0.5f, 0f), new Vector2(235f, 726f));
            Assert.AreEqual(258f, drawn.height, 0.01f);
            Assert.AreEqual(258f * 235f / 726f, drawn.width, 0.01f);
            Assert.AreEqual(0f, drawn.center.x, 0.01f);
            Assert.AreEqual(0f, drawn.y, 0.01f);
        }

        [Test]
        public void Glitch_IsTheRightLength_LoudButNotClipping_AndRepeatable()
        {
            var samples = ChaplinEasterEgg.GlitchSamples(44100, 1.2f, 7);
            Assert.AreEqual(Mathf.RoundToInt(44100 * 1.2f), samples.Length);

            var peak = 0f;
            foreach (var sample in samples)
            {
                peak = Mathf.Max(peak, Mathf.Abs(sample));
            }

            Assert.Greater(peak, 0.2f, "It should be heard.");
            Assert.LessOrEqual(peak, 1f, "It shouldn't clip.");
            CollectionAssert.AreEqual(samples, ChaplinEasterEgg.GlitchSamples(44100, 1.2f, 7), "Same seed, same sound.");
            Assert.AreEqual(0, ChaplinEasterEgg.GlitchSamples(44100, 0f, 7).Length);
        }
    }
}
