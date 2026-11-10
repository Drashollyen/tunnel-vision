using System.Collections;
using TunnelVision.Level;
using TunnelVision.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.Player
{
    /// <summary>
    /// Handles the player dying. Controls stop, the level lights up so the player can see
    /// what got them, the screen fades to black, and the player comes back at the last
    /// checkpoint. With no lives left, the level reloads.
    /// </summary>
    [RequireComponent(typeof(PlayerLives), typeof(Rigidbody2D))]
    public class PlayerRespawn : MonoBehaviour
    {
        // Just under the HUD (10), so the Round Over text shows on top of the black.
        private const int FadeSortOrder = 9;
        private const float RevealIntensity = 1f;

        [Header("Player")]
        [Tooltip("Optional. Found on this object if left empty.")]
        [SerializeField] private PlayerMovement movement;
        [Tooltip("Optional. The player's own light, found in the children if left empty.")]
        [SerializeField] private Light2D visionLight;

        [Header("Scene")]
        [Tooltip("Optional. The level's global light, found in the scene if left empty.")]
        [SerializeField] private Light2D globalLight;
        [Tooltip("Optional. A full-screen black image. One is made if left empty.")]
        [SerializeField] private CanvasGroup blackFade;

        [Header("Timing")]
        [SerializeField] private float revealDuration = 1f;
        [SerializeField] private float revealHoldDuration = 2f;
        [SerializeField] private float fadeDuration = 0.75f;
        [SerializeField] private float blackHoldDuration = 0.3f;

        private PlayerLives _lives;
        private Rigidbody2D _body;
        private Vector3 _levelStart;
        private Checkpoint _checkpoint;
        private bool _isDying;
        private float _normalGlobalIntensity;
        private GameObject _madeFade;

        /// <summary>True from a death until the player can move again.</summary>
        public bool IsRespawning => _isDying;

        private void Awake()
        {
            _lives = GetComponent<PlayerLives>();
            _body = GetComponent<Rigidbody2D>();
            _levelStart = transform.position;

            if (movement == null)
            {
                movement = GetComponent<PlayerMovement>();
            }

            if (visionLight == null)
            {
                visionLight = GetComponentInChildren<Light2D>();
            }
        }

        private void Start()
        {
            if (globalLight == null)
            {
                globalLight = FindGlobalLight();
            }

            if (globalLight != null)
            {
                _normalGlobalIntensity = globalLight.intensity;
            }

            if (blackFade == null)
            {
                blackFade = MakeBlackFade();
            }

            blackFade.alpha = 0f;
        }

        private void OnDestroy()
        {
            if (_madeFade != null)
            {
                Destroy(_madeFade);
            }
        }

        /// <summary>Takes one life. Further calls do nothing until the player is back in play.</summary>
        public void Kill()
        {
            if (_isDying)
            {
                return;
            }

            _isDying = true;
            StartCoroutine(DeathSequence());
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

        private IEnumerator DeathSequence()
        {
            // Stop controls and come off any moving platform.
            if (movement != null)
            {
                movement.EnableControls(false);
            }

            transform.SetParent(null, true);

            // Stop motion, gravity and collisions.
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.simulated = false;

            if (visionLight != null)
            {
                visionLight.enabled = false;
            }

            // Shows the Round Over or Game Over text.
            _lives.LoseLife();

            // Light up the level briefly.
            yield return FadeGlobalLight(RevealIntensity, revealDuration);
            yield return new WaitForSecondsRealtime(revealHoldDuration);

            // Cover the screen.
            yield return FadeBlack(1f, fadeDuration);
            yield return new WaitForSecondsRealtime(blackHoldDuration);

            if (_lives.IsDead)
            {
                if (TryReloadLevel())
                {
                    yield break;
                }

                // The scene can't be reloaded (it isn't in the build), so start over in place.
                if (_checkpoint != null)
                {
                    _checkpoint.SetLit(false);
                    _checkpoint = null;
                }

                _lives.Restart();
            }

            // Move while the screen is fully black.
            MoveTo(_checkpoint != null ? _checkpoint.RespawnPosition : _levelStart);

            if (globalLight != null)
            {
                globalLight.intensity = _normalGlobalIntensity;
            }

            if (visionLight != null)
            {
                visionLight.enabled = true;
            }

            // Give the camera time to catch up, then show the player before handing back control.
            yield return new WaitForSecondsRealtime(blackHoldDuration);
            yield return FadeBlack(0f, fadeDuration);

            _body.simulated = true;
            _body.linearVelocity = Vector2.zero;

            if (movement != null)
            {
                movement.EnableControls(true);
            }

            _isDying = false;
        }

        private static bool TryReloadLevel()
        {
            var scene = SceneManager.GetActiveScene().name;
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                return false;
            }

            SceneFader.Load(scene);
            return true;
        }

        private void MoveTo(Vector3 position)
        {
            var moved = position - transform.position;
            transform.position = position;
            _body.position = position;

            // Stops the camera sliding across the level to catch up.
            CinemachineCore.OnTargetObjectWarped(transform, moved);
        }

        private IEnumerator FadeGlobalLight(float target, float duration)
        {
            if (globalLight == null)
            {
                yield return new WaitForSecondsRealtime(duration);
                yield break;
            }

            var start = globalLight.intensity;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                globalLight.intensity = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            globalLight.intensity = target;
        }

        private IEnumerator FadeBlack(float target, float duration)
        {
            var start = blackFade.alpha;
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                blackFade.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }

            blackFade.alpha = target;
        }

        private static Light2D FindGlobalLight()
        {
            foreach (var candidate in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
            {
                if (candidate.lightType == Light2D.LightType.Global)
                {
                    return candidate;
                }
            }

            return null;
        }

        private CanvasGroup MakeBlackFade()
        {
            _madeFade = new GameObject("Death Fade");

            var canvas = _madeFade.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = FadeSortOrder;

            var group = _madeFade.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            var image = new GameObject("Black");
            image.transform.SetParent(_madeFade.transform, false);
            var rect = image.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var black = image.AddComponent<Image>();
            black.color = Color.black;
            black.raycastTarget = false;

            return group;
        }
    }
}
