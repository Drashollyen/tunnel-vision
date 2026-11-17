using TunnelVision.Player;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Level
{
    // Becomes the player's respawn point when touched, and the first time also gives back some lives.
    // Place the object's origin on top of the ground.
    public class Checkpoint : MonoBehaviour
    {
        [Tooltip("Lives given back the first time the player reaches this checkpoint.")]
        [SerializeField] private int livesGranted = 2;
        private bool _livesTaken;

        // Gives the full amount the first time, then 0 after that.
        public int TakeLives()
        {
            if (_livesTaken)
            {
                return 0;
            }

            _livesTaken = true;
            return livesGranted;
        }

        [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 1.42f);
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Color waitingColor = new Color(0.55f, 0.55f, 0.55f);
        [SerializeField] private Color activeColor = new Color(1f, 0.6f, 0.85f);

        [Tooltip("Optional. With both sprites set, it swaps sprites instead of changing colour.")]
        [SerializeField] private Sprite waitingSprite;
        [SerializeField] private Sprite activeSprite;

        public Vector3 RespawnPosition => transform.position + (Vector3)respawnOffset;
        private AudioSource _bellSound;
        private bool _bellPlayed;

        private void Awake()
        {
            SetLit(false);
        }

        private void Start()
        {
            _bellSound = GetComponent<AudioSource>();
        }

        public void SetLit(bool lit)
        {
            if (sprite == null)
            {
                return;
            }

            if (waitingSprite != null && activeSprite != null)
            {
                sprite.sprite = lit ? activeSprite : waitingSprite;
                sprite.color = Color.white;
                return;
            }

            sprite.color = lit ? activeColor : waitingColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.ReachCheckpoint(this);
                if (!_bellPlayed)
                {
                    _bellSound.PlayIfSet();
                    _bellPlayed = true;
                }
            }
        }
    }
}
