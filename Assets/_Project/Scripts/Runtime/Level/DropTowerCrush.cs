using TunnelVision.Player;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Level
{
    // Kills the player if they touch this trigger while the drop tower seat is falling.
    public class DropTowerCrush : MonoBehaviour
    {
        private DropTower _dropTower;
        private AudioSource _hitSound;

        private void Start()
        {
            _dropTower = GetComponentInParent<DropTower>();
            _hitSound = GetComponentInParent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null && _dropTower != null && _dropTower.IsFalling)
            {
                _hitSound.PlayIfSet();
                player.Kill();
            }
        }
    }
}
