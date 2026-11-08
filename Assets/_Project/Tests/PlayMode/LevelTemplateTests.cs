using System.Collections;
using NUnit.Framework;
using TunnelVision.Gameplay;
using TunnelVision.Player;
using TunnelVision.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class LevelTemplateTests
    {
        private const string TemplatePath = "Assets/_Project/Scenes/Levels/LevelTemplate.unity";
        private const float SpawnHeight = 1.42f;

        [UnitySetUp]
        public IEnumerator LoadTemplate()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                TemplatePath, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return new WaitForSeconds(1.5f);
        }

        [Test]
        public void HasExactlyOnePlayerWithFullLives()
        {
            var players = Object.FindObjectsByType<PlayerLives>();
            Assert.AreEqual(1, players.Length);
            Assert.AreEqual(players[0].MaxLives, players[0].CurrentLives);
        }

        [Test]
        public void HudIsActive()
        {
            Assert.That(Object.FindAnyObjectByType<LivesDisplay>(), Is.Not.Null);
        }

        [Test]
        public void PlayerLandsOnGround()
        {
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            Assert.That(Mathf.Abs(player.rb.linearVelocity.y), Is.LessThan(0.01f));
            Assert.That(player.transform.position.y, Is.GreaterThan(0f));
        }

        [Test]
        public void GroundHasCollision()
        {
            var ground = GameObject.Find("Grid/Ground").GetComponent<CompositeCollider2D>();
            Assert.That(ground, Is.Not.Null);
            Assert.That(ground.pathCount, Is.GreaterThan(0));
        }

        [Test]
        public void PlayerStaysAtSpawnHeight()
        {
            var player = Object.FindAnyObjectByType<PlayerMovement>();
            Assert.That(player.transform.position.y, Is.EqualTo(SpawnHeight).Within(0.1f));
        }

        [Test]
        public void GlobalLightIsOffSoOnlyTheConeLights()
        {
            var globalLights = System.Array.FindAll(
                Object.FindObjectsByType<Light2D>(),
                light => light.lightType == Light2D.LightType.Global);

            Assert.AreEqual(1, globalLights.Length);
            Assert.AreEqual(0f, globalLights[0].intensity);
        }

        [Test]
        public void CameraIsConfinedToBounds()
        {
            var confiner = Object.FindAnyObjectByType<CinemachineConfiner2D>();
            Assert.That(confiner.BoundingShape2D, Is.Not.Null);
            Assert.AreEqual("CameraBounds", confiner.BoundingShape2D.name);
        }
    }
}
