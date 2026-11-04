using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.UI
{
    public class IntroScreenController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "ConeTest";
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        public void Play()
        {
            SceneManager.LoadScene(nextSceneName);
        }

        public void OpenOptions()
        {
            mainPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }

        public void CloseOptions()
        {
            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }

        public void Quit()
        {
            Debug.Log("Quit");
            Application.Quit();
        }
    }
}
