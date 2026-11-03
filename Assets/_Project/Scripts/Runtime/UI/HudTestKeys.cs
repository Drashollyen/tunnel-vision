using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TunnelVision.UI
{
    public class HudTestKeys : MonoBehaviour
    {
        [SerializeField] private PlayerLives playerLives;

        private void Update()
        {
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
