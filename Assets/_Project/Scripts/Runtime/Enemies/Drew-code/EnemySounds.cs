using UnityEngine;

namespace TunnelVision.Enemies
{
    /// <summary>
    /// An enemy's sounds: the clown, the Thrower Clown and the Sword Swallower all use it, each with
    /// only the sounds it needs.
    /// <list type="bullet">
    /// <item>Footsteps: the Run clip calls <see cref="PlayStep"/> as each foot lands; every second one (by default) plays a sound, alternating between the step sounds.</item>
    /// <item>Squeak: plays every so often, at random, while he's walking (his Animator's Speed is above 0.1).</item>
    /// <item>Laugh: <see cref="PlayLaugh"/>, for the AI to call (say, on spotting Nicholas).</item>
    /// <item>Throwing: the Throw clip calls <see cref="PlayWindup"/> on the wind-up and <see cref="PlayThrow"/> on the release.</item>
    /// </list>
    /// Any sound left empty is simply skipped, so an enemy that doesn't walk just leaves the walking sounds empty.
    /// </summary>
    public class EnemySounds : MonoBehaviour
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");
        private const float WalkingSpeed = 0.1f;

        [SerializeField] private AudioSource source;
        [SerializeField] private Animator animator;

        [Header("Walking")]
        [Tooltip("Played in turn, one per footstep.")]
        [SerializeField] private AudioClip[] steps;

        [Tooltip("Plays a step sound on every this-many footsteps: 1 for every step, 2 for every other one.")]
        [Min(1)]
        [SerializeField] private int stepSoundEvery = 2;

        [SerializeField] private AudioClip squeak;

        [Tooltip("The squeak plays at a random moment between these many seconds of walking.")]
        [SerializeField] private Vector2 squeakEverySeconds = new Vector2(2f, 6f);

        [Header("Voice")]
        [SerializeField] private AudioClip laugh;

        [Header("Throwing")]
        [SerializeField] private AudioClip windup;
        [SerializeField] private AudioClip throwSound;

        private int _nextStep;
        private int _footsteps;
        private float _untilSqueak;

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

        /// <summary>
        /// A footstep, called by the Run clip as each foot lands. Only every
        /// <see cref="stepSoundEvery"/>th one makes a sound, alternating between the step sounds.
        /// </summary>
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

            _nextStep %= steps.Length;
            Play(steps[_nextStep]);
            _nextStep++;
        }

        /// <summary>The clown laugh.</summary>
        public void PlayLaugh() => Play(laugh);

        /// <summary>The wind-up before a throw. Called by the Throw clip.</summary>
        public void PlayWindup() => Play(windup);

        /// <summary>The throw itself, as the ball leaves his hand. Called by the Throw clip.</summary>
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
