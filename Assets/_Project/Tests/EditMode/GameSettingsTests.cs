using System.Collections.Generic;
using NUnit.Framework;
using TunnelVision.Settings;

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
            Assert.That(settings.Fullscreen, Is.EqualTo(GameSettings.DefaultFullscreen));
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
                Fullscreen = false,
            };

            settings.Save();
            var reloaded = new GameSettings(store);
            reloaded.Load();

            Assert.That(reloaded.MasterVolume, Is.EqualTo(0.25f));
            Assert.That(reloaded.InterfaceVolume, Is.EqualTo(0.5f));
            Assert.That(reloaded.InterfaceSounds, Is.False);
            Assert.That(reloaded.Fullscreen, Is.False);
            Assert.That(store.SaveCount, Is.EqualTo(1));
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
                Fullscreen = false,
            };

            settings.ResetToDefaults();

            Assert.That(settings.MasterVolume, Is.EqualTo(GameSettings.DefaultMasterVolume));
            Assert.That(settings.InterfaceVolume, Is.EqualTo(GameSettings.DefaultInterfaceVolume));
            Assert.That(settings.InterfaceSounds, Is.EqualTo(GameSettings.DefaultInterfaceSounds));
            Assert.That(settings.Fullscreen, Is.EqualTo(GameSettings.DefaultFullscreen));
        }
    }
}
