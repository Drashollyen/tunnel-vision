using System.Collections;
using NUnit.Framework;
using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class LevelOneTests
    {
        private const string LevelPath = "Assets/_Project/Scenes/Levels/Level1_BigTop.unity";
        private const float StartX = 4f;
        private const float CheckpointX = 74f;
        private const float DeathWait = 3f;

        private PlayerLives _lives;

        [UnitySetUp]
        public IEnumerator LoadLevel()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                LevelPath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return new WaitForSeconds(0.5f);
            _lives = Object.FindAnyObjectByType<PlayerLives>();
        }

        private void PutPlayerAt(float x, float y)
        {
            _lives.transform.position = new Vector3(x, y, 0f);
        }

        [UnityTest]
        public IEnumerator FallingInAPit_TakesALifeAndReturnsToStart()
        {
            PutPlayerAt(21f, 3f);
            yield return new WaitForSeconds(DeathWait);

            Assert.AreEqual(2, _lives.CurrentLives);
            Assert.AreEqual(StartX, _lives.transform.position.x, 0.2f);
            Assert.That(_lives.transform.position.y, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator AfterCheckpoint_PitReturnsToCheckpoint()
        {
            PutPlayerAt(CheckpointX, 9f);
            yield return new WaitForSeconds(0.5f);

            PutPlayerAt(81f, 9f);
            yield return new WaitForSeconds(DeathWait);

            Assert.AreEqual(2, _lives.CurrentLives);
            Assert.AreEqual(CheckpointX, _lives.transform.position.x, 0.2f);
            Assert.That(_lives.transform.position.y, Is.GreaterThan(7f));
        }
    }
}
