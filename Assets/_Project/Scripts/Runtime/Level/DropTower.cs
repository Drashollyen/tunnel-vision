using System.Collections;
using UnityEngine;

namespace TunnelVision.Level
{
    public class DropTower : MonoBehaviour
    {
        private enum TowerState { Idle, Falling, Waiting, Rising }

        [Header("Movement")]
        [SerializeField] private float fallSpeed = 15f;
        [SerializeField] private float returnSpeed = 2f;
        [SerializeField] private float waitDuration = 1f;

        [Header("References")]
        [SerializeField] private GameObject seatPlatform;
        [SerializeField] private Transform bottomPoint;

        [Header("Audio")]
        [SerializeField] private AudioSource fallSound;
        [SerializeField] private AudioSource crushSound;
        [SerializeField] private AudioSource riseSound;

        private TowerState currentState = TowerState.Idle;
        private Vector3 topPosition;
        private bool playerOnPlatform;

        public bool IsFalling => currentState == TowerState.Falling;

        void Start()
        {
            // Record the starting height as the return point
            topPosition = seatPlatform.transform.position;
        }

        void Update()
        {
            if (currentState == TowerState.Falling)
            {
                seatPlatform.transform.position = Vector3.MoveTowards(
                    seatPlatform.transform.position,
                    bottomPoint.position,
                    fallSpeed * Time.deltaTime
                );

                if (seatPlatform.transform.position == bottomPoint.position)
                {
                    currentState = TowerState.Waiting;
                    crushSound.Play();
                    StartCoroutine(WaitAtBottom());
                }
            }
            else if (currentState == TowerState.Rising)
            {
                seatPlatform.transform.position = Vector3.MoveTowards(
                    seatPlatform.transform.position,
                    topPosition,
                    returnSpeed * Time.deltaTime
                );

                if (seatPlatform.transform.position == topPosition)
                {
                    currentState = TowerState.Idle;
                    riseSound.Stop();
                }
            }
        }

        private IEnumerator WaitAtBottom()
        {
            yield return new WaitForSeconds(waitDuration);
            currentState = TowerState.Rising;
            riseSound.Play();
        }

        // Using OnTriggerStay2D ensures it also triggers if the player is still standing
        // underneath when the tower finishes resetting at the top
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (currentState == TowerState.Idle && !playerOnPlatform && collision.CompareTag("Player"))
            {
                currentState = TowerState.Falling;
                fallSound.Play();
            }
        }

    }
}
