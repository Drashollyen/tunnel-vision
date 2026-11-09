using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Gameplay
{
    public class Killer : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerDeath player = other.GetComponentInParent<PlayerDeath>();

            if (player != null)
            {
                player.Die();
            }
        }
    }
}
