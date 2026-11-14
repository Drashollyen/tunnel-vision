using TMPro;
using TunnelVision.Player;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows how many collectibles the player has. Goes in the HUD with a text for the number
    /// (and an icon beside it if wanted). If the Player has no PlayerCollectibles yet, one is
    /// added, so the counter works before any pickups exist.
    /// </summary>
    public class CollectibleCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text countText;

        private PlayerCollectibles _collectibles;

        private void Awake()
        {
            _collectibles = FindAnyObjectByType<PlayerCollectibles>();

            if (_collectibles == null)
            {
                var player = FindAnyObjectByType<PlayerLives>();
                if (player != null)
                {
                    _collectibles = player.gameObject.AddComponent<PlayerCollectibles>();
                }
            }

            if (_collectibles == null)
            {
                Debug.LogError("CollectibleCounter: no Player found in the scene. Add the Player prefab.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_collectibles == null)
            {
                return;
            }

            _collectibles.CountChanged += ShowCount;
            ShowCount(_collectibles.Count); // Catches up if the count changed while the HUD was off.
        }

        private void OnDisable()
        {
            if (_collectibles != null)
            {
                _collectibles.CountChanged -= ShowCount;
            }
        }

        private void ShowCount(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
            }
        }
    }
}
