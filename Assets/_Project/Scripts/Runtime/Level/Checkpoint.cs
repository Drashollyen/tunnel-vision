using TunnelVision.Player;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// Becomes the player's respawn point when touched. The first time, it also gives back up
    /// to <see cref="LivesGranted"/> lives (never past the maximum).
    /// Place the object's origin on top of the ground.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        public const int DefaultLivesGranted = 2;

        [Tooltip("Lives given back the first time the player reaches this checkpoint.")]
        [SerializeField] private int livesGranted = DefaultLivesGranted;
        private bool _livesTaken;

        public int LivesGranted => livesGranted;

        /// <summary>The lives this checkpoint gives: its full amount the first time, then none.</summary>
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

        [Tooltip("Optional. When both are set, the checkpoint swaps sprites instead of changing colour.")]
        [SerializeField] private Sprite waitingSprite;
        [SerializeField] private Sprite activeSprite;

        public Vector3 RespawnPosition => transform.position + (Vector3)respawnOffset;
        private AudioSource bellSound;
        private bool bellPlayed = false;

        private void Awake()
        {
            SetLit(false);
        }

        void Start()
        {
            bellSound = GetComponent<AudioSource>();
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
                if (!bellPlayed)
                {
                    bellSound.PlayIfSet();
                    bellPlayed = true;
                }

            }
        }
    }
}
