using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class CollectibleSpawnerTests
    {
        private const string Prefab = "Assets/_Project/Prefabs/Level/TeddyBear.prefab";

        private CollectibleSpawner _spawner;
        private readonly List<Vector3> _points = new List<Vector3>();

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("CollectibleSpawnerTest");

            var holder = new GameObject("Collectibles");
            holder.SetActive(false); // So Start waits; the test spawns itself.
            _points.Clear();
            for (var i = 0; i < 8; i++)
            {
                var point = new GameObject("Spawn Point " + (i + 1)).transform;
                point.SetParent(holder.transform, false);
                point.position = new Vector3(i * 10f, 2f, 0f);
                _points.Add(point.position);
            }

            _spawner = holder.AddComponent<CollectibleSpawner>();
            typeof(CollectibleSpawner).GetField("teddyPrefab", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_spawner, UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(Prefab));
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [UnityTest]
        public IEnumerator Spawn_PutsATeddyOnEveryPoint()
        {
            _spawner.Spawn();
            yield return null;

            Assert.AreEqual(_points.Count, _spawner.Spawned.Count, "One teddy per spawn point, every run.");
            var used = new HashSet<Vector3>();
            foreach (var teddy in _spawner.Spawned)
            {
                Assert.IsNotNull(teddy, "Each one should be a teddy.");
                var spot = _points.Find(p => Mathf.Abs(p.x - teddy.transform.position.x) < 0.01f);
                Assert.AreNotEqual(default(Vector3), spot, "Each teddy sits on a spawn point.");
                Assert.AreEqual(spot.y, teddy.transform.position.y, 0.01f, "Exactly on its point.");
                Assert.IsTrue(used.Add(spot), "No two teddies on one point.");
            }
        }

        [UnityTest]
        public IEnumerator SpawnedTeddies_AreNotCountedAsSpawnPoints()
        {
            _spawner.Spawn();
            yield return null;

            Assert.AreEqual(8, _spawner.SpawnPoints.Count);
        }
    }
}
