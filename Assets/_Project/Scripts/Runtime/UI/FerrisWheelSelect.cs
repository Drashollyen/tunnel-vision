using TunnelVision.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// The level select wheel. Left and right arrows turn it, the top cabin is selected,
    /// Enter starts that level if it's unlocked, and Escape goes back to the menu.
    /// Goes on the Wheel object. Cabins are listed clockwise, starting from the top.
    /// </summary>
    public class FerrisWheelSelect : MonoBehaviour
    {
        [SerializeField] private RectTransform[] cabins;
        [SerializeField] private float turnSpeed = 240f;
        [SerializeField] private string[] levelScenes;
        [SerializeField] private string menuScene = "IntroScreen";
        [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.35f);

        private int _steps;
        private float _angle;

        private float StepAngle => 360f / cabins.Length;

        public int SelectedIndex
        {
            get
            {
                var remainder = _steps % cabins.Length;
                return remainder < 0 ? remainder + cabins.Length : remainder;
            }
        }

        private void OnValidate()
        {
            if (cabins != null && levelScenes != null && levelScenes.Length != cabins.Length)
            {
                Debug.LogWarning("FerrisWheelSelect: Level Scenes needs one entry per cabin.", this);
            }
        }

        private void Start()
        {
            _steps = Mathf.Min(LevelProgress.HighestUnlocked, cabins.Length - 1);
            _angle = _steps * StepAngle;
            ApplyRotation();

            for (var i = 0; i < cabins.Length; i++)
            {
                if (!LevelProgress.IsUnlocked(i))
                {
                    cabins[i].GetComponentInChildren<Image>().color = lockedColor;
                }
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    SceneManager.LoadScene(menuScene);
                    return;
                }

                if (keyboard.rightArrowKey.wasPressedThisFrame)
                {
                    _steps++;
                }
                else if (keyboard.leftArrowKey.wasPressedThisFrame)
                {
                    _steps--;
                }
            }

            var targetAngle = _steps * StepAngle;
            _angle = Mathf.MoveTowards(_angle, targetAngle, turnSpeed * Time.unscaledDeltaTime);

            if (keyboard != null && _angle == targetAngle
                && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                StartSelectedLevel();
            }

            ApplyRotation();
        }

        private void ApplyRotation()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, _angle);
            foreach (var cabin in cabins)
            {
                cabin.localRotation = Quaternion.Euler(0f, 0f, -_angle);
            }
        }

        private void StartSelectedLevel()
        {
            if (!LevelProgress.IsUnlocked(SelectedIndex))
            {
                return;
            }

            var scene = levelScenes[SelectedIndex];
            if (string.IsNullOrEmpty(scene))
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogWarning($"FerrisWheelSelect: {scene} isn't in the build's scene list.");
                return;
            }

            SceneManager.LoadScene(scene);
        }
    }
}
