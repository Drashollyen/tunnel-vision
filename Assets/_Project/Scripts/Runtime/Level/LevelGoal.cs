using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Level
{
    /// <summary>
    /// The end of a level. Unlocks the next level, then shows the HUD's Level Complete screen,
    /// or goes straight back to the wheel if the scene has no such screen. Only fires once.
    /// </summary>
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("This level's position on the wheel, counting from 0.")]
        [SerializeField] private int levelIndex;
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [Tooltip("The scene Next Level loads. Leave empty on the last level to offer only Level Select.")]
        [SerializeField] private string nextLevelScene;
        [Tooltip("Tick on the demo's last level: Level Complete then offers only Continue, to the ending.")]
        [SerializeField] private bool endsDemo;
        [SerializeField] private string demoEndScene = "DemoComplete";

        private bool _reached;

        public bool Reached => _reached;

        /// <summary>Where along the level the player crosses into the goal: the left edge of its trigger.</summary>
        public float CrossingX
        {
            get
            {
                var trigger = GetComponent<Collider2D>();
                return trigger != null ? trigger.bounds.min.x : transform.position.x;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_reached || other.GetComponentInParent<PlayerRespawn>() == null)
            {
                return;
            }

            Reach();
        }

        /// <summary>What touching the goal does. Public so tests can call it.</summary>
        public void Reach()
        {
            if (_reached)
            {
                return;
            }

            _reached = true;
            LevelProgress.CompleteLevel(levelIndex);

            // Teddies found this run are only kept now the level is cleared.
            var collectibles = FindAnyObjectByType<PlayerCollectibles>();
            var teddies = collectibles != null ? collectibles.Count : 0;
            LevelProgress.RecordCollectibles(levelIndex, teddies);

            // And this playthrough's result, for the demo's ending.
            var lives = FindAnyObjectByType<PlayerLives>();
            var livesLost = lives != null ? lives.LivesLost : 0;
            RunStats.Record(new RunStats.LevelResult(levelIndex, LevelClock.Seconds, teddies, livesLost));

            // And the all-time stats, for the main menu's Stats page.
            AllTimeStats.RecordClear(levelIndex, LevelClock.Seconds, teddies, livesLost, endsDemo);

            var screen = FindAnyObjectByType<LevelCompleteScreen>();
            if (screen != null)
            {
                screen.Show(endsDemo ? demoEndScene : nextLevelScene, endsDemo);
            }
            else
            {
                SceneFader.Load(levelSelectScene);
            }
        }
    }
}
