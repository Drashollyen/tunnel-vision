using System;
using UnityEngine;

namespace TunnelVision.Player
{
    /// <summary>
    /// How many collectibles the player has picked up this level. A pickup calls Add().
    /// The HUD's CollectibleCounter listens to CountChanged.
    /// </summary>
    public class PlayerCollectibles : MonoBehaviour
    {
        public event Action<int> CountChanged;

        public int Count { get; private set; }

        /// <summary>Adds to the count. Zero or less is ignored.</summary>
        public void Add(int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            Count += amount;
            CountChanged?.Invoke(Count);
        }
    }
}
