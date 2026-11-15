using System.Collections;
using NUnit.Framework;
using TunnelVision.Level;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>The teddy bear pickup, using the real TeddyBear prefab.</summary>
    public class CollectibleTests
    {
        private const string Prefab = "Assets/_Project/Prefabs/Level/TeddyBear.prefab";

        private Collectible _teddy;
        private PlayerCollectibles _collectibles;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("CollectibleTest");

            var player = new GameObject("Player");
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerLives>();
            _collectibles = player.AddComponent<PlayerCollectibles>();
            player.transform.position = new Vector3(50f, 0f, 0f);

            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            Assert.IsNotNull(prefab, "The teddy prefab should exist.");
            _teddy = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity).GetComponent<Collectible>();
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [Test]
        public void Bob_StaysWithinItsHeight()
        {
            for (var t = 0f; t < 5f; t += 0.1f)
            {
                Assert.LessOrEqual(Mathf.Abs(Collectible.BobOffset(t, 0.15f, 2f)), 0.15f + 0.0001f);
            }
        }

        [UnityTest]
        public IEnumerator Teddy_Bobs()
        {
            var start = _teddy.transform.position.y;
            var moved = false;
            for (var i = 0; i < 60 && !moved; i++)
            {
                yield return null;
                moved = Mathf.Abs(_teddy.transform.position.y - start) > 0.01f;
            }

            Assert.IsTrue(moved, "It should bob so it stands out.");
        }

        [UnityTest]
        public IEnumerator Collecting_AddsOne_AndOnlyOnce()
        {
            _teddy.Collect(_collectibles);
            _teddy.Collect(_collectibles);
            yield return null;

            Assert.AreEqual(1, _collectibles.Count);
            Assert.IsTrue(_teddy.IsCollected);
            Assert.IsFalse(_teddy.GetComponent<Collider2D>().enabled, "It can't be touched again.");
        }

        [UnityTest]
        public IEnumerator Collecting_PopsThenDisappears()
        {
            _teddy.Collect(_collectibles);
            yield return new WaitForSeconds(Collectible.PopSeconds * 0.5f);
            Assert.Greater(_teddy.transform.localScale.x, 1f, "It should grow as it pops.");

            yield return new WaitForSecondsRealtime(1.5f); // The pop and the chime.
            Assert.IsFalse(_teddy.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator TouchingIt_CollectsIt()
        {
            _collectibles.transform.position = _teddy.transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(_teddy.IsCollected);
            Assert.AreEqual(1, _collectibles.Count);
        }

        [UnityTest]
        public IEnumerator TouchingIt_WithoutACounterYet_StillCounts()
        {
            Object.DestroyImmediate(_collectibles);
            var player = Object.FindAnyObjectByType<PlayerLives>();
            player.transform.position = _teddy.transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            Assert.IsTrue(_teddy.IsCollected);
            Assert.AreEqual(1, player.GetComponent<PlayerCollectibles>().Count);
        }
    }
}
