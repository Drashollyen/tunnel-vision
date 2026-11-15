using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>Costs the player a life on touching this object's trigger collider.</summary>
    public class DropTowerCrush : MonoBehaviour
    {

        private DropTower dropTower;
        private AudioSource hitSound;

        void Start()
        {
            dropTower = GetComponentInParent<DropTower>();
            hitSound = GetComponentInParent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null && dropTower.IsFalling)
            {
                hitSound.Play();
                player.Kill();
            }
        }
    }
}
