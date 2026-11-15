using System;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Player
{
    /// <summary>
    /// The player's lives: starts full, loses one per death, and can win some back (checkpoints
    /// do), never past the maximum.
    /// </summary>
    public class PlayerLives : MonoBehaviour
    {
        [SerializeField] private int maxLives = 4;
        public event Action<int> LivesChanged;
        public int CurrentLives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsDead => CurrentLives <= 0;

        /// <summary>Deaths since the level started (or restarted), whatever was won back since.</summary>
        public int LivesLost { get; private set; }

        [SerializeField] private AudioSource deathSound;

        private void Awake()
        {
            CurrentLives = maxLives;
        }

        public void LoseLife()
        {
            if (IsDead)
            {
                return;
            }

            deathSound.PlayIfSet();
            CurrentLives--;
            LivesLost++;
            LivesChanged?.Invoke(CurrentLives);
        }

        /// <summary>Gives back up to this many lives, never past the maximum. Returns how many were added.</summary>
        public int GainLives(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return 0;
            }

            var added = Mathf.Min(amount, maxLives - CurrentLives);
            if (added > 0)
            {
                CurrentLives += added;
                LivesChanged?.Invoke(CurrentLives);
            }

            return added;
        }

        public void Restart()
        {
            CurrentLives = maxLives;
            LivesLost = 0;
            LivesChanged?.Invoke(CurrentLives);
        }
    }
}
