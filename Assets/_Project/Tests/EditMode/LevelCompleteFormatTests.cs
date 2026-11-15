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

        [Test]
        public void Fade_GoesBlackFirst_ThenShowsThePanel()
        {
            Assert.AreEqual((0f, 0f), LevelCompleteScreen.FadeAt(0f));

            var (halfBlack, panelEarly) = LevelCompleteScreen.FadeAt(LevelCompleteScreen.BlackFadeSeconds * 0.5f);
            Assert.AreEqual(0.5f, halfBlack, 0.001f);
            Assert.AreEqual(0f, panelEarly, 0.001f, "The panel waits until the screen is black.");

            var (black, panel) = LevelCompleteScreen.FadeAt(LevelCompleteScreen.BlackFadeSeconds + LevelCompleteScreen.FadeInSeconds);
            Assert.AreEqual(1f, black, 0.001f);
            Assert.AreEqual(1f, panel, 0.001f);
        }
    }
}
