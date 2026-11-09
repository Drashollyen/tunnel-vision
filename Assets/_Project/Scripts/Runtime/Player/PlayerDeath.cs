using System.Collections;
using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TunnelVision.Player
{

    public class PlayerDeath : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private PlayerLives lives;
        [SerializeField] private Light2D visionLight;

        [Header("Scene")]
        [SerializeField] private Light2D globalLight;
        [SerializeField] private CanvasGroup blackFade;

        [Header("Timing")]
        [SerializeField] private float revealDuration = 1f;
        [SerializeField] private float revealHoldDuration = 2f;
        [SerializeField] private float fadeDuration = 0.75f;
        [SerializeField] private float blackHoldDuration = 0.3f;

        private Vector3 respawnPosition;
        private float normalGlobalIntensity;
        private bool deathInProgress;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            respawnPosition = transform.position;
            normalGlobalIntensity = globalLight.intensity;
            blackFade.alpha = 0f;
        }

        public void Die()
        {
            if (deathInProgress)
            { return; }

            deathInProgress = true;
            StartCoroutine(DeathSequence());
        }

        public void SetCheckpoint(Vector3 position)
        {
            respawnPosition = position;
        }

        private IEnumerator DeathSequence()
        {
            // Stop controls and detach from any moving platforms
            movement.EnableControls(false);
            transform.SetParent(null, true);

            // Stop motion, gravity and physical interactions
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;

            visionLight.enabled = false;
            lives.LoseLife();

            // Reveal the level briefly
            yield return FadeGlobalLight(1f, revealDuration);
            yield return new WaitForSecondsRealtime(revealHoldDuration);

            // Cover the entire screen
            yield return FadeBlack(1f, fadeDuration);
            yield return new WaitForSecondsRealtime(blackHoldDuration);

            if (lives.IsDead)
            {
                // Reload the level
                SceneManager.LoadScene(
                    SceneManager.GetActiveScene().buildIndex
                );
                yield break;
            }

            // Reposition while the screen is completely black.
            transform.position = respawnPosition;
            rb.position = new Vector2(
                respawnPosition.x,
                respawnPosition.y
            );

            globalLight.intensity = normalGlobalIntensity;
            visionLight.enabled = true;

            // Allow the camera time to catch up
            yield return new WaitForSecondsRealtime(blackHoldDuration);

            // Show the starting position before restoring control
            yield return FadeBlack(0f, fadeDuration);

            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            movement.EnableControls(true);

            deathInProgress = false;
        }

        private IEnumerator FadeGlobalLight(float target, float duration)
        {
            float start = globalLight.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                globalLight.intensity = Mathf.Lerp(
                    start,
                    target,
                    Mathf.Clamp01(elapsed / duration)
                );

                yield return null;
            }

            globalLight.intensity = target;
        }

        private IEnumerator FadeBlack(float target, float duration)
        {
            float start = blackFade.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;

                blackFade.alpha = Mathf.Lerp(
                    start,
                    target,
                    Mathf.Clamp01(elapsed / duration)
                );

                yield return null;
            }

            blackFade.alpha = target;
        }
    }
}
