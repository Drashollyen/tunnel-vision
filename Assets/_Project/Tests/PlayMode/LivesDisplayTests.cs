using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>Builds a bare player and four eyes (one per life) in a scene of their own.</summary>
    public class LivesDisplayTests
    {
        private PlayerLives _lives;
        private Image[] _eyes;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("LivesDisplayTest");

            _lives = new GameObject("Player").AddComponent<PlayerLives>();

            var canvas = new GameObject("HUD", typeof(Canvas));
            var holder = new GameObject("Lives Display", typeof(RectTransform));
            holder.transform.SetParent(canvas.transform, false);
            holder.SetActive(false); // So Awake waits until the eyes are filled in.

            _eyes = new Image[4];
            for (var i = 0; i < _eyes.Length; i++)
            {
                _eyes[i] = new GameObject("Eye " + (i + 1), typeof(RectTransform), typeof(Image)).GetComponent<Image>();
                _eyes[i].transform.SetParent(holder.transform, false);
            }

            var display = holder.AddComponent<LivesDisplay>();
            typeof(LivesDisplay).GetField("eyes", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(display, _eyes);
            holder.SetActive(true);
            yield return null; // Let Start run.
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        [UnityTest]
        public IEnumerator AtStart_AllEyesShowAtFullSize()
        {
            foreach (var eye in _eyes)
            {
                Assert.AreEqual(1f, eye.rectTransform.localScale.x, 0.001f);
                Assert.AreEqual(1f, eye.color.a, 0.001f);
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator LosingALife_PopsTheLastEyeAway()
        {
            _lives.LoseLife();
            yield return new WaitForSecondsRealtime(LivesDisplay.SwellSeconds * 0.8f);

            Assert.Greater(_eyes[3].rectTransform.localScale.x, 1.05f, "The eye should swell first.");

            yield return new WaitForSecondsRealtime(LivesDisplay.LoseSeconds);

            Assert.AreEqual(0f, _eyes[3].rectTransform.localScale.x, 0.001f, "The eye should be gone.");
            Assert.AreEqual(0f, _eyes[3].color.a, 0.001f);
            Assert.AreEqual(1f, _eyes[0].rectTransform.localScale.x, 0.001f, "The other eyes stay.");
            Assert.AreEqual(1f, _eyes[1].rectTransform.localScale.x, 0.001f);
            Assert.AreEqual(1f, _eyes[2].rectTransform.localScale.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator Pop_StillPlaysWhilePaused()
        {
            Time.timeScale = 0f;
            try
            {
                _lives.LoseLife();
                yield return new WaitForSecondsRealtime(LivesDisplay.LoseSeconds + 0.1f);

                Assert.AreEqual(0f, _eyes[3].rectTransform.localScale.x, 0.001f);
            }
            finally
            {
                Time.timeScale = 1f;
            }
        }
    }
}
