using UnityEngine;

namespace TunnelVision.Gameplay
{
    /// <summary>Costs the player a life on touching this object's trigger collider.</summary>
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.Kill();
            }
        }
    }
}
