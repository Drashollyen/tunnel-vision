using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// The Sword Swallower's thrown sword. Flies straight, point first, and costs Nicholas a life on
    /// a hit, like any hazard. Breaks when it hits the ground or after a few seconds in the air.
    /// Plays its flight sound as it's thrown.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SwordProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        [Tooltip("Layers the sword breaks on when it touches them.")]
        [SerializeField] private LayerMask breaksOn;

        [Tooltip("Seconds before the sword disappears if it hasn't hit anything.")]
        [SerializeField] private float lifetime = 4f;

        [Tooltip("Plays as the sword is thrown. Optional.")]
        [SerializeField] private AudioSource flightSound;

        private Rigidbody2D Body => body != null ? body : body = GetComponent<Rigidbody2D>();

        private void Start()
        {
            Destroy(gameObject, lifetime);

            if (flightSound != null)
            {
                // Spawned after the scene loaded, so bring it in line with the Effects Volume first.
                SoundEffectsVolume.Apply();
                flightSound.Play();
            }
        }

        /// <summary>Sends the sword off at <paramref name="velocity"/>, turned to point the way it flies.</summary>
        public void Launch(Vector2 velocity)
        {
            Body.linearVelocity = velocity;
            transform.rotation = Quaternion.Euler(0f, 0f, AngleFor(velocity));
        }

        /// <summary>The rotation, in degrees, that points the sword (drawn blade to the right) along <paramref name="velocity"/>.</summary>
        public static float AngleFor(Vector2 velocity) => Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponentInParent<PlayerRespawn>();
            if (player != null)
            {
                player.Kill();
                Destroy(gameObject);
                return;
            }

            if ((breaksOn.value & (1 << other.gameObject.layer)) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
