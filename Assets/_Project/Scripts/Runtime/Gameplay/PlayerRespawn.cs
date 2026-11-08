using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace TunnelVision.Gameplay
{
    /// <summary>
    /// Handles the player dying: takes a life, then returns the player to the last
    /// checkpoint. With no lives left, the level starts again with full lives.
    /// </summary>
    [RequireComponent(typeof(PlayerLives), typeof(Rigidbody2D))]
    public class PlayerRespawn : MonoBehaviour
    {
        [SerializeField] private float respawnDelay = 0.5f;
        [SerializeField] private float gameOverDelay = 2f;

        private PlayerLives _lives;
        private Rigidbody2D _body;
        private Vector3 _levelStart;
        private Checkpoint _checkpoint;
        private bool _isDying;

        private void Awake()
        {
            _lives = GetComponent<PlayerLives>();
            _body = GetComponent<Rigidbody2D>();
            _levelStart = transform.position;
        }

        /// <summary>Takes one life. Further calls do nothing until the player is back in play.</summary>
        public void Kill()
        {
            if (_isDying)
            {
                return;
            }

            _isDying = true;
            _lives.LoseLife();
            StartCoroutine(RespawnAfterDelay());
        }

        public void ReachCheckpoint(Checkpoint checkpoint)
        {
            if (_checkpoint == checkpoint)
            {
                return;
            }

            if (_checkpoint != null)
            {
                _checkpoint.SetLit(false);
            }

            _checkpoint = checkpoint;
            _checkpoint.SetLit(true);
        }

        private IEnumerator RespawnAfterDelay()
        {
            _body.simulated = false;
            yield return new WaitForSeconds(_lives.IsDead ? gameOverDelay : respawnDelay);

            if (_lives.IsDead)
            {
                if (_checkpoint != null)
                {
                    _checkpoint.SetLit(false);
                    _checkpoint = null;
                }

                _lives.Restart();
            }

            MoveTo(_checkpoint != null ? _checkpoint.RespawnPosition : _levelStart);
            _body.linearVelocity = Vector2.zero;
            _body.simulated = true;
            _isDying = false;
        }

        private void MoveTo(Vector3 position)
        {
            var moved = position - transform.position;
            transform.position = position;
            _body.position = position;

            // Stops the camera sliding across the level to catch up.
            CinemachineCore.OnTargetObjectWarped(transform, moved);
        }
    }
}
