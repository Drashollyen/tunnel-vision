using System.Collections;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Level
{
    // Moves back and forth between point A and point B, pausing at each end.
    public class MovingPlatform : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        public float moveSpeed = 2f;

        private Vector3 _nextPosition;
        private bool _waiting;
        [SerializeField] private float waitDuration;
        public AudioSource movingSound;

        private Rigidbody2D _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _nextPosition = pointB.position;
            movingSound.PlayIfSet();
        }

        private void FixedUpdate()
        {
            Vector2 newPosition = Vector2.MoveTowards(_rb.position, _nextPosition, moveSpeed * Time.fixedDeltaTime);
            _rb.MovePosition(newPosition);

            if (Vector2.Distance(_rb.position, _nextPosition) < 0.01f && !_waiting)
            {
                movingSound.StopIfSet();
                _waiting = true;
                StartCoroutine(WaitThenTurnAround(waitDuration));
            }
        }

        private IEnumerator WaitThenTurnAround(float duration)
        {
            yield return new WaitForSeconds(duration);
            _waiting = false;
            _nextPosition = (_nextPosition == pointA.position) ? pointB.position : pointA.position;
            movingSound.PlayIfSet();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // The player becomes a child of the platform so they ride along with it.
                collision.gameObject.transform.parent = transform;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                RideParenting.LetGo(collision.transform, transform);
            }
        }
    }
}
