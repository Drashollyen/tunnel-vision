using NUnit.Framework;
using TunnelVision.UI;

namespace TunnelVision.Tests
{
    public class ChaplinEasterEggTests
    {
        [Test]
        public void Black_FadesInSmoothly_AndStays()
        {
            Assert.AreEqual(0f, ChaplinEasterEgg.BlackAt(0f, 1.2f), 0.001f, "Clear to begin with.");
            Assert.AreEqual(0.5f, ChaplinEasterEgg.BlackAt(0.6f, 1.2f), 0.001f, "Halfway at halfway.");
            Assert.AreEqual(1f, ChaplinEasterEgg.BlackAt(1.2f, 1.2f), 0.001f, "Black at the end.");
            Assert.AreEqual(1f, ChaplinEasterEgg.BlackAt(9f, 1.2f), 0.001f, "And it stays black.");
            Assert.AreEqual(1f, ChaplinEasterEgg.BlackAt(0f, 0f), 0.001f, "No fade time, straight to black.");

            var last = 0f;
            for (var t = 0f; t <= 1.2f; t += 0.05f)
            {
                var alpha = ChaplinEasterEgg.BlackAt(t, 1.2f);
                Assert.GreaterOrEqual(alpha, last, "It only ever gets darker.");
                last = alpha;
            }
        }
    }
}
