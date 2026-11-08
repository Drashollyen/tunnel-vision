using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    public class LivesDisplay : MonoBehaviour
    {
        [SerializeField] private Image[] eyes;
        [SerializeField] private Color aliveColor = Color.white;
        [SerializeField] private Color lostColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);

        private PlayerLives _playerLives;

        private void Awake()
        {
            _playerLives = FindAnyObjectByType<PlayerLives>();

            if (_playerLives == null)
            {
                Debug.LogError("LivesDisplay: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_playerLives == null)
            {
                return;
            }

            _playerLives.LivesChanged += ShowLives;
            ShowLives(_playerLives.CurrentLives); // Catches up if lives changed while the HUD was off.
        }

        private void OnDisable()
        {
            if (_playerLives != null)
            {
                _playerLives.LivesChanged -= ShowLives;
            }
        }

        private void Start()
        {
            if (eyes.Length != _playerLives.MaxLives)
            {
                Debug.LogWarning($"LivesDisplay: has {eyes.Length} eyes but the player has {_playerLives.MaxLives} max lives.");
            }

            // The player's Awake may run after this display's OnEnable, so show the lives again here.
            ShowLives(_playerLives.CurrentLives);
        }

        private void ShowLives(int lives)
        {
            for (int i = 0; i < eyes.Length; i++)
            {
                eyes[i].color = i < lives ? aliveColor : lostColor;
            }
        }
    }
}
