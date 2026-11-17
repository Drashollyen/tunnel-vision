using System;
using UnityEngine;

namespace TunnelVision.Player
{
    // Counts the teddies the player has picked up this level. The HUD counter listens to CountChanged.
    public class PlayerCollectibles : MonoBehaviour
    {
        public event Action<int> CountChanged;

        public int Count { get; private set; }

        // Finds the player's counter, or adds one to the player if there isn't one yet,
        // so every script that asks gets the same counter. Returns null if there's no player.
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
