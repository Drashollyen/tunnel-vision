using System.Collections.Generic;
using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Places the level's teddy bears. Its children are hand-placed spawn points, and every run
    /// puts one teddy on each, so the teddies are always in the same places. Add or move teddies
    /// by adding or moving the children.
    /// </summary>
    public class CollectibleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject teddyPrefab;

        private readonly List<Collectible> _spawned = new List<Collectible>();

        /// <summary>The teddies placed this run.</summary>
        public IReadOnlyList<Collectible> Spawned => _spawned;

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

        private void Start()
        {
            Spawn();
        }

        /// <summary>Puts a teddy on every spawn point. Public so tests can call it.</summary>
        public void Spawn()
        {
            if (teddyPrefab == null)
            {
                Debug.LogWarning("CollectibleSpawner: no teddy prefab set, so no collectibles.", this);
                return;
            }

            foreach (var point in SpawnPoints)
            {
                var teddy = Instantiate(teddyPrefab, point.position, Quaternion.identity, transform);
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
