using NUnit.Framework;
using TunnelVision.Enemies;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class ClownThrowerTests
    {
        private const float Gravity = 9.81f;

        // Steps the ball's flight forward and returns where it is after the given time.
        private static Vector2 PositionAfter(Vector2 from, Vector2 velocity, float seconds)
        {
            return from + velocity * seconds + 0.5f * new Vector2(0f, -Gravity) * seconds * seconds;
        }

        [TestCase(6f, 0f)]
        [TestCase(-6f, 0f)]
        [TestCase(5f, 3f)]
        [TestCase(-4f, -2f)]
        public void LaunchVelocity_LandsOnTheTarget(float dx, float dy)
        {
            var from = new Vector2(1f, 2f);
            var to = from + new Vector2(dx, dy);
            const float speed = 8f;

            var velocity = ClownThrower.LaunchVelocity(from, to, speed, Gravity);
            var flight = Mathf.Abs(dx) / speed;

            var landed = PositionAfter(from, velocity, flight);
            Assert.That(landed.x, Is.EqualTo(to.x).Within(0.001f));
            Assert.That(landed.y, Is.EqualTo(to.y).Within(0.001f));
        }

        [Test]
        public void LaunchVelocity_TravelsSidewaysAtTheThrowSpeed()
        {
            var velocity = ClownThrower.LaunchVelocity(Vector2.zero, new Vector2(-10f, 0f), 8f, Gravity);

            Assert.That(velocity.x, Is.EqualTo(-8f).Within(0.001f));
            Assert.That(velocity.y, Is.GreaterThan(0f), "A level throw should arc upward first.");
        }

        [Test]
        public void LaunchVelocity_TargetStraightAboveStillReachesIt()
        {
            var to = new Vector2(0f, 2f);
            var velocity = ClownThrower.LaunchVelocity(Vector2.zero, to, 8f, Gravity);

            var landed = PositionAfter(Vector2.zero, velocity, 0.25f);
            Assert.That(landed.y, Is.EqualTo(to.y).Within(0.001f));
            Assert.That(velocity.x, Is.EqualTo(0f).Within(0.001f));
        }
    }
}
