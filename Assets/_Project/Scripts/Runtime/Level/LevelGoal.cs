using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Level
{
    // The end of a level. Saves progress and stats, then shows the Level Complete screen
    // (or goes straight back to Level Select if the scene doesn't have one).
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("This level's position on the wheel, counting from 0.")]
        [SerializeField] private int levelIndex;
        [SerializeField] private string levelSelectScene = "LevelSelect";
        [Tooltip("The scene Next Level loads. Leave empty on the last level to offer only Level Select.")]
        [SerializeField] private string nextLevelScene;
        [Tooltip("Tick on the demo's last level: Level Complete then only offers Continue, to the ending.")]
        [SerializeField] private bool endsDemo;
        [SerializeField] private string demoEndScene = "DemoComplete";

        private bool _reached;

        public bool Reached => _reached;

        // The left edge of the goal's trigger, where the player crosses into it.
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

        // Public so tests can call it.
        public void Reach()
        {
            if (_reached)
            {
                return;
            }

            _reached = true;
            LevelProgress.CompleteLevel(levelIndex);

            // Teddies are only saved once the level is cleared.
            var collectibles = FindAnyObjectByType<PlayerCollectibles>();
            var teddies = collectibles != null ? collectibles.Count : 0;
            LevelProgress.RecordCollectibles(levelIndex, teddies);

            // This playthrough's result, shown on the demo's ending screen.
            var lives = FindAnyObjectByType<PlayerLives>();
            var livesLost = lives != null ? lives.LivesLost : 0;
            RunStats.Record(new RunStats.LevelResult(levelIndex, Time.timeSinceLevelLoad, teddies, livesLost));

            // The all-time stats, for the main menu's Stats page.
            AllTimeStats.RecordClear(levelIndex, Time.timeSinceLevelLoad, teddies, livesLost, endsDemo);

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
