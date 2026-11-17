using UnityEngine;

namespace TunnelVision.UI
{
    // The main menu. The public methods are hooked up to the buttons in the IntroScreen scene.
    public class IntroScreenController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "LevelSelect";
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private GameObject statsPanel;

        private PanelSwitcher _panels;

        private void Awake()
        {
            _panels = gameObject.AddComponent<PanelSwitcher>();

            // The options page is shared with the pause menu, so it tells us when Back is pressed.
            if (optionsPanel != null && optionsPanel.TryGetComponent(out OptionsMenu options))
            {
                options.BackPressed += CloseOptions;
            }
        }

        public void Play()
        {
            // Starts a new playthrough, so the ending only counts levels cleared from now on.
            Level.RunStats.Reset();

            SceneFader.Load(nextSceneName);
        }

        public void OpenOptions()
        {
            _panels.Switch(mainPanel, optionsPanel);
        }

        public void CloseOptions()
        {
            _panels.Switch(optionsPanel, mainPanel);
        }

        public void OpenInfo()
        {
            _panels.Switch(mainPanel, infoPanel);
        }

        public void CloseInfo()
        {
            _panels.Switch(infoPanel, mainPanel);
        }

        public void OpenStats()
        {
            _panels.Switch(mainPanel, statsPanel);
        }

        public void CloseStats()
        {
            _panels.Switch(statsPanel, mainPanel);
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
