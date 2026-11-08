using UnityEngine;

namespace TunnelVision.Gameplay
{
    /// <summary>
    /// Becomes the player's respawn point when touched. It doesn't restore lives.
    /// Place the object's origin on top of the ground.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 1.42f);
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Color waitingColor = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color activeColor = new Color(1f, 0.6f, 0.85f);

        public Vector3 RespawnPosition => transform.position + (Vector3)respawnOffset;

        private void Awake()
        {
            SetLit(false);
        }

        public void SetLit(bool lit)
        {
            if (sprite != null)
            {
                sprite.color = lit ? activeColor : waitingColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.ReachCheckpoint(this);
            }
        }
    }
}
