using System.Collections;
using TunnelVision.Audio;
using UnityEngine;

namespace TunnelVision.Level
{
    // A drop tower ride: when the player stands in its trigger the seat drops fast, waits at the
    // bottom, then slowly rises back to the top.
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

        public bool IsFalling => currentState == TowerState.Falling;

        private void Start()
        {
            topPosition = seatPlatform.transform.position;
        }

        private void Update()
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
                    crushSound.PlayIfSet();
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
                    riseSound.StopIfSet();
                }
            }
        }

        private IEnumerator WaitAtBottom()
        {
            yield return new WaitForSeconds(waitDuration);
            currentState = TowerState.Rising;
            riseSound.PlayIfSet();
        }

        // Uses "Stay" rather than "Enter" so it drops again if the player is still there
        // when the seat gets back to the top.
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (currentState == TowerState.Idle && collision.CompareTag("Player"))
            {
                currentState = TowerState.Falling;
                fallSound.PlayIfSet();
            }
        }
    }
}
