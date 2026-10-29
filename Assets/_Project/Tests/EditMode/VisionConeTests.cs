using NUnit.Framework;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class VisionConeTests
    {
        private static readonly Vector2 Apex = new Vector2(500f, 100f);

        [Test]
        public void AngleTowards_StraightUp_IsZero()
        {
            Assert.That(VisionConeBackground.AngleTowards(Apex, Apex + Vector2.up * 300f, 45f), Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void AngleTowards_UpRight_IsPositive()
        {
            Assert.That(VisionConeBackground.AngleTowards(Apex, Apex + new Vector2(100f, 100f), 60f), Is.EqualTo(45f).Within(0.01f));
        }

        [Test]
        public void AngleTowards_IsClampedToMaxAngle()
        {
            Assert.That(VisionConeBackground.AngleTowards(Apex, Apex + Vector2.left * 300f, 45f), Is.EqualTo(-45f).Within(0.01f));
            Assert.That(VisionConeBackground.AngleTowards(Apex, Apex + new Vector2(10f, -300f), 45f), Is.EqualTo(45f).Within(0.01f));
        }

        [Test]
        public void AngleTowards_PointerOnApex_IsZero()
        {
            Assert.That(VisionConeBackground.AngleTowards(Apex, Apex, 45f), Is.EqualTo(0f));
        }
    }
}
