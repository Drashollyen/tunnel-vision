using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Loads each real level and checks every teddy spawn point is safe and reachable: solid
    /// ground just below, inside the camera's bounds, not inside anything solid, clear of drop
    /// towers, lifts and wheels. A bad point fails here before it can kill or strand a player.
    /// </summary>
    public class CollectibleSpawnSafetyTests
    {
        private const int GroundLayers = 1 << 6;
        private const float MaxGroundBelow = 1.5f;   // Ground this close is walking or a small hop.
        private static readonly Vector2 TeddySize = new Vector2(0.74f, 0.87f);
        private const float LiftClearance = 4f;      // Lifts travel straight up and down.
        private const float WheelClearance = 9f;     // Wheel seats orbit 8 from the hub.

        private static IEnumerable<string> Levels()
        {
            yield return "Assets/_Project/Scenes/Levels/Level1.unity";
            yield return "Assets/_Project/Scenes/Levels/Level2.unity";
        }

        private static IEnumerator Load(string path)
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                path, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return new WaitForFixedUpdate();
            Physics2D.SyncTransforms();
        }

        private static List<Transform> SpawnPoints(out CollectibleSpawner spawner)
        {
            spawner = Object.FindAnyObjectByType<CollectibleSpawner>();
            Assert.IsNotNull(spawner, "The level should have a Collectibles spawner.");
            return spawner.SpawnPoints;
        }

        [UnityTest]
        public IEnumerator EveryLevel_HasMoreSpotsThanTeddies([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var points = SpawnPoints(out var spawner);

            Assert.Greater(points.Count, spawner.Count, "Spots to choose between, so runs differ.");
            Assert.AreEqual(spawner.Count, spawner.Spawned.Count, "It should place its teddies at the start.");
        }

        [UnityTest]
        public IEnumerator EverySpot_HasGroundJustBelow([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            foreach (var point in SpawnPoints(out _))
            {
                var hit = Physics2D.Raycast(point.position, Vector2.down, MaxGroundBelow, GroundLayers);
                Assert.IsTrue(hit.collider != null && !hit.collider.isTrigger,
                    $"{point.name} should have solid ground within {MaxGroundBelow} below it.");
            }
        }

        [UnityTest]
        public IEnumerator EverySpot_IsInsideTheLevel([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var bounds = GameObject.Find("Confiner").GetComponent<Collider2D>();
            foreach (var point in SpawnPoints(out _))
            {
                Assert.IsTrue(bounds.OverlapPoint(point.position), $"{point.name} should be inside the camera's bounds.");
            }
        }

        [UnityTest]
        public IEnumerator EverySpot_IsClearOfSolidsAndHazards([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            foreach (var point in SpawnPoints(out _))
            {
                foreach (var other in Physics2D.OverlapBoxAll(point.position, TeddySize, 0f))
                {
                    if (other.GetComponentInParent<Collectible>() != null)
                    {
                        continue; // The teddy placed there.
                    }

                    Assert.IsTrue(other.isTrigger, $"{point.name} overlaps the solid {other.name}.");
                    Assert.IsNull(other.GetComponentInParent<DropTower>(), $"{point.name} is under a drop tower.");
                    Assert.IsNull(other.GetComponentInParent<DropTowerCrush>(), $"{point.name} is in a crush zone.");
                }
            }
        }

        [UnityTest]
        public IEnumerator EverySpot_IsClearOfLiftsAndWheels([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var lifts = Object.FindObjectsByType<MovingPlatform>(FindObjectsInactive.Include);
            var hubs = Object.FindObjectsByType<BigWheelRotation>(FindObjectsInactive.Include)
                .Where(seat => seat.transRotationPoint != null)
                .Select(seat => (Vector2)seat.transRotationPoint.position)
                .Distinct()
                .ToList();

            foreach (var point in SpawnPoints(out _))
            {
                foreach (var lift in lifts)
                {
                    Assert.Greater(Mathf.Abs(point.position.x - lift.transform.position.x), LiftClearance,
                        $"{point.name} is in {lift.name}'s path.");
                }

                foreach (var hub in hubs)
                {
                    Assert.Greater(Mathf.Abs(point.position.x - hub.x), WheelClearance,
                        $"{point.name} is in a wheel's swing.");
                }
            }
        }
    }
}
