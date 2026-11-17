using TMPro;
using TunnelVision.Level;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // The level select Ferris wheel. Left and right arrows turn it, the top cabin is the selected
    // one, Enter starts that level if it's unlocked, and Escape goes back to the menu. With the
    // mouse, clicking a cabin turns it to the top, and clicking the top cabin acts like Enter.
    // Cabins past the prototype's levels show "???" and open a notice instead.
    // Goes on the Wheel object. List the cabins clockwise, starting from the top.
    public class FerrisWheelSelect : MonoBehaviour
    {
        public enum CabinState
        {
            Playable,
            Locked,
            NotInPrototype,
        }

        public const string NotInPrototypeMessage = "This Level Is Not Available In The Prototype";
        private const string UnknownLevelName = "???";

        [SerializeField] private RectTransform[] cabins;
        [Tooltip("How many cabins, from the first, have levels in this prototype. The rest show \"???\".")]
        [SerializeField] private int prototypeLevelCount = 3;
        [Tooltip("Optional. Shown when a \"???\" cabin is picked.")]
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

        [Header("Sounds")]
        [Tooltip("Each turn of one cabin.")]
        [SerializeField] private AudioClip turnSound;
        [Tooltip("Enter on a playable cabin, as the level starts loading.")]
        [SerializeField] private AudioClip startSound;
        [Tooltip("Enter on a cabin the player hasn't unlocked yet.")]
        [SerializeField] private AudioClip lockedSound;
        [Tooltip("Enter on a \"???\" cabin, as the notice opens.")]
        [SerializeField] private AudioClip notInPrototypeSound;
        [Tooltip("Escape, going back to the main menu.")]
        [SerializeField] private AudioClip backSound;

        private const float SelectedCabinScale = 1.15f;
        private const float CabinEaseSpeed = 12f;
        private const float ArrowBobHeight = 10f;
        private const float ArrowBobSpeed = 4f;

        // The menu plays its greyed out click at this volume, so the wheel matches it.
        private const float ErrorVolume = 0.6f;

        // A locked cabin rocks on its hinge for a moment when picked.
        public const float WobbleSeconds = 0.4f;
        private const float WobbleDegrees = 6f;
        private const float WobbleSwingsPerSecond = 9f;

        private int _steps;
        private float _angle;
        private Vector2 _arrowRestPosition;
        private int _labelledIndex = -1;
        private int _wobbleIndex = -1;
        private float _wobbleStartedAt;

        // The cabin that's rocking because it's locked, or -1 when none is.
        public int WobblingCabin => _wobbleIndex;

        private float StepAngle => 360f / cabins.Length;

        public int SelectedIndex
        {
            get
            {
                var remainder = _steps % cabins.Length;
                return remainder < 0 ? remainder + cabins.Length : remainder;
            }
        }

        public static CabinState GetCabinState(int index, int prototypeLevelCount, int highestUnlocked)
        {
            if (index >= prototypeLevelCount)
            {
                return CabinState.NotInPrototype;
            }

            return index <= highestUnlocked ? CabinState.Playable : CabinState.Locked;
        }

        // The text on the plate under the wheel.
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

        // Where the wheel should turn to after an arrow press. It can only get one cabin ahead of
        // where it is now, so mashing the arrow keys can't queue up a long spin.
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

            // While the notice is open, or on the frame it closed, the keys belong to the notice,
            // so the Enter that closes it doesn't also start a level.
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
                    InterfaceSounds.Play(backSound);
                    SceneFader.Load(menuScene);
                    return;
                }

                var direction = keyboard.rightArrowKey.wasPressedThisFrame ? 1
                    : keyboard.leftArrowKey.wasPressedThisFrame ? -1 : 0;
                if (direction != 0)
                {
                    Turn(direction);
                }
            }

            var clicked = ClickedCabin();
            if (clicked >= 0 && clicked != SelectedIndex)
            {
                TurnTo(clicked);
                clicked = -1;
            }

            var targetAngle = _steps * StepAngle;
            _angle = Mathf.MoveTowards(_angle, targetAngle, turnSpeed * Time.unscaledDeltaTime);

            var enterPressed = keyboard != null
                && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame);
            if (_angle == targetAngle && (enterPressed || clicked == SelectedIndex))
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

        // How far a locked cabin is tipped, in degrees, this long after it was picked: a quick
        // swing back and forth that dies away, then 0.
        public static float WobbleAngle(float elapsed)
        {
            if (elapsed < 0f || elapsed >= WobbleSeconds)
            {
                return 0f;
            }

            var fade = 1f - elapsed / WobbleSeconds;
            return WobbleDegrees * fade * Mathf.Sin(elapsed * WobbleSwingsPerSecond * 2f * Mathf.PI);
        }

        private void ApplyRotation()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, _angle);

            var wobble = 0f;
            if (_wobbleIndex >= 0)
            {
                var elapsed = Time.unscaledTime - _wobbleStartedAt;
                wobble = WobbleAngle(elapsed);
                if (elapsed >= WobbleSeconds)
                {
                    _wobbleIndex = -1;
                }
            }

            // Cabins turn the opposite way to the wheel, so they always hang upright.
            for (var i = 0; i < cabins.Length; i++)
            {
                var tip = i == _wobbleIndex ? wobble : 0f;
                cabins[i].localRotation = Quaternion.Euler(0f, 0f, -_angle + tip);
            }
        }

        // What an arrow key does: direction is -1 for left or 1 for right. It only clicks when
        // the wheel actually takes another step.
        public void Turn(int direction)
        {
            var next = NextStep(_steps, _angle, StepAngle, direction);
            if (next != _steps)
            {
                _steps = next;
                InterfaceSounds.Play(turnSound);
            }
        }

        /// <summary>The cabin under the mouse when the left button was pressed this frame, or -1.</summary>
        private int ClickedCabin()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return -1;
            }

            var point = mouse.position.ReadValue();
            for (var i = 0; i < cabins.Length; i++)
            {
                // The Body is the visible cabin; the cabin itself is only a small hinge.
                var body = cabins[i].Find("Body") as RectTransform;
                var target = body != null ? body : cabins[i];
                var canvas = target.GetComponentInParent<Canvas>();
                var view = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
                if (RectTransformUtility.RectangleContainsScreenPoint(target, point, view))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// The step change that brings cabin <paramref name="index"/> to the top the short way round,
        /// from cabin <paramref name="selected"/> on a wheel of <paramref name="count"/> cabins.
        /// </summary>
        public static int StepsTo(int selected, int index, int count)
        {
            var delta = ((index - selected) % count + count) % count;
            return delta > count / 2 ? delta - count : delta;
        }

        /// <summary>What clicking a cabin does: turns the wheel the short way until that cabin is at the top.</summary>
        public void TurnTo(int index)
        {
            var delta = StepsTo(SelectedIndex, index, cabins.Length);
            if (delta != 0)
            {
                _steps += delta;
                InterfaceSounds.Play(turnSound);
            }
        }

        private void ShowNotInPrototype()
        {
            InterfaceSounds.Play(notInPrototypeSound, ErrorVolume);

            if (prototypeNotice != null)
            {
                prototypeNotice.Show(NotInPrototypeMessage);
            }
            else
            {
                Debug.LogWarning("FerrisWheelSelect: no Prototype Notice set, so the \"???\" cabin does nothing.", this);
            }
        }

        // What Enter does: starts the level, shows the "???" notice, or wobbles the cabin if it's locked.
        public void StartSelectedLevel()
        {
            var state = StateOf(SelectedIndex);

            if (state == CabinState.NotInPrototype)
            {
                ShowNotInPrototype();
                return;
            }

            if (state == CabinState.Locked)
            {
                InterfaceSounds.Play(lockedSound, ErrorVolume);
                _wobbleIndex = SelectedIndex;
                _wobbleStartedAt = Time.unscaledTime;
                return;
            }

            // Unlocked, but its level hasn't been made yet: treated like a "???" cabin.
            var scene = levelScenes != null && SelectedIndex < levelScenes.Length ? levelScenes[SelectedIndex] : null;
            if (string.IsNullOrEmpty(scene))
            {
                ShowNotInPrototype();
                return;
            }

            InterfaceSounds.Play(startSound);
            SceneFader.Load(scene);
        }
    }
}
