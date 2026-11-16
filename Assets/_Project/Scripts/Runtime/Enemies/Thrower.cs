using System.Collections;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    public class Thrower : MonoBehaviour
    {
        [Header("Throwing")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform throwPoint;
        [SerializeField] private int throwsPerRound = 3;
        [SerializeField] private float range = 10f;

        [Header("Timing")]
        [SerializeField] private float timeBetweenThrows = 0.7f;
        [SerializeField] private float reloadTime = 3f;

        private Transform _player;

        private void Awake()
        {
            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("Thrower: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
        }

        private void Start()
        {
            StartCoroutine(ThrowLoop());
        }

        private IEnumerator ThrowLoop()
        {
            while (true)
            {
                var distanceToPlayer = Vector2.Distance(transform.position, _player.position);

                if (distanceToPlayer < range)
                {
                    for (var i = 0; i < throwsPerRound; i++)
                    {
                        Throw();
                        yield return new WaitForSeconds(timeBetweenThrows);
                    }

                    yield return new WaitForSeconds(reloadTime);
                }
                else
                {
                    yield return null;
                }
            }
        }

        private void Throw()
        {
            var direction = Mathf.Sign(_player.position.x - transform.position.x);

            var scale = transform.localScale;
            transform.localScale = new Vector3(direction * Mathf.Abs(scale.x), scale.y, scale.z);

            var projectile = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);
            projectile.Launch(new Vector2(direction, 0f));
        }
    }
}
