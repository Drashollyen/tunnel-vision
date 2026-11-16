using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// The demo as a whole: each level has what it needs to be played and finished, the level
    /// select wheel leads to all three, and Nicholas sounds the same in every level.
    /// </summary>
    public class DemoSetupTests
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private static IEnumerable<int> Levels() => new[] { 1, 2, 3 };

        private static IEnumerator Load(string path)
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                path, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
        }

        private static T Get<T>(object target, string field) => (T)target.GetType().GetField(field, Private).GetValue(target);

        [UnityTest]
        public IEnumerator EveryLevel_HasAPlayer_AHud_TheRightGoal_AndTeddies([ValueSource(nameof(Levels))] int level)
        {
            yield return Load($"Assets/_Project/Scenes/Levels/Level{level}.unity");

            Assert.AreEqual(1, Object.FindObjectsByType<PlayerLives>(FindObjectsInactive.Include).Length, "One Nicholas.");
            Assert.AreEqual("Player", Object.FindAnyObjectByType<PlayerLives>().tag, "Rides find him by his tag.");
            Assert.IsNotNull(Object.FindAnyObjectByType<LevelCompleteScreen>(FindObjectsInactive.Include), "The HUD, with its Level Complete screen.");
            Assert.IsNotNull(GameObject.Find("Confiner"), "Camera bounds.");
            Assert.IsNotNull(Object.FindAnyObjectByType<CollectibleSpawner>(), "Teddies to find.");

            var goals = Object.FindObjectsByType<LevelGoal>(FindObjectsInactive.Include);
            Assert.AreEqual(1, goals.Length, "One way to finish.");
            Assert.AreEqual(level - 1, Get<int>(goals[0], "levelIndex"), "The goal unlocks the right cabin.");
            Assert.AreEqual(level == 3, Get<bool>(goals[0], "endsDemo"), "Only Level 3 ends the demo.");
            if (level < 3)
            {
                var next = Get<string>(goals[0], "nextLevelScene");
                Assert.AreEqual($"Level{level + 1}", next);
                Assert.IsTrue(Application.CanStreamedLevelBeLoaded(next), $"{next} should be in the build.");
            }
        }

        [UnityTest]
        public IEnumerator EveryLevel_UsesThePlayerPrefabsOwnSounds([ValueSource(nameof(Levels))] int level)
        {
            yield return Load($"Assets/_Project/Scenes/Levels/Level{level}.unity");
            var player = Object.FindAnyObjectByType<PlayerLives>();

            var jump = Get<AudioSource>(player.GetComponent<PlayerMovement>(), "jumpSound");
            var death = Get<AudioSource>(player, "deathSound");
            Assert.AreEqual("Nicholas-Jump", jump.resource.name);
            Assert.AreEqual(0.3f, jump.volume, 0.001f);
            Assert.AreEqual("Nicholas-Death", death.resource.name);
            Assert.AreEqual(0.4f, death.volume, 0.001f);
            Assert.AreEqual(jump.gameObject, player.gameObject, "Played from Nicholas himself.");
        }

        [UnityTest]
        public IEnumerator LevelSelect_LeadsToAllThreeLevels()
        {
            yield return Load("Assets/_Project/Scenes/Menus/LevelSelect.unity");
            var wheel = Object.FindAnyObjectByType<FerrisWheelSelect>();
            var scenes = Get<string[]>(wheel, "levelScenes");

            CollectionAssert.AreEqual(new[] { "Level1", "Level2", "Level3" }, scenes.Take(3).ToArray());
            Assert.IsTrue(scenes.Skip(3).All(string.IsNullOrEmpty), "Cabins 4 and 5 have no level in the demo.");
            Assert.AreEqual(3, Get<int>(wheel, "prototypeLevelCount"));
            foreach (var scene in scenes.Take(3))
            {
                Assert.IsTrue(Application.CanStreamedLevelBeLoaded(scene), $"{scene} should be in the build.");
            }
        }
    }
}
