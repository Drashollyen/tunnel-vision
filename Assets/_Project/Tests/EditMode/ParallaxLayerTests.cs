using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class ParallaxLayerTests
    {
        [Test]
        public void FactorZeroDoesNotMove()
        {
            Assert.AreEqual(Vector3.zero, ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 0f));
        }

        [Test]
        public void FactorOneFollowsCameraExactly()
        {
            Assert.AreEqual(new Vector3(4, 2, 0), ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 1f));
        }

        [Test]
        public void HalfFactorMovesHalfAsFar()
        {
            Assert.AreEqual(new Vector3(2, 1, 0), ParallaxLayer.OffsetFor(new Vector3(4, 2, 0), 0.5f));
        }

        [Test]
        public void IgnoresCameraZ()
        {
            Assert.AreEqual(0f, ParallaxLayer.OffsetFor(new Vector3(0, 0, -10), 1f).z);
        }
    }
}
