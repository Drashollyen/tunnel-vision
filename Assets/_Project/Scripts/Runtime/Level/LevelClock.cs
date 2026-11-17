using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Level
{
    /// <summary>
    /// The round time: seconds since the level loaded, not counting time it was held (a restart
    /// at the checkpoint holds it while the screen is black). Loading a level, including
    /// restarting it from the start, always begins it from zero.
    /// </summary>
    public static class LevelClock
    {
        private static Scene _scene;
        private static float _heldTotal;
        private static float _heldSince = -1f;

        /// <summary>Seconds of the current round.</summary>
        public static float Seconds
        {
            get
            {
                ForgetOtherLevels();
                var now = Time.timeSinceLevelLoad;
                var held = _heldTotal + (_heldSince >= 0f ? now - _heldSince : 0f);
                return Mathf.Max(0f, now - held);
            }
        }

        /// <summary>True while the round time is stopped.</summary>
        public static bool IsHeld
        {
            get
            {
                ForgetOtherLevels();
                return _heldSince >= 0f;
            }
        }

        /// <summary>Stops the round time until <see cref="Release"/>.</summary>
        public static void Hold()
        {
            ForgetOtherLevels();
            if (_heldSince < 0f)
            {
                _heldSince = Time.timeSinceLevelLoad;
            }
        }

        /// <summary>Lets the round time carry on from where <see cref="Hold"/> stopped it.</summary>
        public static void Release()
        {
            ForgetOtherLevels();
            if (_heldSince >= 0f)
            {
                _heldTotal += Time.timeSinceLevelLoad - _heldSince;
                _heldSince = -1f;
            }
        }

        // Holds belong to one loaded level; a new or reloaded level starts clean.
        private static void ForgetOtherLevels()
        {
            var active = SceneManager.GetActiveScene();
            if (active != _scene)
            {
                _scene = active;
                _heldTotal = 0f;
                _heldSince = -1f;
            }
        }
    }
}
