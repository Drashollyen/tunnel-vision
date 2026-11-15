using TMPro;
using TunnelVision.Audio;
using TunnelVision.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Shown when the player reaches a level's goal. Nicholas walks on by himself (stopping at the
    /// platform's edge) while the screen fades to black, the game freezes once it's black,
    /// then the panel fades in on the black: "Level Complete" with the time taken and the
    /// collectibles found, a Next Level button (when there is a next level) and Level Select.
    /// Game sounds and music fade out as it starts. Used with the mouse,
    /// like the other menus. Goes on an always-on object; the overlay is a child that starts hidden.
    /// </summary>
    public class LevelCompleteScreen : MonoBehaviour
    {
        public const string Title = "Level Complete";
        /// <summary>How long the screen takes to go black, the same as the death sequence's fade.</summary>
        public const float BlackFadeSeconds = 0.75f;
        /// <summary>How long the panel then takes to appear on the black.</summary>
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

        /// <summary>True while any level complete screen is up, so nothing else (the pause menu) opens over it.</summary>
        public static bool IsAnyShowing { get; private set; }

        public bool IsShowing { get; private set; }

        /// <summary>How black the screen is behind the panel, from 0 to 1.</summary>
        public float Blackness => _backdrop != null ? _backdrop.color.a : 0f;

        /// <summary>How visible the panel is, from 0 to 1.</summary>
        public float PanelVisibility => _panelGroup != null ? _panelGroup.alpha : (_group != null ? _group.alpha : 0f);

        /// <summary>The backdrop's blackness and the panel's visibility this long after crossing the goal.</summary>
        public static (float black, float panel) FadeAt(float elapsed) => (
            Mathf.Clamp01(elapsed / BlackFadeSeconds),
            Mathf.Clamp01((elapsed - BlackFadeSeconds) / FadeInSeconds));

        /// <summary>The scene Next Level loads, or null when there isn't one.</summary>
        public string NextScene => _nextScene;

        /// <summary>A time in seconds as minutes and seconds, such as "1:05".</summary>
        public static string FormatTime(float seconds)
        {
            var whole = Mathf.Max(0, Mathf.FloorToInt(seconds));
            return $"{whole / 60}:{whole % 60:00}";
        }

        /// <summary>The two lines under the title.</summary>
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
                // Never leave the game frozen if this goes away some other way.
                Time.timeScale = 1f;
                IsAnyShowing = false;
            }
        }

        /// <summary>
        /// Shows the screen. <paramref name="nextScene"/> is the level Next Level loads; leave it
        /// empty, or name a scene that isn't in the build, and only Level Select is offered.
        /// </summary>
        public void Show(string nextScene)
        {
            if (IsShowing)
            {
                return;
            }

            IsShowing = true;
            IsAnyShowing = true;
            _nextScene = !string.IsNullOrEmpty(nextScene) && Application.CanStreamedLevelBeLoaded(nextScene)
                ? nextScene
                : null;

            // The time taken is game time, so pauses and menus don't count. Read it before freezing.
            var seconds = Time.timeSinceLevelLoad;
            var collectibles = FindAnyObjectByType<PlayerCollectibles>();

            // Walk on through the fade instead of stopping dead; freeze once the screen is black.
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

            overlay.SetActive(true);
            _shownAt = Time.unscaledTime;
            Draw(0f);

            // Mouse-only, like the other menus: nothing selected, so nothing stays highlighted.
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
            _panelGroup.blocksRaycasts = shown > 0f; // No clicking buttons that can't be seen yet.
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
            // Stays frozen through the fade, so the level doesn't come back to life behind it.
            // SceneFader clears the freeze once the next scene has loaded.
            SceneFader.Load(scene);
        }
    }
}
