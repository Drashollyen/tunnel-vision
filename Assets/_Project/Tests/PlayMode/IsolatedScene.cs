using System.Collections;
using System.Collections.Generic;
using TunnelVision.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Gives a test an empty scene of its own. Objects in other open scenes (a whole level an
    /// earlier test loaded, say) are switched off until Close, so FindAnyObjectByType can't find
    /// them. Scenes are never unloaded, and the Test Runner's own objects are left alone, since
    /// stopping those stops the test run. The test scene has its own Audio Listener, so sounds a
    /// test plays don't warn that nothing is listening. Each test also starts with gameplay
    /// sound on, in case an earlier one ended on Out Of Lives or in the pause menu.
    /// </summary>
    public static class IsolatedScene
    {
        private static readonly List<GameObject> SwitchedOff = new List<GameObject>();

        public static Scene Current { get; private set; }

        public static IEnumerator Create(string name)
        {
            Current = SceneManager.CreateScene(name + " " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(Current);
            SoundEffectsVolume.RestoreGameplayNow();

            SwitchedOff.Clear();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene == Current || !scene.isLoaded)
                {
                    continue;
                }

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.activeSelf && !BelongsToTestRunner(root))
                    {
                        root.SetActive(false);
                        SwitchedOff.Add(root);
                    }
                }
            }

            // The camera that normally hears sounds was just switched off with its scene, so the
            // test scene gets its own listener. It goes when the scene is unloaded in Close.
            new GameObject("Test Audio Listener", typeof(AudioListener));

            yield return null;
        }

        public static void Close()
        {
            if (Current.IsValid() && Current.isLoaded && SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(Current);
            }

            foreach (var root in SwitchedOff)
            {
                if (root != null)
                {
                    root.SetActive(true);
                }
            }

            SwitchedOff.Clear();
        }

        private static bool BelongsToTestRunner(GameObject root)
        {
            foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var space = behaviour != null ? behaviour.GetType().Namespace : null;
                if (space != null && (space.StartsWith("UnityEngine.TestTools") || space.StartsWith("UnityEngine.TestRunner")))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
