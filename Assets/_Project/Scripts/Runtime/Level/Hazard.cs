using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Level
{
    // Kills the player when they touch this object's trigger.
    public class Hazard : MonoBehaviour
    {
        [Tooltip("Tick for a pit: the player just keeps falling, with no death hop.")]
        [SerializeField] private bool fallsIn;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.Kill(fallsIn);
            }
        }
    }
}
