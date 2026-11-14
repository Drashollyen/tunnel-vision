using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Gives a test an empty scene of its own and closes every other scene, so nothing an
    /// earlier test loaded (a whole level, say) can be found by FindAnyObjectByType.
    /// </summary>
    public static class IsolatedScene
    {
        public static Scene Current { get; private set; }

        public static IEnumerator Create(string name)
        {
            Current = SceneManager.CreateScene(name + " " + System.Guid.NewGuid());
            SceneManager.SetActiveScene(Current);

            var others = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene != Current && scene.isLoaded)
                {
                    others.Add(scene);
                }
            }

            foreach (var scene in others)
            {
                yield return SceneManager.UnloadSceneAsync(scene);
            }
        }

        public static void Close()
        {
            if (Current.IsValid() && Current.isLoaded && SceneManager.sceneCount > 1)
            {
                SceneManager.UnloadSceneAsync(Current);
            }
        }
    }
}
