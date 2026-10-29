using System.Collections;
using NUnit.Framework;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    public class MainMenuTests
    {
        private const string MainMenuScene = "MainMenu";

        [UnitySetUp]
        public IEnumerator LoadMainMenu()
        {
            yield return SceneManager.LoadSceneAsync(MainMenuScene);
            yield return null; // Let Start() run.
        }

        [Test]
        public void MainMenu_HasPlayOptionsAndQuitButtons()
        {
            Assert.That(FindButton("Play Button"), Is.Not.Null);
            Assert.That(FindButton("Options Button"), Is.Not.Null);
            Assert.That(FindButton("Quit Button"), Is.Not.Null);
        }

        [Test]
        public void MainMenu_StartsWithOptionsHidden()
        {
            Assert.That(Find("Main Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Options Panel").activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator OptionsButton_OpensOptions_AndBackReturns()
        {
            FindButton("Options Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.False);
            Assert.That(Find("Options Panel").activeInHierarchy, Is.True);

            FindButton("Back Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Options Panel").activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator MenuPanel_IsCenteredAndFitsItsContent()
        {
            yield return null; // Layout runs at end of frame.
            Canvas.ForceUpdateCanvases();

            var panel = (RectTransform)Find("Main Panel").transform;
            Assert.That(panel.anchoredPosition, Is.EqualTo(Vector2.zero));
            Assert.That(panel.rect.width, Is.GreaterThan(0f));

            foreach (RectTransform child in panel)
            {
                Assert.That(child.rect.width, Is.LessThanOrEqualTo(panel.rect.width), child.name + " overflows the panel");
            }
        }

        [Test]
        public void MenuControls_PlayInterfaceSounds()
        {
            Assert.That(Object.FindAnyObjectByType<UiSoundPlayer>(), Is.Not.Null);

            foreach (var name in new[] { "Play Button", "Options Button", "Quit Button", "Back Button", "Reset Button" })
            {
                Assert.That(Find(name).GetComponent<UiSelectableSounds>(), Is.Not.Null, name + " has no sounds");
            }
        }

        [UnityTest]
        public IEnumerator PlayButton_LoadsGameScene()
        {
            var gameScene = Object.FindAnyObjectByType<MainMenuController>().GameSceneName;
            FindButton("Play Button").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(gameScene));
        }

        private static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }

            Assert.Fail($"'{name}' not found in the MainMenu scene");
            return null;
        }

        private static Button FindButton(string name) => Find(name).GetComponent<Button>();
    }
}
