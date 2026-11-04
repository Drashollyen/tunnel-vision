using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    public class HudTestKeys : MonoBehaviour
    {
        private PlayerLives playerLives;

        private void Awake()
        {
            playerLives = FindAnyObjectByType<PlayerLives>();
        }

        private void Update()
        {
            if (playerLives == null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                if (playerLives.IsDead)
                {
                    playerLives.Restart();
                }
                else
                {
                    playerLives.LoseLife();
                }
            }
        }
    }
}
