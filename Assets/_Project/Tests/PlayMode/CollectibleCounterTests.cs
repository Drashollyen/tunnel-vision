using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// The game has no pickups yet, so these build a bare player and a counter in a scene of
    /// their own and call Add() directly, the way a pickup will.
    /// </summary>
    public class CollectibleCounterTests
    {

        [UnitySetUp]
        public IEnumerator MakeScene()
        {
            yield return IsolatedScene.Create("CollectibleCounterTest");
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        private static GameObject MakePlayer(bool withCollectibles)
        {
            var player = new GameObject("Player");
            player.AddComponent<PlayerLives>();
            if (withCollectibles)
            {
                player.AddComponent<PlayerCollectibles>();
            }

            return player;
        }

        private static TMP_Text MakeCounter()
        {
            var hud = new GameObject("HUD", typeof(Canvas));
            var holder = new GameObject("CollectibleCounter", typeof(RectTransform));
            holder.transform.SetParent(hud.transform, false);
            holder.SetActive(false); // So Awake waits until the text is set.

            var text = new GameObject("Count", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(holder.transform, false);
            text.text = "not set";

            var counter = holder.AddComponent<CollectibleCounter>();
            typeof(CollectibleCounter)
                .GetField("countText", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(counter, text);
            holder.SetActive(true);
            return text;
        }

        [UnityTest]
        public IEnumerator ShowsZeroAtTheStart()
        {
            MakePlayer(withCollectibles: true);
            var text = MakeCounter();
            yield return null;

            Assert.AreEqual("0", text.text);
        }

        [UnityTest]
        public IEnumerator GoesUpByOneEachTimeAddIsCalled()
        {
            var collectibles = MakePlayer(withCollectibles: true).GetComponent<PlayerCollectibles>();
            var text = MakeCounter();
            yield return null;

            for (var expected = 1; expected <= 3; expected++)
            {
                collectibles.Add();
                yield return null;
                Assert.AreEqual(expected.ToString(), text.text);
            }
        }

        [UnityTest]
        public IEnumerator CatchesUp_WhenTheCountChangedWhileHidden()
        {
            var collectibles = MakePlayer(withCollectibles: true).GetComponent<PlayerCollectibles>();
            var text = MakeCounter();
            yield return null;

            text.transform.parent.gameObject.SetActive(false);
            collectibles.Add(2);
            text.transform.parent.gameObject.SetActive(true);
            yield return null;

            Assert.AreEqual("2", text.text);
        }

        [UnityTest]
        public IEnumerator PlayerWithoutCollectibles_GetsThemAdded()
        {
            // The Player prefab doesn't have PlayerCollectibles yet.
            var player = MakePlayer(withCollectibles: false);
            var text = MakeCounter();
            yield return null;

            var collectibles = player.GetComponent<PlayerCollectibles>();
            Assert.That(collectibles, Is.Not.Null);

            collectibles.Add();
            yield return null;
            Assert.AreEqual("1", text.text);
        }

        [UnityTest]
        public IEnumerator NoPlayer_LogsAClearErrorAndDoesNotCrash()
        {
            LogAssert.Expect(LogType.Error, "CollectibleCounter: no Player found in the scene. Add the Player prefab.");
            var text = MakeCounter();
            yield return null;

            Assert.AreEqual("not set", text.text);
            Assert.IsFalse(text.GetComponentInParent<CollectibleCounter>().enabled);
        }
    }
}
