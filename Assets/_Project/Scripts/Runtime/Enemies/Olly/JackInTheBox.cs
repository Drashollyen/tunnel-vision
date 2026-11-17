using System.Collections;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.Enemies
{
    // Springs up out of its box when Nicholas comes close, then drops back down and waits before it
    // can spring again. The jack's collider is only switched on while it is up.
    public class JackInTheBox : MonoBehaviour
    {
        [Tooltip("The child object that pops up. Its collider is only on while it is up.")]
        [SerializeField] private Transform jack;
        [SerializeField] private float triggerRange = 1.5f;

        [Header("Spring")]
        [SerializeField] private float springHeight = 2f;
        [SerializeField] private float springSpeed = 12f;

        [Header("Timing")]
        [SerializeField] private float stayUpTime = 0.5f;
        [Tooltip("Seconds it waits after dropping back down before it can spring again.")]
        [SerializeField] private float windUpTime = 1.5f;

        private Transform _player;
        private Collider2D _jackCollider;
        private Vector3 _hiddenPosition;
        private bool _isBusy;

        private void Awake()
        {
            var playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("JackInTheBox: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
                return;
            }

            _player = playerLives.transform;
            _jackCollider = jack.GetComponent<Collider2D>();
            _hiddenPosition = jack.localPosition;
            SetJackDangerous(false);
        }

        private void Update()
        {
            if (_isBusy)
            {
                return;
            }

            var distanceToPlayer = Vector2.Distance(transform.position, _player.position);

            if (distanceToPlayer < triggerRange)
            {
                StartCoroutine(Spring());
            }
        }

        private IEnumerator Spring()
        {
            _isBusy = true;
            SetJackDangerous(true);

            var upPosition = _hiddenPosition + new Vector3(0f, springHeight, 0f);
            yield return MoveJackTo(upPosition);
            yield return new WaitForSeconds(stayUpTime);

            yield return MoveJackTo(_hiddenPosition);
            SetJackDangerous(false);

            yield return new WaitForSeconds(windUpTime);
            _isBusy = false;
        }

        private IEnumerator MoveJackTo(Vector3 target)
        {
            while (jack.localPosition != target)
            {
                jack.localPosition = Vector3.MoveTowards(jack.localPosition, target, springSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void SetJackDangerous(bool dangerous)
        {
            if (_jackCollider != null)
            {
                _jackCollider.enabled = dangerous;
            }
        }
    }
}
