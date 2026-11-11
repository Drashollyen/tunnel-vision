using UnityEngine;

namespace TunnelVision.Level
{
    public class BigWheelRotation : MonoBehaviour
    {
        private float fRotationSpeed = 20f;
        public Transform transRotationPoint;
        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            transform.Rotate(Vector3.back, -fRotationSpeed * Time.deltaTime);
            transform.RotateAround(transRotationPoint.position, Vector3.back, fRotationSpeed * Time.deltaTime);
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
