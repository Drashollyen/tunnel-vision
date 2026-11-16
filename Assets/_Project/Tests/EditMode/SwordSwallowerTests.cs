using NUnit.Framework;
using TunnelVision.Enemies;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class SwordSwallowerTests
    {
        [Test]
        public void Sword_FliesLevel_TheWayHeFaces()
        {
            Assert.AreEqual(new Vector2(10f, 0f), SwordSwallower.LaunchVelocity(true, 10f));
            Assert.AreEqual(new Vector2(-10f, 0f), SwordSwallower.LaunchVelocity(false, 10f));
        }

        [Test]
        public void Sword_SpeedSign_DoesNotFlipIt()
        {
            // A negative speed typed in the Inspector still throws the way he faces.
            Assert.AreEqual(new Vector2(10f, 0f), SwordSwallower.LaunchVelocity(true, -10f));
        }

        [Test]
        public void Sword_PointsTheWayItFlies()
        {
            Assert.AreEqual(0f, SwordProjectile.AngleFor(Vector2.right), 0.001f);
            Assert.AreEqual(180f, Mathf.Abs(SwordProjectile.AngleFor(Vector2.left)), 0.001f);
            Assert.AreEqual(90f, SwordProjectile.AngleFor(Vector2.up), 0.001f);
            Assert.AreEqual(-45f, SwordProjectile.AngleFor(new Vector2(1f, -1f)), 0.001f);
        }
    }
}
