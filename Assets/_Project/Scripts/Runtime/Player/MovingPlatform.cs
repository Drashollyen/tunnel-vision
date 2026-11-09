using System.Collections;
using UnityEngine;

namespace TunnelVision
{
    public class MovingPlatform : MonoBehaviour
    {
        public Transform pointA;
        public Transform pointB;
        public float moveSpeed = 2f;

        private Vector3 nextPosition;
        private bool platformWaiting;
        [SerializeField] private float waitDuration;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            nextPosition = pointB.position;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, nextPosition, moveSpeed * Time.deltaTime);

            if (transform.position == nextPosition && !platformWaiting)
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
