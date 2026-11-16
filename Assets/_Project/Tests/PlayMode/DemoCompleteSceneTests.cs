using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.Audio;
using TunnelVision.Level;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
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
            yield return TestScenes.Load(Scene);
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
            foreach (var name in new[] { "Chaplin", "Nicholas", "Clown", "Balloon Dog", "Sword Swallower" })
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
        public IEnumerator Chaplin_IsThere_AndClickable()
        {
            // That he's there with a sprite is TheWholeCast_IsThere's job; this is what makes him clickable.
            var chaplin = Find("Chaplin");
            Assert.AreEqual("Cast", chaplin.transform.parent.name, "Chaplin stands with the rest of the cast.");
            Assert.IsTrue(chaplin.GetComponent<Image>().raycastTarget, "Chaplin has to catch clicks.");
            Assert.IsNull(chaplin.GetComponent<Button>(), "He's hidden: no button look, and no extra button.");

            var egg = chaplin.GetComponent<ChaplinEasterEgg>();
            Assert.IsNotNull(egg);
            Assert.AreEqual("chaplin-tunnel-vision", egg.Voice.name);
            Assert.AreEqual(Find("Confetti").GetComponent<ConfettiRain>(), egg.Confetti);
            yield break;
        }

        [UnityTest]
        public IEnumerator Chaplin_StaysClearOfTheStatsButtonsAndCast()
        {
            var chaplin = ScreenRect(Find("Chaplin"));
            foreach (var name in new[] { "Stats Box", "Main Menu Button", "Nicholas", "Clown", "Balloon Dog", "Sword Swallower", "TitlePlate", "Subtitle" })
            {
                Assert.IsFalse(chaplin.Overlaps(ScreenRect(Find(name))), $"Chaplin overlaps {name}.");
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator ClickingChaplin_RunsTheWholeShow_ThenCloses()
        {
            var egg = Find("Chaplin").GetComponent<ChaplinEasterEgg>();
            var quits = 0;
            var realQuit = ChaplinEasterEgg.QuitAction;
            ChaplinEasterEgg.QuitAction = () => quits++;
            try
            {
                yield return null;
                var music = MusicPlayer.Current;
                ExecuteEvents.Execute(egg.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
                yield return null;

                Assert.AreEqual(ChaplinEasterEgg.Stage.MusicFading, egg.Current);
                Assert.IsTrue(music == null || music.IsFadingOut, "The music should fade out.");
                Assert.IsNotNull(Find("Easter Egg Overlay"), "The black screen goes on top of everything.");

                var seen = new System.Collections.Generic.HashSet<ChaplinEasterEgg.Stage>();
                for (var until = Time.realtimeSinceStartup + 15f; Time.realtimeSinceStartup < until && egg.Current != ChaplinEasterEgg.Stage.Closing;)
                {
                    seen.Add(egg.Current);
                    yield return null;
                }

                Assert.IsTrue(seen.Contains(ChaplinEasterEgg.Stage.FadingToBlack), "The screen should fade to black.");
                Assert.IsTrue(seen.Contains(ChaplinEasterEgg.Stage.Voice), "Chaplin's clip should play.");
                Assert.AreEqual(ChaplinEasterEgg.Stage.Closing, egg.Current);
                Assert.AreEqual(1, quits, "The game closes once at the end.");
                Assert.AreEqual(1f, egg.Black.color.a, 0.001f, "It ends on black.");
                foreach (var colour in egg.Confetti.PieceColours)
                {
                    Assert.AreEqual(Color.black, colour, "The confetti should have turned black.");
                }
            }
            finally
            {
                ChaplinEasterEgg.QuitAction = realQuit;
            }
        }

        private static Rect ScreenRect(GameObject go)
        {
            var corners = new Vector3[4];
            ((RectTransform)go.transform).GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
