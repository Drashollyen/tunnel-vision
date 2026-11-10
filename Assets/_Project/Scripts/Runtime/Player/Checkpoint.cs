using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Gameplay
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer checkpointRenderer;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite activatedSprite;
        [SerializeField] private Transform respawnPoint;

        private bool activated;

        private void Awake()
        {
            checkpointRenderer.sprite = defaultSprite;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated)
            {
                return;
            }

            PlayerDeath player = other.GetComponentInParent<PlayerDeath>();

            if (player == null)
            {
                return;
            }

            player.SetCheckpoint(respawnPoint.position);

            activated = true;
            checkpointRenderer.sprite = activatedSprite;
        }

    }
}
