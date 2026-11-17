using System.Collections.Generic;
using UnityEngine;

namespace TunnelVision.Level
{
    // Puts a teddy bear on each of this object's children when the level starts.
    // To add or move teddies, add or move the child objects.
    public class CollectibleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject teddyPrefab;

        private readonly List<Collectible> _spawned = new List<Collectible>();

        public IReadOnlyList<Collectible> Spawned => _spawned;

        // The direct children, skipping any teddies already spawned under this object.
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

        // Public so tests can call it.
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
            // Shows each spawn point in the Scene view as a teddy-sized box.
            Gizmos.color = new Color(0.6f, 0.35f, 0.15f, 0.9f);
            foreach (var point in SpawnPoints)
            {
                Gizmos.DrawWireCube(point.position, new Vector3(0.74f, 0.87f, 0f));
            }
        }
    }
}
