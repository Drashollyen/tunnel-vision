using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Builds a bare player and a Round Over message in a scene of their own,
    /// so these tests don't depend on the HUD prefab or any level.
    /// </summary>
    public class RoundOverMessageTests
    {
        private const float ButtonDelay = 0.5f;

        private PlayerLives _lives;
        private RoundOverMessage _message;
        private GameObject _panel;
        private TMP_Text _title;
        private TMP_Text _subtitle;
        private Button _button;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("RoundOverTest");

            new GameObject("EventSystem", typeof(EventSystem));

            _lives = new GameObject("Player").AddComponent<PlayerLives>();

            var canvas = new GameObject("HUD", typeof(Canvas));
            var holder = new GameObject("RoundOverMessage");
            holder.transform.SetParent(canvas.transform, false);
            holder.SetActive(false); // So Awake waits until the fields are filled in.

            _panel = new GameObject("Panel", typeof(RectTransform));
            _panel.transform.SetParent(holder.transform, false);
            _title = MakeText("Title");
            _subtitle = MakeText("Subtitle");
            _button = new GameObject("Level Select Button", typeof(RectTransform), typeof(Image), typeof(Button))
                .GetComponent<Button>();
            _button.transform.SetParent(_panel.transform, false);

            _message = holder.AddComponent<RoundOverMessage>();
            Set("panel", _panel);
            Set("title", _title);
            Set("subtitle", _subtitle);
            Set("levelSelectButton", _button);
            Set("buttonDelay", ButtonDelay);
            Set("levelSelectScene", "NoSuchSceneForTests"); // SceneFader only warns, so no scene change.
            holder.SetActive(true);
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
            typeof(RoundOverMessage)
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_message, value);
        }

        private void LoseAllLives()
        {
            for (var i = 0; i < _lives.MaxLives; i++)
            {
                _lives.LoseLife();
            }
        }

        [UnityTest]
        public IEnumerator StartsHidden()
        {
            yield return null;

            Assert.IsFalse(_panel.activeSelf);
            Assert.IsFalse(_button.gameObject.activeSelf);
            Assert.IsFalse(_message.IsGameOver);
        }

        [UnityTest]
        public IEnumerator RoundOver_ShowsLivesLeft_WithoutPausingOrButton()
        {
            yield return null;
            _lives.LoseLife();
            yield return new WaitForSecondsRealtime(ButtonDelay * 2f);

            Assert.IsTrue(_panel.activeSelf);
            Assert.AreEqual("Round Over", _title.text);
            Assert.AreEqual("2 Lives Remaining", _subtitle.text);
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(_button.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator GameOver_PausesStraightAway_ButtonWaitsForTheDelay()
        {
            yield return null;
            LoseAllLives();
            yield return null;

            Assert.IsTrue(_message.IsGameOver);
            Assert.AreEqual("Game Over", _title.text);
            Assert.IsFalse(_subtitle.gameObject.activeSelf);
            Assert.AreEqual(0f, Time.timeScale, "Game Over should pause the game.");
            Assert.IsFalse(_button.gameObject.activeSelf, "The button should wait for the delay.");
        }

        [UnityTest]
        public IEnumerator GameOver_ShowsAndSelectsTheButtonAfterTheDelay()
        {
            yield return null;
            LoseAllLives();
            yield return new WaitForSecondsRealtime(ButtonDelay + 0.25f);

            Assert.IsTrue(_button.gameObject.activeInHierarchy);
            Assert.AreEqual(_button.gameObject, EventSystem.current.currentSelectedGameObject,
                "The button should be selected so Enter works.");
        }

        [UnityTest]
        public IEnumerator GameOver_StaysUp()
        {
            yield return null;
            LoseAllLives();
            yield return new WaitForSecondsRealtime(3f); // Longer than a Round Over stays up.

            Assert.IsTrue(_panel.activeSelf);
            Assert.AreEqual(0f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator PressingTheButton_Unpauses()
        {
            yield return null;
            LoseAllLives();
            yield return new WaitForSecondsRealtime(ButtonDelay + 0.25f);

            LogAssert.Expect(LogType.Warning, "SceneFader: NoSuchSceneForTests isn't in the build's scene list.");
            _button.onClick.Invoke();

            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Destroyed_WhilePaused_Unpauses()
        {
            yield return null;
            LoseAllLives();
            yield return null;

            Object.Destroy(_message.gameObject);
            yield return null;

            Assert.AreEqual(1f, Time.timeScale);
        }
    }
}
