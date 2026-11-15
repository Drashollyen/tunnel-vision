using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class DemoCompleteTests
    {
        [Test]
        public void Flipbook_LoopsThroughTheFrames()
        {
            Assert.AreEqual(0, SpriteFlipbook.FrameAt(0f, 9, 10f));
            Assert.AreEqual(5, SpriteFlipbook.FrameAt(0.55f, 9, 10f));
            Assert.AreEqual(0, SpriteFlipbook.FrameAt(0.9f, 9, 10f), "Back to the start after the last frame.");
            Assert.AreEqual(0, SpriteFlipbook.FrameAt(1f, 0, 10f), "No frames, no crash.");
        }

        [Test]
        public void Hop_NeverGoesBelowWhereTheyStand()
        {
            for (var t = 0f; t < 3f; t += 0.05f)
            {
                var y = CelebrationLoop.At(CelebrationLoop.Move.Hop, t, 30f, 1.5f, 0.2f);
                Assert.GreaterOrEqual(y, 0f);
                Assert.LessOrEqual(y, 30f + 0.001f);
            }
        }

        [Test]
        public void Sway_GoesBothWays()
        {
            var most = 0f;
            var least = 0f;
            for (var t = 0f; t < 2f; t += 0.02f)
            {
                var angle = CelebrationLoop.At(CelebrationLoop.Move.Sway, t, 10f, 1f, 0f);
                most = Mathf.Max(most, angle);
                least = Mathf.Min(least, angle);
            }

            Assert.Greater(most, 9f);
            Assert.Less(least, -9f);
        }

        [Test]
        public void Eye_AimsAtItsTarget()
        {
            Assert.AreEqual(0f, EyeGlance.AngleTo(Vector2.zero, Vector2.right), 0.01f);
            Assert.AreEqual(180f, Mathf.Abs(EyeGlance.AngleTo(Vector2.zero, Vector2.left)), 0.01f);
            Assert.AreEqual(90f, EyeGlance.AngleTo(Vector2.zero, Vector2.up), 0.01f);
        }

        [Test]
        public void Title_PopsInPastFullSizeThenSettles()
        {
            Assert.AreEqual(0f, DemoCompleteScreen.TitleScaleAt(0f), 0.001f, "Hidden at first.");
            Assert.Greater(DemoCompleteScreen.TitleScaleAt(DemoCompleteScreen.TitleDelay + DemoCompleteScreen.TitlePopSeconds * 0.7f), 1.1f);
            Assert.AreEqual(1f, DemoCompleteScreen.TitleScaleAt(5f), 0.001f);
        }

        [Test]
        public void Stats_ListEachLevelThenTheTotal()
        {
            var text = DemoCompleteScreen.StatsText(new[]
            {
                new RunStats.LevelResult(0, 65f, 3, 0),
                new RunStats.LevelResult(1, 90f, 1, 1),
            });

            StringAssert.Contains("Level 1:  1:05  ·  3 Teddies  ·  0 Lives Lost", text);
            StringAssert.Contains("Level 2:  1:30  ·  1 Teddy  ·  1 Life Lost", text);
            StringAssert.Contains("Total:  2:35  ·  4 Teddies  ·  1 Life Lost", text);
        }

        [Test]
        public void Stats_WithNothingCleared_SaySo()
        {
            Assert.AreEqual(DemoCompleteScreen.NoStats, DemoCompleteScreen.StatsText(new RunStats.LevelResult[0]));
        }

        [Test]
        public void Wording_IsTitleCase()
        {
            foreach (var line in new[] { DemoCompleteScreen.Title, DemoCompleteScreen.Subtitle, DemoCompleteScreen.NoStats })
            {
                foreach (var word in line.Split(' '))
                {
                    Assert.That(char.IsUpper(word[0]), Is.True, $"\"{word}\" in \"{line}\" should start with a capital.");
                }
            }
        }
    }
}
