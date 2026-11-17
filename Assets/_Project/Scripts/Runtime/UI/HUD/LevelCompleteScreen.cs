using TMPro;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Shown when the player reaches the end of a level. Nicholas keeps walking while the screen fades
    // to black, then the game freezes and "Level Complete" fades in with the time, teddies and buttons.
    public class LevelCompleteScreen : MonoBehaviour
    {
        public const string Title = "Level Complete";
        public const string NextLevelLabel = "Next Level";
        public const string ContinueLabel = "Continue";
        public const float BlackFadeSeconds = 0.75f; // Same as the fade when the player dies.
        public const float FadeInSeconds = 0.4f;
        public const float SoundsFadeSeconds = 0.5f;
        public const float MusicFadeSeconds = 1f;

        [Tooltip("The full-screen image behind the panel. It fades to solid black.")]
        [SerializeField] private GameObject overlay;
        [Tooltip("The box with the title, stats and buttons. It fades in once the screen is black.")]
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text stats;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [Tooltip("Optional. Played as the screen appears.")]
        [SerializeField] private AudioClip completeSound;

        private CanvasGroup _group;
        private CanvasGroup _panelGroup;
        private PlayerMovement _movement;
        private bool _freezeWhenBlack;
        private Image _backdrop;
        private string _nextScene;
        private float _shownAt = float.PositiveInfinity;

        // Lets other scripts (like the pause menu) know not to open on top of this screen.
        public static bool IsAnyShowing { get; private set; }

        public bool IsShowing { get; private set; }

        // From 0 (clear) to 1 (fully black).
        public float Blackness => _backdrop != null ? _backdrop.color.a : 0f;

        // From 0 (hidden) to 1 (fully shown).
        public float PanelVisibility => _panelGroup != null ? _panelGroup.alpha : (_group != null ? _group.alpha : 0f);

        // How black the screen is and how visible the panel is, this many seconds after the level ends.
        public static (float black, float panel) FadeAt(float elapsed) => (
            Mathf.Clamp01(elapsed / BlackFadeSeconds),
            Mathf.Clamp01((elapsed - BlackFadeSeconds) / FadeInSeconds));

        // True on the last level, where the only button is Continue (to the ending).
        public bool EndsDemo { get; private set; }

        // The scene Next Level loads, or null when there isn't one.
        public string NextScene => _nextScene;

        // Turns seconds into minutes and seconds, such as "1:05".
        public static string FormatTime(float seconds)
        {
            var whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{whole / 60}:{whole % 60:00}";
        }

        public static string StatsText(float seconds, int collectibles) =>
            $"Time: {FormatTime(seconds)}\nCollectibles: {collectibles}";

        private void Awake()
        {
            IsAnyShowing = false;
            _group = overlay.TryGetComponent(out CanvasGroup group) ? group : overlay.AddComponent<CanvasGroup>();
            _backdrop = overlay.GetComponent<Image>();
            if (panel != null)
            {
                _panelGroup = panel.TryGetComponent(out CanvasGroup panelGroup) ? panelGroup : panel.AddComponent<CanvasGroup>();
            }

            overlay.SetActive(false);

            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(GoToNextLevel);
            if (levelSelectButton != null)
                levelSelectButton.onClick.AddListener(GoToLevelSelect);
        }

        private void OnDestroy()
        {
            if (IsShowing)
            {
                // Never leave the game frozen if this screen is removed some other way.
                Time.timeScale = 1f;
                IsAnyShowing = false;
            }
        }

        // nextScene is what the Next Level (or Continue) button loads. If it's empty or not in the
        // build, that button is hidden. endsDemo is true on the last level.
        public void Show(string nextScene, bool endsDemo)
        {
            if (IsShowing)
            {
                return;
            }

            EndsDemo = endsDemo;

            IsShowing = true;
            IsAnyShowing = true;
            _nextScene = !string.IsNullOrEmpty(nextScene) && Application.CanStreamedLevelBeLoaded(nextScene)
                ? nextScene
                : null;

            // Game time, so time spent paused doesn't count. Read it before the game freezes.
            var seconds = Time.timeSinceLevelLoad;
            var collectibles = FindAnyObjectByType<PlayerCollectibles>();

            // Nicholas walks on by himself during the fade instead of stopping dead.
            _movement = FindAnyObjectByType<PlayerMovement>();
            if (_movement != null)
            {
                _movement.EnableControls(false);
                _movement.AutoWalk(1f);
            }

            _freezeWhenBlack = true;

            SoundEffectsVolume.FadeOutGameplay(SoundsFadeSeconds);
            var music = FindAnyObjectByType<LevelMusic>();
            if (music != null)
                music.FadeTo(0f, MusicFadeSeconds);
            InterfaceSounds.Play(completeSound);

            if (title != null)
                title.text = Title;
            if (stats != null)
                stats.text = StatsText(seconds, collectibles != null ? collectibles.Count : 0);
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(_nextScene != null);
            SetNextLabel(EndsDemo ? ContinueLabel : NextLevelLabel);

            // On the last level only Continue shows. If the ending can't load, Level Select
            // stays so the player is never stuck.
            if (levelSelectButton != null)
                levelSelectButton.gameObject.SetActive(!EndsDemo || _nextScene == null);

            overlay.SetActive(true);
            _shownAt = Time.unscaledTime;
            Draw(0f);

            // The menus are mouse-only, so make sure no button starts highlighted.
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        private void Update()
        {
            if (IsShowing && PanelVisibility < 1f)
            {
                Draw(Time.unscaledTime - _shownAt);
            }
        }

        private void Draw(float elapsed)
        {
            var (black, shown) = FadeAt(elapsed);

            if (_freezeWhenBlack && black >= 1f)
            {
                _freezeWhenBlack = false;
                Time.timeScale = 0f;
                if (_movement != null)
                {
                    _movement.AutoWalk(0f);
                }
            }

            if (_panelGroup == null || _backdrop == null)
            {
                // No separate panel: the whole overlay fades in together.
                _group.alpha = shown;
                return;
            }

            _group.alpha = 1f;
            var color = _backdrop.color;
            color.a = black;
            _backdrop.color = color;

            _panelGroup.alpha = shown;
            _panelGroup.blocksRaycasts = shown > 0f; // Buttons can't be clicked until they start to appear.
        }

        private void SetNextLabel(string label)
        {
            if (nextLevelButton == null)
            {
                return;
            }

            var text = nextLevelButton.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private void GoToNextLevel()
        {
            if (_nextScene != null)
            {
                Leave(_nextScene);
            }
        }

        private void GoToLevelSelect() => Leave(levelSelectScene);

        private void Leave(string scene)
        {
            // The game stays frozen during the fade so the level doesn't come back to life behind it.
            // SceneFader unfreezes it once the next scene has loaded.
            SceneFader.Load(scene);
        }
    }
}
