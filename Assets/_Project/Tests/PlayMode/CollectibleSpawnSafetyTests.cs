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
    /// Loads each real level and checks its teddies are where they should be: three fixed spots
    /// per level, inside the camera's bounds, not inside anything solid or a drop tower. Spots on
    /// the ground need solid ground just below. A few spots are off the ground on purpose, picked
    /// up by riding a wheel or drop tower or by a jump; those are listed below with how to reach
    /// them, and are left out of the ground and ride-clearance checks. A spot that moves fails here.
    /// </summary>
    public class CollectibleSpawnSafetyTests
    {
        private const int GroundLayers = 1 << 6;
        private const int TeddiesPerLevel = 3;
        private const float MaxGroundBelow = 1.5f;   // Ground this close is walking or a small hop.
        private static readonly Vector2 TeddySize = new Vector2(0.74f, 0.87f);
        private const float LiftClearance = 4f;      // Lifts travel straight up and down.
        private const float WheelClearance = 2f;     // How close a spot can be to the circle the seats ride.

        // Off-the-ground spots, checked by hand from the level layout, and how you get each one.
        private static readonly Dictionary<string, (Vector2 spot, string how)[]> StuntSpots =
            new Dictionary<string, (Vector2, string)[]>
            {
                ["Level1"] = new[]
                {
                    (new Vector2(332f, 14.3f), "Jump left off Platform 29, or fall down the right side of Platform 24."),
                    (new Vector2(383.5f, -1.2f), "Stand on Big Wheel 4's seat as it swings through the bottom left."),
                },
                ["Level2"] = new[]
                {
                    (new Vector2(186f, 22f), "Jump right off the top of Drop Tower 1."),
                    (new Vector2(85f, 14f), "Drop off the right edge of Platform 07, through Big Wheel 2's hub."),
                },
                ["Level3"] = new[]
                {
                    (new Vector2(168.5f, 18f), "Ride Drop Tower 2 to the top."),
                },
            };

        private static IEnumerable<string> Levels()
        {
            yield return "Assets/_Project/Scenes/Levels/Level1.unity";
            yield return "Assets/_Project/Scenes/Levels/Level2.unity";
            yield return "Assets/_Project/Scenes/Levels/Level3.unity";
        }

        private static IEnumerator Load(string path)
        {
            yield return TestScenes.Load(path);
            yield return new WaitForFixedUpdate();
            Physics2D.SyncTransforms();
        }

        private static List<Transform> SpawnPoints(out CollectibleSpawner spawner)
        {
            spawner = Object.FindAnyObjectByType<CollectibleSpawner>();
            Assert.IsNotNull(spawner, "The level should have a Collectibles spawner.");
            return spawner.SpawnPoints;
        }

        private static bool IsStuntSpot(Vector2 point)
        {
            return StuntSpots.TryGetValue(SceneManager.GetActiveScene().name, out var spots)
                   && spots.Any(s => Vector2.Distance(s.spot, point) < 0.01f);
        }

        [UnityTest]
        public IEnumerator EveryLevel_PlacesATeddyOnEachOfItsThreeSpots([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var points = SpawnPoints(out var spawner);

            Assert.AreEqual(TeddiesPerLevel, points.Count, "Three fixed spots per level.");
            Assert.AreEqual(points.Count, spawner.Spawned.Count, "A teddy on every spot at the start.");
        }

        [UnityTest]
        public IEnumerator EveryStuntSpot_IsStillThere([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var points = SpawnPoints(out _).Select(p => (Vector2)p.position).ToList();
            if (!StuntSpots.TryGetValue(SceneManager.GetActiveScene().name, out var spots))
            {
                yield break;
            }

            foreach (var (spot, how) in spots)
            {
                Assert.IsTrue(points.Any(p => Vector2.Distance(p, spot) < 0.01f),
                    $"A teddy was checked by hand at {spot} ({how}) but isn't there now. Check the new spot can be reached, then update this list.");
            }
        }

        [UnityTest]
        public IEnumerator EveryGroundSpot_HasGroundJustBelow([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            foreach (var point in SpawnPoints(out _))
            {
                if (IsStuntSpot(point.position))
                {
                    continue;
                }

                var hit = Physics2D.Raycast(point.position, Vector2.down, MaxGroundBelow, GroundLayers);
                Assert.IsTrue(hit.collider != null && !hit.collider.isTrigger,
                    $"{point.name} should have solid ground within {MaxGroundBelow} below it, or be a listed stunt spot.");
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
        public IEnumerator EveryGroundSpot_IsClearOfLiftsAndWheels([ValueSource(nameof(Levels))] string level)
        {
            yield return Load(level);
            var lifts = Object.FindObjectsByType<MovingPlatform>(FindObjectsInactive.Include);
            // Each wheel: its hub, and how far out its seats swing.
            var wheels = Object.FindObjectsByType<BigWheelRotation>(FindObjectsInactive.Include)
                .Where(seat => seat.transRotationPoint != null)
                .GroupBy(seat => seat.transRotationPoint)
                .Select(g => ((Vector2)g.Key.position,
                    g.Max(seat => Vector2.Distance(seat.transform.position, g.Key.position))))
                .ToList();

            foreach (var point in SpawnPoints(out _))
            {
                if (IsStuntSpot(point.position))
                {
                    continue; // Reached from the ride itself.
                }

                foreach (var lift in lifts)
                {
                    Assert.Greater(Mathf.Abs(point.position.x - lift.transform.position.x), LiftClearance,
                        $"{point.name} is in {lift.name}'s path.");
                }

                foreach (var (hub, radius) in wheels)
                {
                    var fromSeats = Mathf.Abs(Vector2.Distance(point.position, hub) - radius);
                    Assert.Greater(fromSeats, WheelClearance, $"{point.name} is in a wheel's swing.");
                }
            }
        }
    }
}
