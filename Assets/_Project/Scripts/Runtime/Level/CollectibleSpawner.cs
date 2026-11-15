using System.Collections.Generic;
using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Places the level's teddy bears. Its children are hand-placed spawn points on safe ground;
    /// each time the level starts it picks <see cref="count"/> of them at random and puts a teddy
    /// on each, so every run is a little different. Add or move points by moving the children.
    /// </summary>
    public class CollectibleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject teddyPrefab;
        [Tooltip("How many teddies each run places, picked from the spawn points.")]
        [SerializeField] private int count = 3;

        private readonly List<Collectible> _spawned = new List<Collectible>();

        /// <summary>The teddies placed this run.</summary>
        public IReadOnlyList<Collectible> Spawned => _spawned;

        public int Count => count;

        /// <summary>Every spawn point: the spawner's direct children.</summary>
        public List<Transform> SpawnPoints
        {
            get
            {
                var points = new List<Transform>();
                foreach (Transform child in transform)
                {
                    if (child.GetComponent<Collectible>() == null)
                    {
                        points.Add(child);
                    }
                }

                return points;
            }
        }

        /// <summary>
        /// Picks <paramref name="howMany"/> different indices from 0 to <paramref name="total"/> - 1,
        /// at random. Asking for more than there are gives them all.
        /// </summary>
        public static List<int> Pick(int total, int howMany, System.Random random)
        {
            var indices = new List<int>();
            for (var i = 0; i < total; i++)
            {
                indices.Add(i);
            }

            // Shuffle, then keep the first ones.
            for (var i = indices.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            indices.RemoveRange(Mathf.Min(howMany, total), total - Mathf.Min(howMany, total));
            return indices;
        }

        private void Start()
        {
            Spawn(new System.Random());
        }

        /// <summary>Places the teddies. Public so tests can choose the randomness.</summary>
        public void Spawn(System.Random random)
        {
            if (teddyPrefab == null)
            {
                Debug.LogWarning("CollectibleSpawner: no teddy prefab set, so no collectibles.", this);
                return;
            }

            var points = SpawnPoints;
            foreach (var index in Pick(points.Count, count, random))
            {
                var teddy = Instantiate(teddyPrefab, points[index].position, Quaternion.identity, transform);
                _spawned.Add(teddy.GetComponent<Collectible>());
            }
        }

        private void OnDrawGizmos()
        {
            // Spawn points show in the Scene view as small boxes, the size of a teddy.
            Gizmos.color = new Color(0.6f, 0.35f, 0.15f, 0.9f);
            foreach (var point in SpawnPoints)
            {
                Gizmos.DrawWireCube(point.position, new Vector3(0.74f, 0.87f, 0f));
            }
        }
    }
}
