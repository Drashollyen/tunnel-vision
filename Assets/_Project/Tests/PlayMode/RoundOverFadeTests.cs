using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>
    /// A lost life's "Game Over" and lives line stay up through the reveal, then fade out as the
    /// screen goes black. Uses a real, shortened death sequence.
    /// </summary>
    public class RoundOverFadeTests
    {
        private const float Step = 0.3f; // Each part of the shortened death sequence.

        private PlayerRespawn _respawn;
        private RoundOverMessage _message;
        private GameObject _panel;
        private Button _button;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("RoundOverFadeTest");

            var player = new GameObject("Player");
            player.transform.position = new Vector3(100f, 100f, 0f);
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<PlayerLives>();
            _respawn = player.AddComponent<PlayerRespawn>();
            TestPlayer.ShortenDeath(_respawn, Step);

            var canvas = new GameObject("HUD", typeof(Canvas));
            var holder = new GameObject("RoundOverMessage");
            holder.transform.SetParent(canvas.transform, false);
            holder.SetActive(false); // So Awake waits until the fields are filled in.
            _panel = new GameObject("Panel", typeof(RectTransform));
            _panel.transform.SetParent(holder.transform, false);

            _message = holder.AddComponent<RoundOverMessage>();
            Set("panel", _panel);
            Set("title", MakeText("Title"));
            Set("subtitle", MakeText("Subtitle"));
            _button = new GameObject("Level Select Button", typeof(RectTransform), typeof(Image), typeof(Button))
                .GetComponent<Button>();
            _button.transform.SetParent(_panel.transform, false);
            Set("levelSelectButton", _button);
            Set("levelSelectScene", "NoSuchSceneForTests");
            holder.SetActive(true);
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            Time.timeScale = 1f;
            IsolatedScene.Close();
        }

        private TMP_Text MakeText(string name)
        {
            var text = new GameObject(name, typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            text.transform.SetParent(_panel.transform, false);
            return text;
        }

        private void Set(string field, object value)
        {
            Private.Set(_message, field, value);
        }

        [UnityTest]
        public IEnumerator LostLife_StaysUpThroughTheReveal()
        {
            _respawn.Kill();
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack - _respawn.FadeToBlackSeconds - 0.1f);

            Assert.IsTrue(_panel.activeSelf);
            Assert.AreEqual(1f, _message.Visibility, 0.001f);
        }

        [UnityTest]
        public IEnumerator LostLife_FadesOutAsTheScreenGoesBlack()
        {
            _respawn.Kill();
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack - _respawn.FadeToBlackSeconds * 0.5f);

            Assert.Less(_message.Visibility, 1f, "It should be fading with the screen.");
            Assert.Greater(_message.Visibility, 0f, "It should fade, not vanish.");

            yield return new WaitForSecondsRealtime(_respawn.FadeToBlackSeconds * 0.5f + 0.1f);
            Assert.IsFalse(_panel.activeSelf, "Gone by the time the screen is black.");
        }

        [UnityTest]
        public IEnumerator OutOfLives_StaysUp()
        {
            var lives = _respawn.GetComponent<PlayerLives>();
            while (lives.CurrentLives > 1)
            {
                lives.LoseLife();
            }

            yield return new WaitForSecondsRealtime(0.1f);
            _respawn.Kill(); // The last life.
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack + 0.2f);

            Assert.IsTrue(_panel.activeSelf, "Out Of Lives stays on screen with its button.");
            Assert.AreEqual(1f, _message.Visibility, 0.001f);
        }

        private void LoseAllButOneLife()
        {
            var lives = _respawn.GetComponent<PlayerLives>();
            while (lives.CurrentLives > 1)
            {
                lives.LoseLife();
            }
        }

        [UnityTest]
        public IEnumerator OutOfLivesButton_WaitsForTheScreenToStartGoingBlack()
        {
            LoseAllButOneLife();
            yield return new WaitForSecondsRealtime(0.1f);
            _respawn.Kill(); // The last life.
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack - _respawn.FadeToBlackSeconds - 0.1f);

            Assert.IsFalse(_button.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator OutOfLivesButton_FadesInAsTheScreenGoesBlack()
        {
            LoseAllButOneLife();
            yield return new WaitForSecondsRealtime(0.1f);
            _respawn.Kill(); // The last life.
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack - _respawn.FadeToBlackSeconds * 0.5f);

            Assert.IsTrue(_button.gameObject.activeSelf);
            Assert.Greater(_message.ButtonVisibility, 0f, "It should be fading in.");
            Assert.Less(_message.ButtonVisibility, 1f, "It should fade in, not pop.");

            yield return new WaitForSecondsRealtime(_respawn.FadeToBlackSeconds * 0.5f + 0.1f);
            Assert.AreEqual(1f, _message.ButtonVisibility, 0.001f, "Fully there once the screen is black.");
        }
    }
}
