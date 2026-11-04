using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Gameplay;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    public class LivesDisplayTests
    {
        private static readonly Color Bright = Color.white;
        private static readonly Color Dim = new Color(0.3f, 0.3f, 0.3f, 0.4f);

        private readonly List<GameObject> _created = new List<GameObject>();

        [TearDown]
        public void DestroyCreatedObjects()
        {
            foreach (GameObject created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        [UnityTest]
        public IEnumerator Start_ShowsAllEyesBright()
        {
            CreatePlayer();
            Image[] eyes = CreateDisplay(3, out _);
            yield return null;

            AssertEyes(eyes, Bright, Bright, Bright);
        }

        [UnityTest]
        public IEnumerator LoseLife_DimsTheLastBrightEye()
        {
            PlayerLives player = CreatePlayer();
            Image[] eyes = CreateDisplay(3, out _);
            yield return null;

            player.LoseLife();

            AssertEyes(eyes, Bright, Bright, Dim);
        }

        [UnityTest]
        public IEnumerator Restart_BrightensAllEyes()
        {
            PlayerLives player = CreatePlayer();
            Image[] eyes = CreateDisplay(3, out _);
            yield return null;

            player.LoseLife();
            player.LoseLife();
            player.LoseLife();
            player.Restart();

            AssertEyes(eyes, Bright, Bright, Bright);
        }

        [UnityTest]
        public IEnumerator NoPlayerLives_LogsErrorAndTurnsOff()
        {
            LogAssert.Expect(LogType.Error, "LivesDisplay: no PlayerLives found in the scene. Add the Player prefab.");

            CreateDisplay(3, out LivesDisplay display);
            yield return null;

            Assert.That(display.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator EyeCountMismatch_LogsWarning_AndKeepsWorking()
        {
            LogAssert.Expect(LogType.Warning, "LivesDisplay: has 2 eyes but the player has 3 max lives.");

            PlayerLives player = CreatePlayer();
            Image[] eyes = CreateDisplay(2, out _);
            yield return null;

            player.LoseLife();
            player.LoseLife();

            AssertEyes(eyes, Bright, Dim);
        }

        [UnityTest]
        public IEnumerator ReEnabled_ShowsCurrentLives()
        {
            PlayerLives player = CreatePlayer();
            Image[] eyes = CreateDisplay(3, out LivesDisplay display);
            yield return null;

            display.enabled = false;
            player.LoseLife();
            display.enabled = true;

            AssertEyes(eyes, Bright, Bright, Dim);
        }

        [UnityTest]
        public IEnumerator DestroyedDisplay_StopsListening()
        {
            PlayerLives player = CreatePlayer();
            CreateDisplay(3, out LivesDisplay display);
            yield return null;

            Object.DestroyImmediate(display.gameObject);
            player.LoseLife();

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PlayerDestroyedFirst_CausesNoErrors()
        {
            PlayerLives player = CreatePlayer();
            CreateDisplay(3, out LivesDisplay display);
            yield return null;

            Object.DestroyImmediate(player.gameObject);
            Object.DestroyImmediate(display.gameObject);

            LogAssert.NoUnexpectedReceived();
        }

        private PlayerLives CreatePlayer()
        {
            var player = new GameObject("Test Player");
            _created.Add(player);
            return player.AddComponent<PlayerLives>();
        }

        // The eyes are assigned while the object is inactive, the same order a prefab gives:
        // serialized values are in place before Awake runs.
        private Image[] CreateDisplay(int eyeCount, out LivesDisplay display)
        {
            var root = new GameObject("Test Lives Display");
            _created.Add(root);
            root.SetActive(false);

            var eyes = new Image[eyeCount];
            for (int i = 0; i < eyeCount; i++)
            {
                var eye = new GameObject($"Eye {i + 1}");
                eye.transform.SetParent(root.transform);
                eyes[i] = eye.AddComponent<Image>();
            }

            display = root.AddComponent<LivesDisplay>();
            FieldInfo eyesField = typeof(LivesDisplay).GetField("eyes", BindingFlags.NonPublic | BindingFlags.Instance);
            eyesField.SetValue(display, eyes);

            root.SetActive(true);
            return eyes;
        }

        private static void AssertEyes(Image[] eyes, params Color[] expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(eyes[i].color, Is.EqualTo(expected[i]), $"Eye {i + 1}");
            }
        }
    }
}
