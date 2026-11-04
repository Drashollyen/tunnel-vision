using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    public class LivesDisplay : MonoBehaviour
    {
        [SerializeField] private PlayerLives playerLives;
        [SerializeField] private Image[] eyes;
        [SerializeField] private GameObject youDiedText;
        [SerializeField] private Color aliveColor = Color.white;
        [SerializeField] private Color lostColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);

        private void OnEnable()
        {
            playerLives.LivesChanged += ShowLives;
        }

        private void OnDisable()
        {
            playerLives.LivesChanged -= ShowLives;
        }

        private void Start()
        {
            ShowLives(playerLives.CurrentLives);
        }

        private void ShowLives(int lives)
        {
            for (int i = 0; i < eyes.Length; i++)
            {
                eyes[i].color = i < lives ? aliveColor : lostColor;
            }

            youDiedText.SetActive(lives <= 0);
        }
    }
}
