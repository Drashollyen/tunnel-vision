using System;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Player
{
    public class PlayerLives : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        public event Action<int> LivesChanged;
        public int CurrentLives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsDead => CurrentLives <= 0;
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
            LivesChanged?.Invoke(CurrentLives);
        }

        public void Restart()
        {
            CurrentLives = maxLives;
            LivesChanged?.Invoke(CurrentLives);
        }
    }
}
