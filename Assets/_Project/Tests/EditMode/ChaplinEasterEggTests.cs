using NUnit.Framework;
using TunnelVision.Audio;

namespace TunnelVision.Tests
{
    public class ChaplinEasterEggTests
    {
        [Test]
        public void WindDownPitch_DragsFromNormalToTheEndPitch_AndStays()
        {
            Assert.AreEqual(1f, MusicPlayer.WindDownPitchAt(0f, 1.5f, 0.15f), 0.001f, "Normal speed to begin with.");
            Assert.AreEqual(0.15f, MusicPlayer.WindDownPitchAt(1.5f, 1.5f, 0.15f), 0.001f, "At the end pitch by the end.");
            Assert.AreEqual(0.15f, MusicPlayer.WindDownPitchAt(9f, 1.5f, 0.15f), 0.001f, "And it stays there.");
            Assert.AreEqual(0.15f, MusicPlayer.WindDownPitchAt(0f, 0f, 0.15f), 0.001f, "No wind-down time, straight there.");
            Assert.Greater(MusicPlayer.WindDownPitchAt(0.75f, 1.5f, 0.15f), 0.575f, "Slow at first, faster towards the end.");

            var last = 1f;
            for (var t = 0f; t <= 1.5f; t += 0.05f)
            {
                var pitch = MusicPlayer.WindDownPitchAt(t, 1.5f, 0.15f);
                Assert.LessOrEqual(pitch, last, "It only ever gets lower.");
                last = pitch;
            }
        }
    }
}
