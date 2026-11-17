using System.Collections;
using System.Collections.Generic;
using TunnelVision.Level;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TunnelVision.Player
{
    // Handles the player dying: controls stop, the level lights up so the player can see what got
    // them, the screen fades to black, and they come back at the last checkpoint.
    // With no lives left it stays on the black Game Over screen (see RoundOverMessage).
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

        [Header("Death Fall")]
        [Tooltip("On death, pause, hop up, then drop down through the floor.")]
        [SerializeField] private bool fallOnDeath = true;
        [Tooltip("Seconds frozen in the death pose before the hop.")]
        [SerializeField] private float deathPause = 0.4f;
        [Tooltip("How fast the hop starts going up.")]
        [SerializeField] private float deathHopSpeed = 9f;
        [Tooltip("How hard the fall pulls down.")]
        [SerializeField] private float deathFallGravity = 30f;

        private PlayerLives _lives;
        private Rigidbody2D _body;
        private Vector3 _levelStart;
        private Checkpoint _checkpoint;
        private bool _isDying;
        private float _normalGlobalIntensity;
        private GameObject _madeFade;
        private Coroutine _fall;
        private readonly List<CinemachineCamera> _heldCameras = new List<CinemachineCamera>();

        // True from a death until the player can move again.
        public bool IsRespawning => _isDying;

        // Only raised for a new checkpoint, not the one the player already respawns at.
        public event System.Action<Checkpoint> CheckpointReached;

        public event System.Action Died;

        // Raised as the screen starts fading back in on the respawned player.
        public event System.Action Respawning;

        // How long from a death until the screen is fully black.
        public float SecondsUntilBlack => revealDuration + revealHoldDuration + fadeDuration;

        public float FadeInSeconds => fadeDuration;

        public float FadeToBlackSeconds => fadeDuration;

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

        // Takes one life and starts the death sequence. Does nothing if they're already dying.
        // fellIntoPit skips the death hop, so the player just keeps falling.
        public void Kill(bool fellIntoPit = false)
        {
            // Nothing can kill the player once the level is complete and they're walking off.
            if (_isDying || UI.LevelCompleteScreen.IsAnyShowing)
            {
                return;
            }

            _isDying = true;
            Died?.Invoke();
            StartCoroutine(DeathSequence(fellIntoPit));
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
            if (_lives != null)
            {
                _lives.GainLives(checkpoint.TakeLives()); // Only the first visit pays out.
            }

            CheckpointReached?.Invoke(checkpoint);
        }

        private IEnumerator DeathSequence(bool fellIntoPit)
        {
            // Stop controls and come off any moving platform.
            if (movement != null)
            {
                movement.EnableControls(false);
            }

            transform.SetParent(null, true);

            // Falling into a pit carries on at the speed they were already falling.
            var fallingSpeed = Mathf.Min(0f, _body.linearVelocity.y);

            // Stop motion, gravity and collisions.
            _body.linearVelocity = Vector2.zero;
            _body.angularVelocity = 0f;
            _body.simulated = false;

            if (visionLight != null)
            {
                visionLight.enabled = false;
            }

            // The death pose, then a hop and a drop through the floor while the level lights up.
            if (movement != null)
            {
                movement.SetDead(true);
            }

            if (fallOnDeath)
            {
                // The camera holds still while they fall out of the level, like old platformers;
                // following them down would drag it along the bottom of the camera bounds.
                HoldCameras();
                _fall = StartCoroutine(fellIntoPit ? Fall(fallingSpeed) : DeathFall());
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
                // Game Over: stay on the black screen. RoundOverMessage pauses the game and
                // offers the way back to Level Select. IsRespawning stays true, so nothing
                // can kill the player again.
                yield break;
            }

            // While the screen is fully black, move the player back and face them right again.
            if (_fall != null)
            {
                StopCoroutine(_fall);
                _fall = null;
            }

            ReleaseCameras();
            MoveTo(_checkpoint != null ? _checkpoint.RespawnPosition : _levelStart);
            if (movement != null)
            {
                movement.FaceStartDirection();
                movement.SetDead(false);
            }

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
            Respawning?.Invoke();
            yield return FadeBlack(0f, fadeDuration);

            _body.simulated = true;
            _body.linearVelocity = Vector2.zero;

            if (movement != null)
            {
                movement.EnableControls(true);
            }

            _isDying = false;
        }

        // The old-school platformer death: freeze for a moment, hop up, then drop through the floor.
        private IEnumerator DeathFall()
        {
            yield return new WaitForSecondsRealtime(deathPause);
            yield return Fall(deathHopSpeed);
        }

        // Moves the player up and down by hand (physics is off while dying) until the screen is black.
        // A positive startSpeed means a hop up first. Uses unscaled time so it still plays if the game is paused.
        private IEnumerator Fall(float startSpeed)
        {
            var velocity = startSpeed;
            var stopAt = Time.unscaledTime + SecondsUntilBlack;
            while (Time.unscaledTime < stopAt)
            {
                var dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                velocity -= deathFallGravity * dt;
                transform.position += new Vector3(0f, velocity * dt, 0f);
                yield return null;
            }

            _fall = null;
        }

        // Stops the cameras following the player until ReleaseCameras is called.
        private void HoldCameras()
        {
            _heldCameras.Clear();
            foreach (var cam in FindObjectsByType<CinemachineCamera>())
            {
                if (cam.Follow == transform)
                {
                    cam.Follow = null;
                    _heldCameras.Add(cam);
                }
            }
        }

        private void ReleaseCameras()
        {
            foreach (var cam in _heldCameras)
            {
                if (cam != null)
                {
                    cam.Follow = transform;

                    // Snap straight to the respawned player instead of gliding across the level.
                    cam.PreviousStateIsValid = false;
                }
            }

            _heldCameras.Clear();
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
            foreach (var candidate in FindObjectsByType<Light2D>())
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
