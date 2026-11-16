using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class ThrowerBrain : MonoBehaviour
    {
        [Tooltip("Optional. The Thrower Clown's body, found on this object if left empty.")]
        [SerializeField] private ClownThrower clownThrower;
        [Tooltip("Optional. The Sword Swallower's body, found on this object if left empty.")]
        [SerializeField] private SwordSwallower swordSwallower;

        [Header("Throwing")]
        [SerializeField] private float range = 10f;
        [SerializeField] private int throwsPerRound = 3;
        [SerializeField] private float reloadTime = 3f;

        private Transform _player;
        private int _throwsThisRound;
        private float _nextRoundAt;

        private void Awake()
        {
            if (clownThrower == null)
            {
                clownThrower = GetComponent<ClownThrower>();
            }

            if (swordSwallower == null)
            {
                swordSwallower = GetComponent<SwordSwallower>();
            }

            if (clownThrower == null && swordSwallower == null)
            {
                Debug.LogError("ThrowerBrain: needs a ClownThrower or a SwordSwallower on the same object.", this);
                enabled = false;
                return;
            }

            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("ThrowerBrain: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
        }

        private void Update()
        {
            if (Time.time < _nextRoundAt)
            {
                return;
            }

            var distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer > range)
            {
                return;
            }

            if (TryThrow())
            {
                _throwsThisRound++;

                if (_throwsThisRound >= throwsPerRound)
                {
                    _throwsThisRound = 0;
                    _nextRoundAt = Time.time + reloadTime;
                }
            }
        }

        private bool TryThrow()
        {
            if (clownThrower != null)
            {
                return clownThrower.CanThrow && clownThrower.ThrowAt(_player);
            }

            return swordSwallower.CanThrow && swordSwallower.ThrowAt(_player);
        }
    }
}
