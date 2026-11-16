using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;
using static TunnelVision.Tests.Private;

namespace TunnelVision.Tests
{
    /// <summary>
    /// After any respawn, at the level start or a checkpoint, the player faces the way the
    /// level starts (right), whichever way they were facing when they died.
    /// </summary>
    public class RespawnFacingTests
    {
        private const float Step = 0.15f; // Each part of the shortened death sequence.
        private const float Timeout = 5f;

        private PlayerRespawn _respawn;
        private PlayerMovement _movement;
        private SpriteRenderer _body;
        private Transform _eye;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("RespawnFacingTest");

            var player = new GameObject("Player");
            player.transform.position = new Vector3(100f, 100f, 0f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerLives>();

            player.SetActive(false); // So Awake waits until the parts are filled in.
            _movement = TestPlayer.AddMovement(player, rb, out _body, out _eye);
            _respawn = player.AddComponent<PlayerRespawn>();
            TestPlayer.ShortenDeath(_respawn, Step);

            player.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        private void FaceLeftAndLookUpLeft()
        {
            _body.flipX = true;
            Set(_movement, "facingRight", false);
            _eye.localRotation = Quaternion.Euler(0f, 0f, 135f);
            Set(_movement, "targetAngle", 135f);
        }

        private IEnumerator DieAndWaitForRespawn()
        {
            _respawn.Kill();
            yield return null;
            var giveUpAt = Time.realtimeSinceStartup + Timeout;
            while (_respawn.IsRespawning && Time.realtimeSinceStartup < giveUpAt)
            {
                yield return null;
            }

            Assert.IsFalse(_respawn.IsRespawning, "The player never came back.");
            yield return new WaitForFixedUpdate();
        }

        private void AssertFacingTheStartDirection()
        {
            Assert.IsTrue(_movement.FacingRight, "The body should face right.");
            Assert.IsFalse(_body.flipX, "The body sprite shouldn't be flipped.");
            Assert.Less(Quaternion.Angle(Quaternion.identity, _eye.localRotation), 1f,
                "The eye should look the way it did at the start.");
        }

        [UnityTest]
        public IEnumerator Respawn_AtTheLevelStart_FacesRight()
        {
            FaceLeftAndLookUpLeft();
            yield return DieAndWaitForRespawn();

            AssertFacingTheStartDirection();
        }

        [UnityTest]
        public IEnumerator Respawn_AtACheckpoint_FacesRight()
        {
            var checkpoint = new GameObject("Checkpoint").AddComponent<Checkpoint>();
            checkpoint.transform.position = new Vector3(200f, 100f, 0f);
            _respawn.ReachCheckpoint(checkpoint);

            FaceLeftAndLookUpLeft();
            yield return DieAndWaitForRespawn();

            AssertFacingTheStartDirection();
            Assert.AreEqual(checkpoint.RespawnPosition.x, _respawn.transform.position.x, 0.01f,
                "It should be the checkpoint respawn.");
        }

        [Test]
        public void FaceStartDirection_TurnsTheBodyAndEyeBack()
        {
            FaceLeftAndLookUpLeft();
            _movement.FaceStartDirection();

            AssertFacingTheStartDirection();
        }
    }
}
