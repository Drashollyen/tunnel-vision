using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    // Walks back and forth between two points, and runs at Nicholas when he gets close.
    [RequireComponent(typeof(Rigidbody2D))]
    public class Clown : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        // How close counts as "arrived" at a point.
        private const float ArriveDistance = 0.1f;

        [Header("Patrol")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float patrolSpeed = 1.5f;

        [Header("Chase")]
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float chaseRange = 5f;

        [Header("Art")]
        [Tooltip("Tick if the sprites are drawn facing right; untick if they face left.")]
        [SerializeField] private bool artFacesRight = true;
        [Tooltip("Optional. Found on this object if left empty.")]
        [SerializeField] private SpriteRenderer body;
        [Tooltip("Optional. Found on this object if left empty.")]
        [SerializeField] private Animator animator;

        private Rigidbody2D _rigidbody;
        private Transform _player;
        private Transform _currentTarget;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            if (body == null)
            {
                body = GetComponent<SpriteRenderer>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

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
            var distanceToPlayer = Vector2.Distance(_rigidbody.position, _player.position);

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

            if (Mathf.Abs(_rigidbody.position.x - _currentTarget.position.x) < ArriveDistance)
            {
                _currentTarget = _currentTarget == pointA ? pointB : pointA;
            }
        }

        private void MoveToward(float targetX, float speed)
        {
            var distance = targetX - _rigidbody.position.x;
            // -1 to go left, 1 to go right, 0 to stand still once he's there.
            var direction = Mathf.Abs(distance) < ArriveDistance ? 0f : Mathf.Sign(distance);
            var moveSpeed = direction * speed;

            _rigidbody.linearVelocity = new Vector2(moveSpeed, _rigidbody.linearVelocity.y);

            if (animator != null)
            {
                animator.SetFloat(SpeedParam, Mathf.Abs(moveSpeed));
            }

            if (direction != 0f)
            {
                Face(direction);
            }
        }

        private void Face(float direction)
        {
            if (body != null)
            {
                // Flip only when he needs to face the other way from how he's drawn.
                body.flipX = (direction > 0f) != artFacesRight;
            }
        }
    }
}
