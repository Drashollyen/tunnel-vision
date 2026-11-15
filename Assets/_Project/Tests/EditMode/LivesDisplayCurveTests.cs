using NUnit.Framework;
using TunnelVision.UI;

namespace TunnelVision.Tests
{
    public class LivesDisplayCurveTests
    {
        [Test]
        public void LosingEye_StartsAtFullSize()
        {
            Assert.AreEqual(1f, LivesDisplay.LoseScale(0f), 0.001f);
            Assert.AreEqual(1f, LivesDisplay.LoseAlpha(0f), 0.001f);
        }

        [Test]
        public void LosingEye_SwellsBeforePopping()
        {
            Assert.Greater(LivesDisplay.LoseScale(LivesDisplay.SwellSeconds), 1.2f);
            Assert.AreEqual(1f, LivesDisplay.LoseAlpha(LivesDisplay.SwellSeconds), 0.001f,
                "It should stay solid while it swells.");
        }

        [Test]
        public void LosingEye_EndsGone()
        {
            Assert.AreEqual(0f, LivesDisplay.LoseScale(LivesDisplay.LoseSeconds), 0.001f);
            Assert.AreEqual(0f, LivesDisplay.LoseAlpha(LivesDisplay.LoseSeconds), 0.001f);
            Assert.AreEqual(0f, LivesDisplay.LoseScale(10f), 0.001f);
        }

        [Test]
        public void ReturningEye_OvershootsThenSettles()
        {
            Assert.AreEqual(0f, LivesDisplay.AppearScale(0f), 0.001f);
            Assert.Greater(LivesDisplay.AppearScale(LivesDisplay.AppearSeconds * 0.6f), 1.05f);
            Assert.AreEqual(1f, LivesDisplay.AppearScale(LivesDisplay.AppearSeconds), 0.001f);
        }
    }
}
