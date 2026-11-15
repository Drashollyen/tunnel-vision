using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Audio;
using TunnelVision.Player;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>
    /// A bare player and a level music source, with the death sequence shortened so the
    /// fades can be checked quickly.
    /// </summary>
    public class LevelMusicTests
    {
        private const float Step = 0.2f; // Each part of the shortened death sequence.

        private PlayerLives _lives;
        private PlayerRespawn _respawn;
        private AudioSource _source;
        private LevelMusic _music;
        private float _savedMusic;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("LevelMusicTest");
            _savedMusic = SettingsService.Current.MusicVolume;
            SettingsService.Current.MusicVolume = 1f;

            var player = new GameObject("Player");
            player.transform.position = new Vector3(100f, 100f, 0f);
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            _lives = player.AddComponent<PlayerLives>();
            _respawn = player.AddComponent<PlayerRespawn>();
            foreach (var field in new[] { "revealDuration", "revealHoldDuration", "fadeDuration", "blackHoldDuration" })
            {
                typeof(PlayerRespawn).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_respawn, Step);
            }

            var musicObject = new GameObject("Level Music");
            _source = musicObject.AddComponent<AudioSource>();
            _source.clip = AudioClip.Create("Track", 44100 * 10, 1, 44100, false);
            _source.loop = true;
            _source.volume = 0.25f;
            _source.Play();
            _music = musicObject.AddComponent<LevelMusic>();
            yield return null;
        }

        [TearDown]
        public void Clean()
        {
            Time.timeScale = 1f;
            SettingsService.Current.MusicVolume = _savedMusic;
            IsolatedScene.Close();
        }

        [Test]
        public void Volume_IsTheSourceVolumeTimesFadeAndSetting()
        {
            Assert.AreEqual(0.125f, LevelMusic.VolumeFor(0.25f, 1f, 0.5f), 0.001f);
            Assert.AreEqual(0f, LevelMusic.VolumeFor(0.25f, 0f, 1f), 0.001f);
        }

        [UnityTest]
        public IEnumerator Music_FollowsTheMusicSetting()
        {
            Assert.AreEqual(0.25f, _source.volume, 0.001f, "Full music volume keeps the level's own mix.");

            SettingsService.Current.MusicVolume = 0.5f;
            yield return null;
            Assert.AreEqual(0.125f, _source.volume, 0.001f);
        }

        [UnityTest]
        public IEnumerator Dying_FadesTheMusicOutGraduallyByTheBlackScreen()
        {
            _respawn.Kill();
            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack * 0.4f);

            Assert.Less(_music.Fade, 1f, "It should be fading.");
            Assert.Greater(_music.Fade, 0f, "It should fade, not cut out.");

            yield return new WaitForSecondsRealtime(_respawn.SecondsUntilBlack * 0.6f + 0.1f);
            Assert.AreEqual(0f, _source.volume, 0.001f, "Silent once the screen is black.");
        }

        [UnityTest]
        public IEnumerator Respawning_StartsTheTrackAgainAndFadesItIn()
        {
            _source.time = 3f; // Partway into the track.
            _respawn.Kill();

            // Wait until the screen starts fading back in.
            var giveUpAt = Time.realtimeSinceStartup + 5f;
            while (_music.IsSilenced && Time.realtimeSinceStartup < giveUpAt)
            {
                yield return null;
            }

            Assert.IsFalse(_music.IsSilenced, "The music should come back on respawn.");
            Assert.IsTrue(_source.isPlaying);
            Assert.Less(_source.time, 1f, "It should start again from the top.");

            yield return new WaitForSecondsRealtime(_respawn.FadeInSeconds + 0.1f);
            Assert.AreEqual(1f, _music.Fade, 0.001f);
            Assert.AreEqual(0.25f, _source.volume, 0.001f);
        }

        [UnityTest]
        public IEnumerator OutOfLives_StaysSilent()
        {
            while (_lives.CurrentLives > 1)
            {
                _lives.LoseLife();
            }

            _respawn.Kill(); // The last life.
            yield return new WaitForSecondsRealtime(Step * 6f);

            Assert.IsTrue(_music.IsSilenced);
            Assert.AreEqual(0f, _source.volume, 0.001f);
            Assert.IsFalse(_source.isPlaying);
        }
    }
}
