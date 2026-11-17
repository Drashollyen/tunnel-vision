using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    // The Sword Swallower's thrown sword. Flies straight, point first, and costs Nicholas a life if
    // it hits him. Breaks when it hits the ground or after a few seconds in the air.
    [RequireComponent(typeof(Rigidbody2D))]
    public class SwordProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        [Tooltip("Layers the sword breaks on, like the ground.")]
        [SerializeField] private LayerMask breaksOn;

        [Tooltip("Seconds before the sword disappears if it hasn't hit anything.")]
        [SerializeField] private float lifetime = 4f;

        [Tooltip("Optional. Plays as the sword is thrown.")]
        [SerializeField] private AudioSource flightSound;

        // Uses the Rigidbody set in the Inspector, or finds the one on this object.
        private Rigidbody2D Body => body != null ? body : body = GetComponent<Rigidbody2D>();

        private void Start()
        {
            Destroy(gameObject, lifetime);

            if (flightSound != null)
            {
                // The sword is created mid-level, so set its volume to match the Effects Volume setting first.
                SoundEffectsVolume.Apply();
                flightSound.Play();
            }
        }

        public void Launch(Vector2 velocity)
        {
            Body.linearVelocity = velocity;
            transform.rotation = Quaternion.Euler(0f, 0f, AngleFor(velocity));
        }

        // The angle that points the sword the way it is flying. The sword art points to the right.
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

            // Checks whether the thing we touched is on one of the "breaks on" layers.
            if ((breaksOn.value & (1 << other.gameObject.layer)) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
