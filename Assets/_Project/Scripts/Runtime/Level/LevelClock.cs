using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Level
{
    /// <summary>
    /// The round time: seconds since the level loaded, or since the player last restarted it from
    /// a checkpoint. Loading a level always starts it from zero.
    /// </summary>
    public static class LevelClock
    {
        private static Scene _resetScene;
        private static float _resetAt;

        /// <summary>Seconds of the current round.</summary>
        public static float Seconds
        {
            get
            {
                var offset = SceneManager.GetActiveScene() == _resetScene ? _resetAt : 0f;
                return Mathf.Max(0f, Time.timeSinceLevelLoad - offset);
            }
        }

        /// <summary>Starts the round time again from zero, in this level only.</summary>
        public static void Restart()
        {
            _resetScene = SceneManager.GetActiveScene();
            _resetAt = Time.timeSinceLevelLoad;
        }
    }
}
