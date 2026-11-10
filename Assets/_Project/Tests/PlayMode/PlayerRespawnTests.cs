using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class PlayerRespawnTests
    {
        // The death sequence takes about 5 seconds.
        private const float RespawnTimeout = 10f;

        private GameObject _player;
        private GameObject _other;
        private PlayerLives _lives;
        private PlayerRespawn _respawn;

        [SetUp]
        public void CreatePlayer()
        {
            _player = new GameObject("Player");
            _player.transform.position = new Vector3(100f, 100f, 0f);
            var body = _player.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            _player.AddComponent<BoxCollider2D>();
            _lives = _player.AddComponent<PlayerLives>();
            _respawn = _player.AddComponent<PlayerRespawn>();
        }

        [TearDown]
        public void Clean()
        {
            Object.Destroy(_player);
            if (_other != null)
            {
                Object.Destroy(_other);
            }
        }

        private IEnumerator WaitForRespawn()
        {
            var giveUpAt = Time.realtimeSinceStartup + RespawnTimeout;
            while (_respawn.IsRespawning && Time.realtimeSinceStartup < giveUpAt)
            {
                yield return null;
            }

            Assert.IsFalse(_respawn.IsRespawning, "The player never came back after dying.");
        }

        private Checkpoint MakeCheckpoint(Vector3 position)
        {
            _other = new GameObject("Checkpoint");
            _other.transform.position = position;
            _other.AddComponent<BoxCollider2D>().isTrigger = true;
            return _other.AddComponent<Checkpoint>();
        }

        [UnityTest]
        public IEnumerator Kill_TakesOneLifeAndReturnsToStart()
        {
            _player.transform.position = new Vector3(120f, 100f, 0f);
            _respawn.Kill();
            yield return WaitForRespawn();

            Assert.AreEqual(2, _lives.CurrentLives);
            Assert.AreEqual(100f, _player.transform.position.x, 0.01f);
        }

        [UnityTest]
        public IEnumerator Kill_TwiceBeforeRespawn_TakesOneLife()
        {
            _respawn.Kill();
            _respawn.Kill();
            yield return WaitForRespawn();

            Assert.AreEqual(2, _lives.CurrentLives);
        }

        [UnityTest]
        public IEnumerator Checkpoint_BecomesRespawnPointAndDoesNotRestoreLives()
        {
            _respawn.Kill();
            yield return WaitForRespawn();

            var checkpoint = MakeCheckpoint(new Vector3(200f, 50f, 0f));
            _respawn.ReachCheckpoint(checkpoint);
            Assert.AreEqual(2, _lives.CurrentLives);

            _respawn.Kill();
            yield return WaitForRespawn();

            Assert.AreEqual(1, _lives.CurrentLives);
            Assert.AreEqual(checkpoint.RespawnPosition.x, _player.transform.position.x, 0.01f);
            Assert.AreEqual(checkpoint.RespawnPosition.y, _player.transform.position.y, 0.01f);
        }

        [UnityTest]
        public IEnumerator LastLife_RestartsLevelWithFullLivesAndNoCheckpoint()
        {
            _respawn.ReachCheckpoint(MakeCheckpoint(new Vector3(200f, 50f, 0f)));

            for (var i = 0; i < 2; i++)
            {
                _respawn.Kill();
                yield return WaitForRespawn();
            }

            _respawn.Kill();
            Assert.IsTrue(_lives.IsDead);
            yield return WaitForRespawn();

            Assert.AreEqual(3, _lives.CurrentLives);
            Assert.AreEqual(100f, _player.transform.position.x, 0.01f);
        }

        [UnityTest]
        public IEnumerator TouchingHazard_TakesALife()
        {
            _other = new GameObject("Hazard");
            _other.transform.position = _player.transform.position;
            _other.AddComponent<BoxCollider2D>().isTrigger = true;
            _other.AddComponent<Hazard>();

            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.AreEqual(2, _lives.CurrentLives);
        }
    }
}
