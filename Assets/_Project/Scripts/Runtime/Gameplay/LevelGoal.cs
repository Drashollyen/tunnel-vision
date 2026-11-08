using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Gameplay
{
    /// <summary>The end of a level. Unlocks the next level and returns to the wheel.</summary>
    public class LevelGoal : MonoBehaviour
    {
        [Tooltip("This level's position on the wheel, counting from 0.")]
        [SerializeField] private int levelIndex;
        [SerializeField] private string levelSelectScene = "LevelSelect";

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerRespawn>() == null)
            {
                return;
            }

            LevelProgress.CompleteLevel(levelIndex);
            SceneManager.LoadScene(levelSelectScene);
        }
    }
}
