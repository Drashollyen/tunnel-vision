using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Balloon : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 1f;

        private Transform _currentTarget;

        private void Start()
        {
            _currentTarget = pointB;
        }

        private void Update()
        {
            transform.position = Vector2.MoveTowards(transform.position, _currentTarget.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, _currentTarget.position) < ArriveDistance)
            {
                _currentTarget = _currentTarget == pointA ? pointB : pointA;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerRespawn>() != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
