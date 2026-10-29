using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Drives the main menu: Play, Options and Quit, and switching between the main and options panels.
    /// Button OnClick events in the MainMenu scene call the public methods below.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [Tooltip("Scene loaded by Play. It must be listed in File > Build Profiles > Scene List.")]
        [SerializeField] private string gameSceneName = "Game";

        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        [Tooltip("Selected when the main panel opens, so keyboard and gamepad navigation work.")]
        [SerializeField] private Selectable mainFirstSelected;

        [Tooltip("Selected when the options panel opens.")]
        [SerializeField] private Selectable optionsFirstSelected;

        public string GameSceneName => gameSceneName;

        private void Start()
        {
            ShowMain();
        }

        public void Play()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenOptions()
        {
            mainPanel.SetActive(false);
            optionsPanel.SetActive(true);
            Select(optionsFirstSelected);
        }

        public void CloseOptions()
        {
            ShowMain();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMain()
        {
            optionsPanel.SetActive(false);
            mainPanel.SetActive(true);
            Select(mainFirstSelected);
        }

        private static void Select(Selectable selectable)
        {
            if (selectable != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
            }
        }
    }
}
