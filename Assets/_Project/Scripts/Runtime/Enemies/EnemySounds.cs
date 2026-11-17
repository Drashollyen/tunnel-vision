using UnityEngine;

namespace TunnelVision.Enemies
{
    // Plays an enemy's sounds. The clowns and the Sword Swallower all use it. Any sound left empty
    // is skipped, so each enemy only fills in the sounds it needs.
    // PlayStep, PlayWindup and PlayThrow are called by animation events on the Run and Throw clips.
    public class EnemySounds : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private const float WalkingSpeed = 0.1f;

        [SerializeField] private AudioSource source;
        [SerializeField] private Animator animator;

        [Header("Walking")]
        [Tooltip("Played in turn, one after the other.")]
        [SerializeField] private AudioClip[] steps;

        [Tooltip("1 plays a sound on every footstep, 2 on every other one, and so on.")]
        [Min(1)]
        [SerializeField] private int stepSoundEvery = 2;

        [SerializeField] private AudioClip squeak;

        [Tooltip("While walking, the squeak plays at a random time between these two numbers of seconds.")]
        [SerializeField] private Vector2 squeakEverySeconds = new Vector2(2f, 6f);

        [Header("Voice")]
        [SerializeField] private AudioClip laugh;

        [Header("Throwing")]
        [SerializeField] private AudioClip windup;
        [SerializeField] private AudioClip throwSound;

        private int _nextStep;
        private int _footsteps;
        private float _untilSqueak;

        // Fills in the references when the script is first added in the Inspector.
        private void Reset()
        {
            source = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            if (source == null)
            {
                source = GetComponent<AudioSource>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            _untilSqueak = NextSqueakDelay();
        }

        private void Update()
        {
            if (squeak == null || !IsWalking())
            {
                return;
            }

            _untilSqueak -= Time.deltaTime;
            if (_untilSqueak <= 0f)
            {
                Play(squeak);
                _untilSqueak = NextSqueakDelay();
            }
        }

        private bool IsWalking()
        {
            return animator != null
                && animator.runtimeAnimatorController != null
                && animator.GetFloat(SpeedParam) > WalkingSpeed;
        }

        private float NextSqueakDelay()
        {
            var min = Mathf.Max(0.1f, Mathf.Min(squeakEverySeconds.x, squeakEverySeconds.y));
            var max = Mathf.Max(min, Mathf.Max(squeakEverySeconds.x, squeakEverySeconds.y));
            return Random.Range(min, max);
        }

        // Called by the Run clip each time a foot lands.
        public void PlayStep()
        {
            if (steps == null || steps.Length == 0)
            {
                return;
            }

            var every = Mathf.Max(1, stepSoundEvery);
            var footstep = _footsteps++;
            if (footstep % every != 0)
            {
                return;
            }

            // Wraps back to the first sound after the last one.
            _nextStep %= steps.Length;
            Play(steps[_nextStep]);
            _nextStep++;
        }

        public void PlayLaugh() => Play(laugh);

        public void PlayWindup() => Play(windup);

        public void PlayThrow() => Play(throwSound);

        private void Play(AudioClip clip)
        {
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }
    }
}
