using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        [SerializeField] private float lifetime = 4f;

        private Vector2 _direction = Vector2.right;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.Translate(_direction * speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerRespawn>() != null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Sets which way the projectile flies and turns the sprite to face it.</summary>
        public void Launch(Vector2 direction)
        {
            _direction = direction;

            var scale = transform.localScale;
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(scale.x), scale.y, scale.z);
        }
    }
}
