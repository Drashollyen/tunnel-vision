using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.Audio;
using TunnelVision.Level;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>Loads the real Demo Complete scene and checks it's all there and working.</summary>
    public class DemoCompleteSceneTests
    {
        private const string Scene = "Assets/_Project/Scenes/Menus/DemoComplete.unity";

        [UnitySetUp]
        public IEnumerator Load()
        {
            RunStats.Reset();
            RunStats.Record(new RunStats.LevelResult(0, 65f, 3, 0));
            RunStats.Record(new RunStats.LevelResult(1, 90f, 1, 1));
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                Scene, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return new WaitForSecondsRealtime(0.2f);
        }

        [TearDown]
        public void Clean()
        {
            RunStats.Reset();
            if (MusicPlayer.Current != null)
            {
                Object.Destroy(MusicPlayer.Current.gameObject);
            }
        }

        private static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name == name)
                    return t.gameObject;
            }

            return null;
        }

        [UnityTest]
        public IEnumerator Stats_ShowThisRun()
        {
            var stats = Find("Stats").GetComponent<TMP_Text>();
            StringAssert.Contains("Level 1:  1:05", stats.text);
            StringAssert.Contains("Level 2:  1:30", stats.text);
            StringAssert.Contains("Total:  2:35  ·  4 Teddies  ·  1 Life Lost", stats.text);
            yield break;
        }

        [UnityTest]
        public IEnumerator TheWholeCast_IsThere()
        {
            foreach (var name in new[] { "Nicholas", "Clown", "Balloon Dog", "Sword Swallower (Placeholder)" })
            {
                var member = Find(name);
                Assert.IsNotNull(member, name);
                Assert.IsNotNull(member.GetComponent<CelebrationLoop>(), $"{name} should celebrate.");
                Assert.IsNotNull(member.GetComponent<Image>().sprite, $"{name} should have a sprite.");
            }

            Assert.IsNotNull(Find("Eye").GetComponent<EyeGlance>());
            yield break;
        }

        [UnityTest]
        public IEnumerator Clown_Animates()
        {
            var image = Find("Clown").GetComponent<Image>();
            var first = image.sprite;
            var changed = false;
            for (var until = Time.realtimeSinceStartup + 1f; Time.realtimeSinceStartup < until && !changed;)
            {
                yield return null;
                changed = image.sprite != first;
            }

            Assert.IsTrue(changed, "The clown should play its idle frames.");
        }

        [UnityTest]
        public IEnumerator Confetti_Falls_AndTheTitlePopsIn()
        {
            Assert.Greater(Find("Confetti").GetComponent<ConfettiRain>().PieceCount, 0);
            yield return new WaitForSecondsRealtime(DemoCompleteScreen.TitleDelay + DemoCompleteScreen.TitlePopSeconds + 0.2f);
            Assert.Greater(Find("TitlePlate").transform.localScale.x, 1f, "The title should have popped in.");
        }

        [UnityTest]
        public IEnumerator OnlyTheMainMenuButton_IsThere()
        {
            Assert.IsNotNull(Find("Main Menu Button").GetComponent<Button>());
            Assert.IsNull(Find("Level Select Button"), "The ending only leads back to the main menu.");
            Assert.IsNull(Find("KeyHints"), "There are no keyboard shortcuts, so no key hints.");
            Assert.AreEqual(1, Object.FindObjectsByType<Button>(FindObjectsInactive.Include).Length);
            yield break;
        }

        [UnityTest]
        public IEnumerator Music_Plays()
        {
            yield return null;
            Assert.IsNotNull(MusicPlayer.Current);
            Assert.IsTrue(MusicPlayer.Current.GetComponent<AudioSource>().isPlaying);
            Assert.IsTrue(MusicPlayer.Current.PlaysIn("DemoComplete"));
            Assert.AreEqual("demo-complete-music", MusicPlayer.Current.Music.name, "The ending has its own track.");
        }

        [UnityTest]
        public IEnumerator LevelTwo_EndsTheDemo()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/_Project/Scenes/Levels/Level2.unity", new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
            var goal = Object.FindAnyObjectByType<LevelGoal>();
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            Assert.IsTrue((bool)typeof(LevelGoal).GetField("endsDemo", flags).GetValue(goal));
            Assert.AreEqual("DemoComplete", typeof(LevelGoal).GetField("demoEndScene", flags).GetValue(goal));
            Assert.IsTrue(Application.CanStreamedLevelBeLoaded("DemoComplete"), "The ending should be in the build.");
        }
    }
}
