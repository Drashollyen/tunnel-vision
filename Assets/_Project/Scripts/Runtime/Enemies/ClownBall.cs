using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    // A ball thrown by the Thrower Clown. Costs Nicholas a life if it hits him, and breaks when it
    // lands or after a few seconds in the air.
    [RequireComponent(typeof(Rigidbody2D))]
    public class ClownBall : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        [Tooltip("Layers the ball breaks on, like the ground.")]
        [SerializeField] private LayerMask breaksOn;

        [Tooltip("Seconds before the ball disappears if it hasn't hit anything.")]
        [SerializeField] private float lifetime = 4f;

        [Tooltip("Spin speed in degrees per second.")]
        [SerializeField] private float spin = 540f;

        // Uses the Rigidbody set in the Inspector, or finds the one on this object.
        private Rigidbody2D Body => body != null ? body : body = GetComponent<Rigidbody2D>();

        // How strongly gravity pulls this ball down. The thrower needs it to aim the ball.
        public float Gravity => -Physics2D.gravity.y * Body.gravityScale;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        public void Launch(Vector2 velocity)
        {
            Body.linearVelocity = velocity;
            Body.angularVelocity = -Mathf.Sign(velocity.x) * spin;
        }

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
