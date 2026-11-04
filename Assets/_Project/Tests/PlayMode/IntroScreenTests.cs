using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.Settings;
using TunnelVision.UI;
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

        private GameSettings _settingsBeforeTest;

        [UnitySetUp]
        public IEnumerator LoadIntroScreen()
        {
            // These tests save settings, so keep the real ones to put back afterwards.
            _settingsBeforeTest = new GameSettings(new PlayerPrefsSettingsStore());
            _settingsBeforeTest.Load();

            yield return SceneManager.LoadSceneAsync(IntroScene);
            yield return null; // Let Start() run.
        }

        [TearDown]
        public void RestoreSettings()
        {
            _settingsBeforeTest.Save();
            SettingsService.Current.Load();
            SettingsService.Current.Apply();
        }

        [Test]
        public void IntroScreen_HasPlayOptionsAndQuitButtons()
        {
            Assert.That(FindButton("Play Button"), Is.Not.Null);
            Assert.That(FindButton("Options Button"), Is.Not.Null);
            Assert.That(FindButton("Info Button"), Is.Not.Null);
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

            FindButton("Back Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Options Panel").activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator InfoButton_OpensNote_AndCloseReturns()
        {
            Assert.That(Find("Info Panel").activeInHierarchy, Is.False);

            FindButton("Info Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.False);
            Assert.That(Find("Info Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Info Text").GetComponent<TMP_Text>().text, Does.Contain("prototype"));

            FindButton("Close Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Main Panel").activeInHierarchy, Is.True);
            Assert.That(Find("Info Panel").activeInHierarchy, Is.False);
        }

        [UnityTest]
        public IEnumerator OptionsPage_ShowsSavedSettings()
        {
            SettingsService.Current.MasterVolume = 0.25f;
            SettingsService.Current.InterfaceVolume = 0.75f;
            SettingsService.Current.InterfaceSounds = false;
            SettingsService.Current.Fullscreen = false;

            FindButton("Options Button").onClick.Invoke();
            yield return null;

            Assert.That(Find("Master Volume Slider").GetComponent<Slider>().value, Is.EqualTo(0.25f));
            Assert.That(Find("Interface Volume Slider").GetComponent<Slider>().value, Is.EqualTo(0.75f));
            Assert.That(Find("Interface Volume Slider").GetComponent<Slider>().interactable, Is.False);
            Assert.That(Find("Interface Sounds Toggle").GetComponent<Toggle>().isOn, Is.False);
            Assert.That(Find("Fullscreen Toggle").GetComponent<Toggle>().isOn, Is.False);
        }

        [UnityTest]
        public IEnumerator OptionsPage_ChangesAreAppliedAndSurviveARestart()
        {
            FindButton("Options Button").onClick.Invoke();
            yield return null;

            Find("Master Volume Slider").GetComponent<Slider>().value = 0.3f;
            Find("Interface Volume Slider").GetComponent<Slider>().value = 0.6f;
            Find("Interface Sounds Toggle").GetComponent<Toggle>().isOn = false;
            Find("Fullscreen Toggle").GetComponent<Toggle>().isOn = false;

            Assert.That(AudioListener.volume, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(SettingsService.Current.EffectiveInterfaceVolume, Is.EqualTo(0f));
            Assert.That(Find("Interface Volume Slider").GetComponent<Slider>().interactable, Is.False);

            // A new instance only sees what was saved, the same as starting the game again.
            var restarted = new GameSettings(new PlayerPrefsSettingsStore());
            restarted.Load();

            Assert.That(restarted.MasterVolume, Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(restarted.InterfaceVolume, Is.EqualTo(0.6f).Within(0.001f));
            Assert.That(restarted.InterfaceSounds, Is.False);
            Assert.That(restarted.Fullscreen, Is.False);
        }

        [UnityTest]
        public IEnumerator InterfaceSounds_AreAddedToEveryControl()
        {
            foreach (var selectable in Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include))
            {
                Assert.That(selectable.GetComponent<InterfaceSoundTrigger>(), Is.Not.Null, selectable.name);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator PlayButton_LoadsGameScene()
        {
            FindButton("Play Button").onClick.Invoke();
            yield return null;
            yield return null;

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(GameScene));
        }

        [UnityTest]
        public IEnumerator ClickSound_KeepsPlayingWhenTheSceneChanges()
        {
            Object.FindFirstObjectByType<InterfaceSounds>().PlayClick();

            yield return SceneManager.LoadSceneAsync(GameScene);

            Assert.That(GameObject.Find(InterfaceSounds.PlayerName), Is.Not.Null);
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
