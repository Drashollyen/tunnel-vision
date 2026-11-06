using System;
using UnityEngine;

namespace TunnelVision.Gameplay
{
    public class PlayerLives : MonoBehaviour
    {
        [SerializeField] private int maxLives = 3;
        public event Action<int> LivesChanged;
        public int CurrentLives { get; private set; }
        public int MaxLives => maxLives;
        public bool IsDead => CurrentLives <= 0;

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
