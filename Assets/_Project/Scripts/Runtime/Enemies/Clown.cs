using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Clown : MonoBehaviour
    {
        private const float ArriveDistance = 0.1f;

        [Header("Patrol")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float patrolSpeed = 1.5f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float chaseRange = 5f;

        private Rigidbody2D _body;
        private Transform _player;
        private Transform _currentTarget;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();

            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("Clown: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
            _currentTarget = pointB;
        }

        private void FixedUpdate()
        {
            var distanceToPlayer = Vector2.Distance(_body.position, _player.position);

            if (distanceToPlayer < chaseRange)
            {
                MoveToward(_player.position.x, chaseSpeed);
            }
            else
            {
                Patrol();
            }
        }

        private void Patrol()
        {
            MoveToward(_currentTarget.position.x, patrolSpeed);

            if (Mathf.Abs(_body.position.x - _currentTarget.position.x) < ArriveDistance)
            {
                _currentTarget = _currentTarget == pointA ? pointB : pointA;
            }
        }

        private void MoveToward(float targetX, float speed)
        {
            var distance = targetX - _body.position.x;
            var direction = Mathf.Abs(distance) < ArriveDistance ? 0f : Mathf.Sign(distance);

            // Only the sideways speed is set, so gravity still pulls him down.
            _body.linearVelocity = new Vector2(direction * speed, _body.linearVelocity.y);

            if (direction != 0f)
            {
                var scale = transform.localScale;
                transform.localScale = new Vector3(direction * Mathf.Abs(scale.x), scale.y, scale.z);
            }
        }
    }
}
