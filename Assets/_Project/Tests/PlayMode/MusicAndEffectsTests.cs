using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TunnelVision.Audio;
using TunnelVision.Settings;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    /// <summary>The menu music and the Music and Effects volume settings.</summary>
    public class MusicAndEffectsTests
    {
        private AudioClip _track;
        private float _savedMusic;
        private float _savedEffects;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("MusicTest");
            _track = AudioClip.Create("Track", 44100, 1, 44100, false);
            _savedMusic = SettingsService.Current.MusicVolume;
            _savedEffects = SettingsService.Current.EffectsVolume;
        }

        [TearDown]
        public void Clean()
        {
            if (MusicPlayer.Current != null)
            {
                Object.DestroyImmediate(MusicPlayer.Current.gameObject);
            }

            SettingsService.Current.MusicVolume = _savedMusic;
            SettingsService.Current.EffectsVolume = _savedEffects;
            SettingsService.Current.ApplyAudio();
            SoundEffectsVolume.RestoreGameplayNow();
            IsolatedScene.Close();
        }

        private MusicPlayer MakeMusic(string name = "Music")
        {
            var go = new GameObject(name, typeof(AudioSource));
            go.SetActive(false); // So Awake waits until the track is set.
            var player = go.AddComponent<MusicPlayer>();
            typeof(MusicPlayer).GetField("music", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, _track);
            typeof(MusicPlayer).GetField("fadeInSeconds", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(player, 0.2f);
            go.SetActive(true);
            return player;
        }

        [Test]
        public void MusicVolume_IsTheFadeTimesTheSetting()
        {
            Assert.AreEqual(0.3f, MusicPlayer.VolumeFor(0.5f, 0.6f), 0.001f);
            Assert.AreEqual(0f, MusicPlayer.VolumeFor(0f, 1f), 0.001f);
            Assert.AreEqual(1f, MusicPlayer.VolumeFor(1f, 1f), 0.001f);
        }

        [UnityTest]
        public IEnumerator Music_FadesInAndFollowsTheMusicSetting()
        {
            SettingsService.Current.MusicVolume = 0.5f;
            var player = MakeMusic();
            var source = player.GetComponent<AudioSource>();

            Assert.IsTrue(source.isPlaying);
            Assert.IsTrue(source.loop);
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.AreEqual(1f, player.Fade, 0.001f);
            Assert.AreEqual(0.5f, source.volume, 0.001f);

            SettingsService.Current.MusicVolume = 0.2f;
            yield return null;
            Assert.AreEqual(0.2f, source.volume, 0.001f);
        }

        [UnityTest]
        public IEnumerator Music_KeepsPlayingIntoAnotherMenu()
        {
            var player = MakeMusic();
            yield return new WaitForSecondsRealtime(0.3f);

            player.OnLoadStarted("LevelSelect");
            yield return new WaitForSecondsRealtime(0.4f);

            Assert.IsTrue(player != null, "The music shouldn't stop between menus.");
            Assert.IsFalse(player.IsFadingOut);
        }

        [UnityTest]
        public IEnumerator Music_FadesOutBeforeALevel()
        {
            var player = MakeMusic();
            yield return new WaitForSecondsRealtime(0.3f);

            player.OnLoadStarted("Level1");
            Assert.IsTrue(player.IsFadingOut);

            yield return new WaitForSecondsRealtime(UI.SceneFader.FadeSeconds + 0.15f);
            Assert.IsTrue(player == null, "The music should be gone by the time the screen is black.");
        }

        [UnityTest]
        public IEnumerator SecondMenu_KeepsTheTrackAlreadyPlaying()
        {
            var first = MakeMusic("First");
            yield return null;
            var second = MakeMusic("Second");
            yield return null;

            Assert.AreSame(first, MusicPlayer.Current, "The track playing should carry on, not restart.");
            Assert.IsTrue(second == null, "The second menu's music should step aside.");
        }

        [Test]
        public void Effects_AreScaledFromTheirOwnVolume()
        {
            Assert.AreEqual(0.25f, SoundEffectsVolume.Scaled(0.5f, 0.5f), 0.001f);
            Assert.AreEqual(0f, SoundEffectsVolume.Scaled(0.5f, 0f), 0.001f);
        }

        [UnityTest]
        public IEnumerator EffectsSetting_ScalesGameSoundsButNotTheMusic()
        {
            var sound = new GameObject("Jump Sound").AddComponent<AudioSource>();
            sound.volume = 0.5f;
            SettingsService.Current.MusicVolume = 1f;
            var music = MakeMusic().GetComponent<AudioSource>();
            yield return new WaitForSecondsRealtime(0.3f);

            SettingsService.Current.EffectsVolume = 0.5f;
            SettingsService.Current.ApplyAudio();
            Assert.AreEqual(0.25f, sound.volume, 0.001f);

            // Changing it again works from the sound's own volume, not the last result.
            SettingsService.Current.EffectsVolume = 1f;
            SettingsService.Current.ApplyAudio();
            Assert.AreEqual(0.5f, sound.volume, 0.001f);

            yield return null;
            Assert.AreEqual(1f, music.volume, 0.001f, "The music has its own setting.");
        }


        private static AudioSource MakeLoopingSound(string name)
        {
            var sound = new GameObject(name).AddComponent<AudioSource>();
            sound.clip = AudioClip.Create(name, 44100 * 4, 1, 44100, false);
            sound.loop = true;
            sound.volume = 0.5f;
            sound.Play();
            return sound;
        }

        [UnityTest]
        public IEnumerator FadingOutGameplay_FadesThenPausesLoopingSounds()
        {
            var wheel = MakeLoopingSound("Wheel Sound");
            SettingsService.Current.EffectsVolume = 1f;
            SettingsService.Current.ApplyAudio();
            Assert.AreEqual(0.5f, wheel.volume, 0.001f);

            // A long fade, so a slow frame in the test run can't make it look like a cut.
            SoundEffectsVolume.FadeOutGameplay(1f);
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.Less(wheel.volume, 0.5f, "It should be fading.");
            Assert.Greater(wheel.volume, 0f, "It should fade, not cut out.");

            yield return new WaitForSecondsRealtime(1f);
            Assert.AreEqual(0f, wheel.volume, 0.001f);
            Assert.IsFalse(wheel.isPlaying, "Once silent it should be paused, not just quiet.");
            Assert.IsTrue(SoundEffectsVolume.IsGameplaySilenced);
        }

        [UnityTest]
        public IEnumerator FadingOutGameplay_WaitsForTheDelayFirst()
        {
            var wheel = MakeLoopingSound("Wheel Sound");
            SettingsService.Current.EffectsVolume = 1f;
            SettingsService.Current.ApplyAudio();

            SoundEffectsVolume.FadeOutGameplay(0.2f, 1f);
            yield return new WaitForSecondsRealtime(0.3f); // Well inside the 1 s delay.
            Assert.AreEqual(0.5f, wheel.volume, 0.001f, "Nothing should change during the delay.");

            yield return new WaitForSecondsRealtime(1.2f);
            Assert.IsFalse(wheel.isPlaying);
        }

        [UnityTest]
        public IEnumerator ResumingGameplay_BringsTheSoundsBack()
        {
            var wheel = MakeLoopingSound("Wheel Sound");
            SettingsService.Current.EffectsVolume = 1f;
            SettingsService.Current.ApplyAudio();
            SoundEffectsVolume.FadeOutGameplay(0.1f);
            yield return new WaitForSecondsRealtime(0.25f);
            Assert.IsFalse(wheel.isPlaying);

            SoundEffectsVolume.ResumeGameplay(0.1f);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.IsTrue(wheel.isPlaying);
            Assert.AreEqual(0.5f, wheel.volume, 0.001f);
        }

        [UnityTest]
        public IEnumerator FadingOutGameplay_LeavesTheMusicAlone()
        {
            SettingsService.Current.MusicVolume = 1f;
            var music = MakeMusic().GetComponent<AudioSource>();
            yield return new WaitForSecondsRealtime(0.3f);

            SoundEffectsVolume.FadeOutGameplay(0.1f);
            yield return new WaitForSecondsRealtime(0.25f);

            Assert.IsTrue(music.isPlaying);
            Assert.AreEqual(1f, music.volume, 0.001f);
        }
    }
}
