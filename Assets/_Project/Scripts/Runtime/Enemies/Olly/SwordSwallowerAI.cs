using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class SwordSwallowerAI : MonoBehaviour
    {
        private static readonly int ThrowParam = Animator.StringToHash("Throw");

        private const float MaxThrowSeconds = 2f;

        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer body;

        [Tooltip("Tick if the sprites are drawn facing right. The sword swallower art faces left.")]
        [SerializeField] private bool artFacesRight;

        [Header("Throwing")]
        [SerializeField] private SwordProjectile swordPrefab;

        [Tooltip("Where the sword leaves his hand, set up for the way the art faces. Mirrored when he turns.")]
        [SerializeField] private Transform throwPoint;

        [Tooltip("How fast the sword flies.")]
        [SerializeField] private float swordSpeed = 10f;

        [Tooltip("Seconds between throws in the same round.")]
        [SerializeField] private float cooldown = 2.5f;

        [Header("Behaviour")]
        [SerializeField] private float range = 10f;
        [SerializeField] private int throwsPerRound = 3;
        [SerializeField] private float reloadTime = 3f;

        private Transform _player;
        private bool _isThrowing;
        private float _throwStartedAt;
        private float _nextThrowAt;
        private int _throwsThisRound;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (body == null)
            {
                body = GetComponent<SpriteRenderer>();
            }

            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("SwordSwallowerAI: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
        }

        private void Update()
        {
            if (_isThrowing && Time.time - _throwStartedAt > MaxThrowSeconds)
            {
                _isThrowing = false;
            }

            if (_isThrowing || Time.time < _nextThrowAt)
            {
                return;
            }

            var distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer > range)
            {
                return;
            }

            StartThrow();
        }

        private void StartThrow()
        {
            Face(_player.position.x - transform.position.x);
            _isThrowing = true;
            _throwStartedAt = Time.time;
            animator.SetTrigger(ThrowParam);
        }

        public void ReleaseSword()
        {
            if (!_isThrowing)
            {
                return;
            }

            _isThrowing = false;
            _throwsThisRound++;

            if (_throwsThisRound >= throwsPerRound)
            {
                _throwsThisRound = 0;
                _nextThrowAt = Time.time + reloadTime;
            }
            else
            {
                _nextThrowAt = Time.time + cooldown;
            }

            if (swordPrefab == null)
            {
                Debug.LogWarning("SwordSwallowerAI: no sword prefab to throw.", this);
                return;
            }

            var sword = Instantiate(swordPrefab, ReleasePoint(), Quaternion.identity);
            var direction = FacingRight() ? 1f : -1f;
            sword.Launch(new Vector2(direction * swordSpeed, 0f));
        }

        private bool FacingRight()
        {
            return body == null ? artFacesRight : artFacesRight != body.flipX;
        }

        private void Face(float direction)
        {
            if (body == null || direction == 0f)
            {
                return;
            }

            body.flipX = (direction > 0f) != artFacesRight;
        }

        private Vector2 ReleasePoint()
        {
            if (throwPoint == null)
            {
                return transform.position;
            }

            var local = throwPoint.localPosition;

            if (FacingRight() != artFacesRight)
            {
                local.x = -local.x;
            }

            return transform.TransformPoint(local);
        }
    }
}
