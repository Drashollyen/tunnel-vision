using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Runs the intro screen's panels. Each panel selects a control when it opens, so the
    /// arrow keys and Enter work straight away, and a key press brings the selection back
    /// if a mouse click cleared it.
    /// </summary>
    public class IntroScreenController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "LevelSelect";
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject infoPanel;

        [Header("Keyboard")]
        [Tooltip("Optional. Selected when the panel opens. Empty means the panel's first usable control.")]
        [SerializeField] private Selectable mainFirstSelected;
        [SerializeField] private Selectable optionsFirstSelected;
        [SerializeField] private Selectable infoFirstSelected;

        private PanelSwitcher _panels;
        private GameObject _openPanel;

        private void Awake()
        {
            _panels = gameObject.AddComponent<PanelSwitcher>();
        }

        private void Start()
        {
            SelectFirstIn(mainPanel);
        }

        private void Update()
        {
            var events = EventSystem.current;
            var keyboard = Keyboard.current;
            if (events == null || keyboard == null || _openPanel == null || !keyboard.anyKey.wasPressedThisFrame)
            {
                return;
            }

            // Clicking empty space clears the selection, which leaves the keyboard doing nothing.
            var selected = events.currentSelectedGameObject;
            if (selected == null || !selected.activeInHierarchy)
            {
                SelectFirstIn(_openPanel);
            }
        }

        public void Play()
        {
            SceneFader.Load(nextSceneName);
        }

        public void OpenOptions()
        {
            _panels.Switch(mainPanel, optionsPanel);
            SelectFirstIn(optionsPanel);
        }

        public void CloseOptions()
        {
            _panels.Switch(optionsPanel, mainPanel);
            SelectFirstIn(mainPanel);
        }

        public void OpenInfo()
        {
            _panels.Switch(mainPanel, infoPanel);
            SelectFirstIn(infoPanel);
        }

        public void CloseInfo()
        {
            _panels.Switch(infoPanel, mainPanel);
            SelectFirstIn(mainPanel);
        }

        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>The control a panel selects when it opens: the one set in the Inspector, or its first usable one.</summary>
        public Selectable FirstSelectableIn(GameObject panel)
        {
            var chosen = panel == mainPanel ? mainFirstSelected
                : panel == optionsPanel ? optionsFirstSelected
                : panel == infoPanel ? infoFirstSelected
                : null;

            if (chosen != null && chosen.gameObject.activeInHierarchy && chosen.IsInteractable())
            {
                return chosen;
            }

            return FirstUsable(panel);
        }

        /// <summary>The first active, interactable control under a panel, in Hierarchy order.</summary>
        public static Selectable FirstUsable(GameObject panel)
        {
            if (panel == null)
            {
                return null;
            }

            foreach (var selectable in panel.GetComponentsInChildren<Selectable>())
            {
                if (selectable.IsInteractable())
                {
                    return selectable;
                }
            }

            return null;
        }

        private void SelectFirstIn(GameObject panel)
        {
            _openPanel = panel;
            var target = FirstSelectableIn(panel);
            if (target != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(target.gameObject);
            }
        }
    }
}
