using UnityEngine;

namespace TunnelVision.UI
{
    public class IntroScreenController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "LevelSelect";
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject infoPanel;

        private PanelSwitcher _panels;

        private void Awake()
        {
            _panels = gameObject.AddComponent<PanelSwitcher>();

            // The options page is a shared prefab, so its Back button reports back to us.
            if (optionsPanel != null && optionsPanel.TryGetComponent(out OptionsMenu options))
            {
                options.BackPressed += CloseOptions;
            }
        }

        public void Play()
        {
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

        public void Quit()
        {
            Application.Quit();
        }
    }
}
