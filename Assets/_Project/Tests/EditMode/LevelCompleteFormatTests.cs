using NUnit.Framework;
using TunnelVision.UI;

namespace TunnelVision.Tests
{
    public class LevelCompleteFormatTests
    {
        [TestCase(0f, "0:00")]
        [TestCase(5.9f, "0:05")]
        [TestCase(65f, "1:05")]
        [TestCase(600f, "10:00")]
        [TestCase(-3f, "0:00")]
        public void FormatTime_ShowsMinutesAndSeconds(float seconds, string expected)
        {
            Assert.AreEqual(expected, LevelCompleteScreen.FormatTime(seconds));
        }

        [Test]
        public void Stats_ShowTimeThenCollectibles()
        {
            Assert.AreEqual("Time: 1:05\nCollectibles: 3", LevelCompleteScreen.StatsText(65f, 3));
        }

        [Test]
        public void Title_IsTitleCase()
        {
            Assert.AreEqual("Level Complete", LevelCompleteScreen.Title);
        }
    }
}
