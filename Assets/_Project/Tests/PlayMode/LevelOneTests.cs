using System.Collections;
using NUnit.Framework;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class LevelOneTests
    {
        private const string LevelPath = "Assets/_Project/Scenes/Levels/Level1_BigTop.unity";
        private const float StartX = -6f;
        private const float GroundTop = -2.5f;

        // The cotton candy checkpoint, and where it puts the player back.
        private const float CheckpointX = 29.5f;
        private const float CheckpointRespawnX = 23.93f;

        // Gaps between platforms, over the kill plane.
        private const float FirstPitX = 20.2f;
        private const float PitAfterCheckpointX = 32.75f;
        // Falling into the pit, then the death sequence (about 5 seconds).
        private const float DeathTimeout = 12f;

        private PlayerLives _lives;
        private PlayerRespawn _respawn;

        [UnitySetUp]
        public IEnumerator LoadLevel()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                LevelPath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return new WaitForSeconds(0.5f);
            _lives = Object.FindAnyObjectByType<PlayerLives>();
            _respawn = _lives.GetComponent<PlayerRespawn>();
        }

        private IEnumerator WaitForDeathAndRespawn()
        {
            var startLives = _lives.CurrentLives;
            var giveUpAt = Time.realtimeSinceStartup + DeathTimeout;

            while ((_lives.CurrentLives == startLives || _respawn.IsRespawning)
                && Time.realtimeSinceStartup < giveUpAt)
            {
                yield return null;
            }

            Assert.IsFalse(_respawn.IsRespawning, "The player never came back after dying.");
        }

        private void PutPlayerAt(float x, float y)
        {
            _lives.transform.position = new Vector3(x, y, 0f);
        }

        [UnityTest]
        public IEnumerator FallingInAPit_TakesALifeAndReturnsToStart()
        {
            PutPlayerAt(FirstPitX, 2f);
            yield return WaitForDeathAndRespawn();

            Assert.AreEqual(2, _lives.CurrentLives);
            Assert.AreEqual(StartX, _lives.transform.position.x, 0.2f);
            Assert.That(_lives.transform.position.y, Is.GreaterThan(GroundTop - 0.5f));
        }

        [UnityTest]
        public IEnumerator AfterCheckpoint_PitReturnsToCheckpoint()
        {
            PutPlayerAt(CheckpointX, 0f);
            yield return new WaitForSeconds(0.5f);

            PutPlayerAt(PitAfterCheckpointX, 2f);
            yield return WaitForDeathAndRespawn();

            Assert.AreEqual(2, _lives.CurrentLives);
            Assert.AreEqual(CheckpointRespawnX, _lives.transform.position.x, 0.2f);
            Assert.That(_lives.transform.position.y, Is.GreaterThan(GroundTop - 0.5f));
        }
    }
}
