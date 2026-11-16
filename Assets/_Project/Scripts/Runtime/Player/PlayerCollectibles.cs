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

        /// <summary>
        /// The player's collectibles, added to the Player if it has none yet, so everything that
        /// asks gets the same one whatever order it wakes up in. Null when there's no Player.
        /// </summary>
        public static PlayerCollectibles FindOrAdd()
        {
            var found = FindAnyObjectByType<PlayerCollectibles>();
            if (found != null)
            {
                return found;
            }

            var lives = FindAnyObjectByType<PlayerLives>();
            return lives != null ? lives.gameObject.AddComponent<PlayerCollectibles>() : null;
        }

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
