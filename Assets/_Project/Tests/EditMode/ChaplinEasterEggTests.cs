using NUnit.Framework;
using TunnelVision.UI;

namespace TunnelVision.Tests
{
    public class ChaplinEasterEggTests
    {
        [Test]
        public void Countdown_TicksDownToZeroAndStops()
        {
            Assert.AreEqual(10, ChaplinEasterEgg.CountdownAt(0f, 10));
            Assert.AreEqual(10, ChaplinEasterEgg.CountdownAt(0.99f, 10));
            Assert.AreEqual(9, ChaplinEasterEgg.CountdownAt(1f, 10));
            Assert.AreEqual(1, ChaplinEasterEgg.CountdownAt(9.5f, 10));
            Assert.AreEqual(0, ChaplinEasterEgg.CountdownAt(10f, 10));
            Assert.AreEqual(0, ChaplinEasterEgg.CountdownAt(60f, 10), "Never below zero.");
            Assert.AreEqual(10, ChaplinEasterEgg.CountdownAt(-1f, 10), "No time yet, no tick.");
        }

        [Test]
        public void Message_ShowsTheCountdown()
        {
            StringAssert.Contains("In 7...", ChaplinEasterEgg.MessageFor(7));
            StringAssert.Contains("In 0...", ChaplinEasterEgg.MessageFor(0));
        }

        [Test]
        public void Lights_FlickerThenStayOff()
        {
            Assert.AreEqual(0f, ChaplinEasterEgg.LightsOutAlpha(0f, 0.8f), "Lights on to begin with.");
            Assert.AreEqual(1f, ChaplinEasterEgg.LightsOutAlpha(0.8f, 0.8f), "Off at the end.");
            Assert.AreEqual(1f, ChaplinEasterEgg.LightsOutAlpha(5f, 0.8f), "And they stay off.");

            var changes = 0;
            var last = 0f;
            for (var t = 0f; t < 0.8f; t += 0.01f)
            {
                var alpha = ChaplinEasterEgg.LightsOutAlpha(t, 0.8f);
                if (alpha != last)
                {
                    changes++;
                    last = alpha;
                }
            }

            Assert.GreaterOrEqual(changes, 3, "The lights should flicker before going out.");
        }

        [Test]
        public void Wording_IsTitleCase()
        {
            foreach (var line in new[] { ChaplinEasterEgg.HeaderText, ChaplinEasterEgg.MessageFor(10) })
            {
                foreach (var word in line.Split(' ', '\n'))
                {
                    if (word.Length > 0 && char.IsLetter(word[0]))
                    {
                        Assert.That(char.IsUpper(word[0]), Is.True, $"\"{word}\" should start with a capital.");
                    }
                }
            }
        }
    }
}
