using TMPro;
using TunnelVision.Gameplay;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shows "Round Over" with the lives left each time the player loses a life,
    /// and "Game Over" when the last one goes.
    /// </summary>
    public class RoundOverMessage : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text subtitle;
        [SerializeField] private float showSeconds = 2f;

        private PlayerLives playerLives;
        private int lastLives;
        private float hideAt = float.PositiveInfinity;

        private void Awake()
        {
            panel.SetActive(false);
            playerLives = FindAnyObjectByType<PlayerLives>();

            if (playerLives == null)
            {
                Debug.LogError("RoundOverMessage: no PlayerLives found in the scene. Add the Player prefab.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (playerLives == null)
            {
                return;
            }

            playerLives.LivesChanged += OnLivesChanged;
            lastLives = playerLives.CurrentLives;
        }

        private void OnDisable()
        {
            if (playerLives != null)
            {
                playerLives.LivesChanged -= OnLivesChanged;
            }
        }

        private void Start()
        {
            // The player's Awake may run after this message's OnEnable, so read the lives again here.
            lastLives = playerLives.CurrentLives;
        }

        private void Update()
        {
            // Unscaled time, so the message still goes away if something pauses the game.
            if (Time.unscaledTime >= hideAt)
            {
                Hide();
            }
        }

        private void OnLivesChanged(int lives)
        {
            bool lostALife = lives < lastLives;
            lastLives = lives;

            if (!lostALife)
            {
                Hide();
                return;
            }

            panel.SetActive(true);

            if (lives <= 0)
            {
                title.text = "Game Over";
                subtitle.gameObject.SetActive(false);
                hideAt = float.PositiveInfinity;
                return;
            }

            title.text = "Round Over";
            subtitle.text = lives == 1 ? "1 Life Remaining" : $"{lives} Lives Remaining";
            subtitle.gameObject.SetActive(true);
            hideAt = Time.unscaledTime + showSeconds;
        }

        private void Hide()
        {
            panel.SetActive(false);
            hideAt = float.PositiveInfinity;
        }
    }
}
