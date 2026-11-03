using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    public class IntroScreenTests
    {
        private const string IntroScene = "IntroScreen";
        private const string GameScene = "ConeTest";

        [UnitySetUp]
        public IEnumerator LoadIntroScreen()
        {
            yield return SceneManager.LoadSceneAsync(IntroScene);
            yield return null; // Let Start() run.
        }

        [Test]
        public void IntroScreen_HasPlayOptionsAndQuitButtons()
        {
            Assert.That(FindButton("Play Button"), Is.Not.Null);
            Assert.That(FindButton("Options Button"), Is.Not.Null);
            Assert.That(FindButton("Quit Button"), Is.Not.Null);
        }

        [Test]
        public void IntroScreen_StartsWithOptionsHidden()
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
            Assert.That(Find("Coming Soon Text").activeInHierarchy, Is.True);

            FindButton("Back Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Options Panel").activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator PlayButton_LoadsGameScene()
        {
            FindButton("Play Button").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameScene));
        }

        private static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }

            Assert.Fail($"'{name}' not found in the IntroScreen scene");
            return null;
        }

        private static Button FindButton(string name) => Find(name).GetComponent<Button>();
    }
}
