using System.Collections;
using UnityEngine;

namespace TunnelVision.Level
{
    public class MovingPlatform : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        public float moveSpeed = 2f;

        private Vector3 nextPosition;
        private bool platformWaiting;
        [SerializeField] private float waitDuration;

        private Rigidbody2D rb;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            nextPosition = pointB.position;

        }

        // Update is called once per frame
        void FixedUpdate()
        {
            Vector2 newPosition = Vector2.MoveTowards(rb.position, nextPosition, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);

            if (Vector2.Distance(rb.position, nextPosition) < 0.01f && !platformWaiting)
            {
                // Wait for a few seconds
                platformWaiting = true;
                StartCoroutine(PlatformWait(waitDuration));
            }
        }

        IEnumerator PlatformWait(float duration)
        {
            yield return new WaitForSeconds(duration);
            platformWaiting = false;
            // Set the new target position
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.transform.parent = transform;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.transform.parent = null;
            }
        }
    }
}
