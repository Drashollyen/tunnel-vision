using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// A juggling ball thrown by the Thrower Clown. Costs Nicholas a life on a hit, like any hazard,
    /// and breaks when it lands on the ground or after a few seconds in the air.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ClownBall : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;

        [Tooltip("Layers the ball breaks on when it touches them.")]
        [SerializeField] private LayerMask breaksOn;

        [Tooltip("Seconds before the ball disappears if it hasn't hit anything.")]
        [SerializeField] private float lifetime = 4f;

        [Tooltip("How fast the ball spins in flight, in degrees per second.")]
        [SerializeField] private float spin = 540f;

        private Rigidbody2D Body => body != null ? body : body = GetComponent<Rigidbody2D>();

        /// <summary>The downward pull on this ball, in units per second squared.</summary>
        public float Gravity => -Physics2D.gravity.y * Body.gravityScale;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        /// <summary>Sends the ball off at <paramref name="velocity"/>, spinning the way it flies.</summary>
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

            if ((breaksOn.value & (1 << other.gameObject.layer)) != 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
