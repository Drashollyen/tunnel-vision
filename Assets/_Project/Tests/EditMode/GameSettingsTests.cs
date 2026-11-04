using System.Collections.Generic;
using NUnit.Framework;
using TunnelVision.Settings;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class GameSettingsTests
    {
        private sealed class MemoryStore : ISettingsStore
        {
            public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
            public int SaveCount;

            public bool HasKey(string key) => Values.ContainsKey(key);

            public float GetFloat(string key, float defaultValue) =>
                Values.TryGetValue(key, out var v) ? (float)v : defaultValue;

            public void SetFloat(string key, float value) => Values[key] = value;

            public int GetInt(string key, int defaultValue) =>
                Values.TryGetValue(key, out var v) ? (int)v : defaultValue;

            public void SetInt(string key, int value) => Values[key] = value;

            public void Save() => SaveCount++;
        }

        [Test]
        public void Load_WithEmptyStore_UsesDefaults()
        {
            var settings = new GameSettings(new MemoryStore());

            settings.Load();

            Assert.That(settings.MasterVolume, Is.EqualTo(GameSettings.DefaultMasterVolume));
            Assert.That(settings.InterfaceVolume, Is.EqualTo(GameSettings.DefaultInterfaceVolume));
            Assert.That(settings.InterfaceSounds, Is.EqualTo(GameSettings.DefaultInterfaceSounds));
            Assert.That(settings.DisplayMode, Is.EqualTo(GameSettings.DefaultDisplayMode));
            Assert.That(settings.ResolutionWidth, Is.EqualTo(GameSettings.DefaultResolutionSize));
            Assert.That(settings.ResolutionHeight, Is.EqualTo(GameSettings.DefaultResolutionSize));
        }

        [Test]
        public void Save_ThenLoad_RoundTripsValues()
        {
            var store = new MemoryStore();
            var settings = new GameSettings(store)
            {
                MasterVolume = 0.25f,
                InterfaceVolume = 0.5f,
                InterfaceSounds = false,
                DisplayMode = FullScreenMode.Windowed,
                ResolutionWidth = 1280,
                ResolutionHeight = 720,
            };

            settings.Save();
            var reloaded = new GameSettings(store);
            reloaded.Load();

            Assert.That(reloaded.MasterVolume, Is.EqualTo(0.25f));
            Assert.That(reloaded.InterfaceVolume, Is.EqualTo(0.5f));
            Assert.That(reloaded.InterfaceSounds, Is.False);
            Assert.That(reloaded.DisplayMode, Is.EqualTo(FullScreenMode.Windowed));
            Assert.That(reloaded.ResolutionWidth, Is.EqualTo(1280));
            Assert.That(reloaded.ResolutionHeight, Is.EqualTo(720));
            Assert.That(store.SaveCount, Is.EqualTo(1));
        }

        [Test]
        public void Load_WithUnsupportedDisplayMode_UsesDefault()
        {
            var store = new MemoryStore();
            store.SetInt(GameSettings.DisplayModeKey, (int)FullScreenMode.MaximizedWindow);
            var settings = new GameSettings(store);

            settings.Load();

            Assert.That(settings.DisplayMode, Is.EqualTo(GameSettings.DefaultDisplayMode));
        }

        [TestCase(FullScreenMode.ExclusiveFullScreen, 1280, 720, 1280, 720)]
        [TestCase(FullScreenMode.Windowed, 1280, 720, 1280, 720)]
        [TestCase(FullScreenMode.FullScreenWindow, 1280, 720, 2560, 1440)]
        [TestCase(FullScreenMode.Windowed, 0, 0, 2560, 1440)]
        public void GetWindowSize_FollowsModeAndSavedSize(
            FullScreenMode mode, int width, int height, int expectedWidth, int expectedHeight)
        {
            var settings = new GameSettings(new MemoryStore())
            {
                DisplayMode = mode,
                ResolutionWidth = width,
                ResolutionHeight = height,
            };

            Vector2Int size = settings.GetWindowSize(new Vector2Int(2560, 1440));

            Assert.That(size, Is.EqualTo(new Vector2Int(expectedWidth, expectedHeight)));
        }

        [Test]
        public void ResolutionList_RemovesRepeatsAndTinySizes_LargestFirst()
        {
            var available = new[]
            {
                new Vector2Int(640, 480),
                new Vector2Int(1280, 720),
                new Vector2Int(1920, 1080),
                new Vector2Int(1920, 1080),
            };

            var list = ResolutionList.Build(available, new Vector2Int(1600, 900));

            Assert.That(list, Is.EqualTo(new[]
            {
                new Vector2Int(1920, 1080),
                new Vector2Int(1600, 900),
                new Vector2Int(1280, 720),
            }));
        }

        [TestCase(-1f, 0f)]
        [TestCase(1.5f, 1f)]
        [TestCase(0.4f, 0.4f)]
        public void MasterVolume_IsClampedToZeroOne(float input, float expected)
        {
            var settings = new GameSettings(new MemoryStore()) { MasterVolume = input };

            Assert.That(settings.MasterVolume, Is.EqualTo(expected));
        }

        [TestCase(-1f, 0f)]
        [TestCase(1.5f, 1f)]
        public void InterfaceVolume_IsClampedToZeroOne(float input, float expected)
        {
            var settings = new GameSettings(new MemoryStore()) { InterfaceVolume = input };

            Assert.That(settings.InterfaceVolume, Is.EqualTo(expected));
        }

        [Test]
        public void EffectiveInterfaceVolume_IsZeroWhenInterfaceSoundsOff()
        {
            var settings = new GameSettings(new MemoryStore()) { InterfaceVolume = 0.6f };

            Assert.That(settings.EffectiveInterfaceVolume, Is.EqualTo(0.6f));

            settings.InterfaceSounds = false;

            Assert.That(settings.EffectiveInterfaceVolume, Is.EqualTo(0f));
        }

        [Test]
        public void ResetToDefaults_RestoresDefaults()
        {
            var settings = new GameSettings(new MemoryStore())
            {
                MasterVolume = 0.1f,
                InterfaceVolume = 0.1f,
                InterfaceSounds = false,
                DisplayMode = FullScreenMode.Windowed,
                ResolutionWidth = 1280,
                ResolutionHeight = 720,
            };

            settings.ResetToDefaults();

            Assert.That(settings.MasterVolume, Is.EqualTo(GameSettings.DefaultMasterVolume));
            Assert.That(settings.InterfaceVolume, Is.EqualTo(GameSettings.DefaultInterfaceVolume));
            Assert.That(settings.InterfaceSounds, Is.EqualTo(GameSettings.DefaultInterfaceSounds));
            Assert.That(settings.DisplayMode, Is.EqualTo(GameSettings.DefaultDisplayMode));
            Assert.That(settings.ResolutionWidth, Is.EqualTo(GameSettings.DefaultResolutionSize));
            Assert.That(settings.ResolutionHeight, Is.EqualTo(GameSettings.DefaultResolutionSize));
        }
    }
}
