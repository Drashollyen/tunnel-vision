using TMPro;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// The level select wheel. Left and right arrows turn it, the top cabin is selected,
    /// Enter starts that level if it's unlocked, and Escape goes back to the menu.
    /// Cabins past the prototype's levels show "?" and open a notice instead.
    /// Goes on the Wheel object. Cabins are listed clockwise, starting from the top.
    /// </summary>
    public class FerrisWheelSelect : MonoBehaviour
    {
        public enum CabinState
        {
            Playable,
            Locked,
            NotInPrototype,
        }

        public const string NotInPrototypeMessage = "This Level Is Not Available In The Prototype";
        private const string UnknownLevelName = "?";

        [SerializeField] private RectTransform[] cabins;
        [Tooltip("How many cabins, from the first, have levels in this prototype. The rest show \"?\".")]
        [SerializeField] private int prototypeLevelCount = 3;
        [Tooltip("Optional. Shown when a \"?\" cabin is picked.")]
        [SerializeField] private NoticeDialog prototypeNotice;
        [SerializeField] private float turnSpeed = 240f;
        [SerializeField] private string[] levelScenes;
        [SerializeField] private string menuScene = "IntroScreen";
        [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.35f);
        [SerializeField] private string[] levelNames;
        [SerializeField] private TMP_Text selectedLabel;
        [SerializeField] private RectTransform selectArrow;
        [Tooltip("The soft outline behind each cabin, in the same order as Cabins.")]
        [SerializeField] private Image[] cabinOutlines;

        private const float SelectedCabinScale = 1.15f;
        private const float CabinEaseSpeed = 12f;
        private const float ArrowBobHeight = 10f;
        private const float ArrowBobSpeed = 4f;

        private int _steps;
        private float _angle;
        private Vector2 _arrowRestPosition;
        private int _labelledIndex = -1;

        private float StepAngle => 360f / cabins.Length;

        public int SelectedIndex
        {
            get
            {
                var remainder = _steps % cabins.Length;
                return remainder < 0 ? remainder + cabins.Length : remainder;
            }
        }

        /// <summary>Whether a cabin can be played, is locked by progress, or has no level in the prototype.</summary>
        public static CabinState GetCabinState(int index, int prototypeLevelCount, int highestUnlocked)
        {
            if (index >= prototypeLevelCount)
            {
                return CabinState.NotInPrototype;
            }

            return index <= highestUnlocked ? CabinState.Playable : CabinState.Locked;
        }

        /// <summary>The text on the plate under the wheel for a cabin.</summary>
        public static string PlateText(int index, CabinState state, string levelName)
        {
            switch (state)
            {
                case CabinState.Playable:
                    return $"Level {index + 1}: {levelName}";
                case CabinState.NotInPrototype:
                    return $"Level {index + 1}: {UnknownLevelName}";
                default:
                    return "Locked";
            }
        }

        public CabinState StateOf(int index) =>
            GetCabinState(index, prototypeLevelCount, LevelProgress.HighestUnlocked);

        /// <summary>
        /// The new target step after a press. Allows at most one step beyond the cabin
        /// the wheel is nearest to now, so spamming can't queue up a long spin.
        /// </summary>
        public static int NextStep(int steps, float angle, float stepAngle, int direction)
        {
            var showing = Mathf.RoundToInt(angle / stepAngle);
            return Mathf.Clamp(steps + direction, showing - 1, showing + 1);
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
            _steps = Mathf.Min(LevelProgress.HighestUnlocked, prototypeLevelCount - 1, cabins.Length - 1);
            _angle = _steps * StepAngle;
            ApplyRotation();

            if (selectArrow != null)
            {
                _arrowRestPosition = selectArrow.anchoredPosition;
            }

            for (var i = 0; i < cabins.Length; i++)
            {
                var state = StateOf(i);
                if (state != CabinState.Playable)
                {
                    var body = cabins[i].Find("Body");
                    if (body != null)
                    {
                        body.GetComponent<Image>().color = lockedColor;
                    }
                }

                if (state == CabinState.NotInPrototype)
                {
                    var label = cabins[i].Find("Label");
                    if (label != null)
                    {
                        label.GetComponent<TMP_Text>().text = UnknownLevelName;
                    }
                }
            }
        }

        private void Update()
        {
            if (SceneFader.IsFading)
            {
                return;
            }

            // While the notice is up, or on the frame it closed, keys belong to the notice.
            if (prototypeNotice != null
                && (prototypeNotice.IsOpen || prototypeNotice.ClosedOnFrame == Time.frameCount))
            {
                return;
            }

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                {
                    SceneFader.Load(menuScene);
                    return;
                }

                var direction = keyboard.rightArrowKey.wasPressedThisFrame ? 1
                    : keyboard.leftArrowKey.wasPressedThisFrame ? -1 : 0;
                if (direction != 0)
                {
                    _steps = NextStep(_steps, _angle, StepAngle, direction);
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
            ShowSelection();
        }

        private void ShowSelection()
        {
            var blend = 1f - Mathf.Exp(-CabinEaseSpeed * Time.unscaledDeltaTime);
            for (var i = 0; i < cabins.Length; i++)
            {
                var scale = i == SelectedIndex ? SelectedCabinScale : 1f;
                cabins[i].localScale = Vector3.Lerp(cabins[i].localScale, Vector3.one * scale, blend);

                if (cabinOutlines != null && i < cabinOutlines.Length)
                {
                    var outlineColor = cabinOutlines[i].color;
                    outlineColor.a = Mathf.Lerp(outlineColor.a, i == SelectedIndex ? 1f : 0f, blend);
                    cabinOutlines[i].color = outlineColor;
                }
            }

            if (selectArrow != null)
            {
                var bob = Mathf.Sin(Time.unscaledTime * ArrowBobSpeed) * ArrowBobHeight;
                selectArrow.anchoredPosition = _arrowRestPosition + Vector2.up * bob;
            }

            if (selectedLabel != null && _labelledIndex != SelectedIndex)
            {
                _labelledIndex = SelectedIndex;
                var levelName = levelNames != null && SelectedIndex < levelNames.Length
                    ? levelNames[SelectedIndex]
                    : string.Empty;
                selectedLabel.text = PlateText(SelectedIndex, StateOf(SelectedIndex), levelName);
            }
        }

        private void ApplyRotation()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, _angle);
            foreach (var cabin in cabins)
            {
                cabin.localRotation = Quaternion.Euler(0f, 0f, -_angle);
            }
        }

        /// <summary>What Enter does: starts the level, shows the prototype notice, or nothing if locked.</summary>
        public void StartSelectedLevel()
        {
            var state = StateOf(SelectedIndex);

            if (state == CabinState.NotInPrototype)
            {
                if (prototypeNotice != null)
                {
                    prototypeNotice.Show(NotInPrototypeMessage);
                }
                else
                {
                    Debug.LogWarning("FerrisWheelSelect: no Prototype Notice set, so the \"?\" cabin does nothing.", this);
                }

                return;
            }

            if (state != CabinState.Playable || levelScenes == null || SelectedIndex >= levelScenes.Length)
            {
                return;
            }

            var scene = levelScenes[SelectedIndex];
            if (string.IsNullOrEmpty(scene))
            {
                return;
            }

            SceneFader.Load(scene);
        }
    }
}
