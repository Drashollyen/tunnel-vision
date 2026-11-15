using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// At the end of a level the player walks on by themselves. They must stop at the platform's
    /// edge rather than walk off it.
    /// </summary>
    public class AutoWalkTests
    {
        private const int GroundLayer = 6;
        private const float PlatformHalfWidth = 5f;

        private GameObject _player;
        private PlayerMovement _movement;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("AutoWalkTest");
            Time.timeScale = 1f;

            var ground = new GameObject("Platform") { layer = GroundLayer };
            ground.AddComponent<BoxCollider2D>().size = new Vector2(PlatformHalfWidth * 2f, 1f);

            _player = new GameObject("Player");
            _player.transform.position = new Vector3(0f, 1.01f, 0f);
            var rb = _player.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            _player.AddComponent<BoxCollider2D>().size = Vector2.one;
            var feet = new GameObject("GroundCheck").transform;
            feet.SetParent(_player.transform, false);
            feet.localPosition = new Vector3(0f, -0.5f, 0f);

            // The real player always has a body sprite and an eye, and movement uses both.
            var body = new GameObject("BodySprite").AddComponent<SpriteRenderer>();
            body.transform.SetParent(_player.transform, false);
            var eye = new GameObject("Eyeball").transform;
            eye.SetParent(_player.transform, false);

            _player.SetActive(false);
            _movement = _player.AddComponent<PlayerMovement>();
            _movement.rb = rb;
            _movement.groundCheckPos = feet;
            _movement.groundCheckSize = new Vector2(0.5f, 0.1f);
            _movement.groundLayer = 1 << GroundLayer;
            typeof(PlayerMovement).GetField("bodySprite", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_movement, body);
            typeof(PlayerMovement).GetField("eyeball", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_movement, eye);
            _player.SetActive(true);
            yield return new WaitForFixedUpdate();
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [UnityTest]
        public IEnumerator AutoWalk_MovesThePlayerOn()
        {
            _movement.EnableControls(false);
            _movement.AutoWalk(1f);
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(_movement.IsAutoWalking);
            Assert.Greater(_player.transform.position.x, 0.5f, "They should walk on even with controls off.");
            Assert.IsTrue(_movement.FacingRight);
        }

        [UnityTest]
        public IEnumerator AutoWalk_StopsAtTheEdgeInsteadOfFallingOff()
        {
            _movement.EnableControls(false);
            _movement.AutoWalk(1f);
            yield return new WaitForSeconds(3f); // Long enough to reach the edge several times over.

            Assert.Less(_player.transform.position.x, PlatformHalfWidth, "They should stop before the edge.");
            Assert.Greater(_player.transform.position.x, PlatformHalfWidth - 1.5f, "They should get close to it.");
            Assert.Greater(_player.transform.position.y, 0.9f, "They shouldn't have fallen.");
        }

        [UnityTest]
        public IEnumerator AutoWalk_ZeroStops()
        {
            _movement.AutoWalk(1f);
            yield return new WaitForSeconds(0.2f);
            _movement.AutoWalk(0f);
            yield return new WaitForFixedUpdate();
            var stoppedAt = _player.transform.position.x;
            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(_movement.IsAutoWalking);
            Assert.AreEqual(stoppedAt, _player.transform.position.x, 0.05f);
        }
    }
}
