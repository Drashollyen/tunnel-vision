using System;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Level
{
    // A teddy bear the player can pick up. It bobs up and down; touching it adds one to the
    // player's count, plays a chime and pops (grows and fades) before disappearing.
    // It stays collected even if the player dies afterwards. Started from Ash's collectible experiment.
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        public const float PopSeconds = 0.2f;
        private const float PopScale = 1.4f;

        [SerializeField] private SpriteRenderer sprite;
        [Tooltip("Optional. Played on pickup. Keep it on this object so the Effects volume slider controls it.")]
        [SerializeField] private AudioSource pickupSound;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 _restPosition;
        private Vector3 _restScale;
        private float _bobPhase;
        private float _collectedAt = float.NegativeInfinity;

        public event Action<Collectible> Collected;

        public bool IsCollected { get; private set; }

        public static float BobOffset(float time, float height, float speed) => Mathf.Sin(time * speed) * height;

        private void Awake()
        {
            if (sprite == null)
            {
                sprite = GetComponentInChildren<SpriteRenderer>();
            }

            _restPosition = transform.localPosition;
            _restScale = transform.localScale;

            // Each teddy bobs out of step with the others.
            _bobPhase = UnityEngine.Random.value * Mathf.PI * 2f;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCollected)
            {
                return;
            }

            var player = other.GetComponentInParent<PlayerLives>();
            if (player == null)
            {
                return;
            }

            var collectibles = player.GetComponent<PlayerCollectibles>();
            if (collectibles == null)
            {
                collectibles = player.gameObject.AddComponent<PlayerCollectibles>();
            }

            Collect(collectibles);
        }

        // Public so tests can call it.
        public void Collect(PlayerCollectibles collectibles)
        {
            if (IsCollected)
            {
                return;
            }

            IsCollected = true;
            _collectedAt = Time.time;
            GetComponent<Collider2D>().enabled = false;

            if (collectibles != null)
            {
                collectibles.Add();
            }

            pickupSound.PlayIfSet();
            Collected?.Invoke(this);
        }

        private void Update()
        {
            if (!IsCollected)
            {
                transform.localPosition = _restPosition + Vector3.up * BobOffset(Time.time + _bobPhase, bobHeight, bobSpeed);
                return;
            }

            // The pop: grow and fade, then hide once the chime has finished too.
            var popped = Mathf.Clamp01((Time.time - _collectedAt) / PopSeconds);
            transform.localScale = _restScale * Mathf.Lerp(1f, PopScale, popped);
            if (sprite != null)
            {
                var color = sprite.color;
                color.a = 1f - popped;
                sprite.color = color;
            }

            var soundDone = pickupSound == null || !pickupSound.isPlaying;
            if (popped >= 1f && soundDone)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
