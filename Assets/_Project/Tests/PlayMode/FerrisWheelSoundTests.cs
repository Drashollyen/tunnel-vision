using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static TunnelVision.Tests.Private;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Checks the level select wheel plays the right sound for each action. Builds its own
    /// small wheel and notice, and listens to InterfaceSounds.ClipPlayed to see what went off.
    /// </summary>
    public class FerrisWheelSoundTests
    {

        private FerrisWheelSelect _wheel;
        private NoticeDialog _dialog;
        private Button _close;
        private AudioClip _turn, _start, _locked, _notInPrototype, _back, _menuClick;
        private readonly List<AudioClip> _played = new List<AudioClip>();
        private SavedPrefs _progress;

        [UnitySetUp]
        public IEnumerator Build()
        {
            // Only cabin 1 is unlocked, whatever this machine has saved.
            _progress = SavedPrefs.Hold(TestPlayer.ProgressKey);

            yield return IsolatedScene.Create("FerrisWheelSoundTest");

            _turn = Clip("Turn");
            _start = Clip("Start");
            _locked = Clip("Locked");
            _notInPrototype = Clip("NotInPrototype");
            _back = Clip("Back");
            _menuClick = Clip("MenuClick");

            var rig = TestWheel.Build(); // Its cabins have no scenes, so nothing loads.
            _wheel = rig.Wheel;
            _dialog = rig.Dialog;
            _close = rig.Close;
            // The menu's button sounds, which also give the OK button its click.
            var sounds = rig.Canvas.gameObject.AddComponent<InterfaceSounds>();
            Set(sounds, "clickClip", _menuClick);

            Set(_wheel, "turnSound", _turn);
            Set(_wheel, "startSound", _start);
            Set(_wheel, "lockedSound", _locked);
            Set(_wheel, "notInPrototypeSound", _notInPrototype);
            Set(_wheel, "backSound", _back);
            rig.SwitchOn();

            _played.Clear();
            InterfaceSounds.ClipPlayed += Record;
            yield return null; // Let Start run.
        }

        [TearDown]
        public void Clean()
        {
            InterfaceSounds.ClipPlayed -= Record;
            IsolatedScene.Close();
            _progress.Restore();
        }

        private void Record(AudioClip clip) => _played.Add(clip);

        private static AudioClip Clip(string name) => AudioClip.Create(name, 441, 1, 44100, false);

        private void SelectCabin(int index)
        {
            Set(_wheel, "_steps", index);
            Set(_wheel, "_angle", index * 360f / TestWheel.CabinCount);
        }

        [UnityTest]
        public IEnumerator TurningOneCabin_Ticks()
        {
            _wheel.Turn(1);

            CollectionAssert.AreEqual(new[] { _turn }, _played);
            yield break;
        }

        [UnityTest]
        public IEnumerator TurningPastWhatTheWheelAllows_StaysSilent()
        {
            // The wheel hasn't moved yet, so only one step ahead is allowed.
            _wheel.Turn(1);
            _wheel.Turn(1);

            CollectionAssert.AreEqual(new[] { _turn }, _played);
            yield break;
        }

        [UnityTest]
        public IEnumerator LockedCabin_Buzzes()
        {
            SelectCabin(1);
            _wheel.StartSelectedLevel();

            CollectionAssert.AreEqual(new[] { _locked }, _played);
            yield break;
        }

        [UnityTest]
        public IEnumerator LockedCabin_RocksThenSettles()
        {
            SelectCabin(1);
            var cabin = _wheel.transform.GetChild(1);
            _wheel.StartSelectedLevel();
            Assert.AreEqual(1, _wheel.WobblingCabin);

            // Partway through, it's tipped away from the upright angle the wheel gives it.
            var tipped = false;
            var until = Time.unscaledTime + FerrisWheelSelect.WobbleSeconds;
            while (Time.unscaledTime < until)
            {
                var upright = Quaternion.Euler(0f, 0f, -_wheel.transform.localEulerAngles.z);
                tipped |= Quaternion.Angle(upright, cabin.localRotation) > 1f;
                yield return null;
            }

            yield return null;
            Assert.IsTrue(tipped, "The locked cabin should visibly rock.");
            Assert.AreEqual(-1, _wheel.WobblingCabin);
            var settled = Quaternion.Euler(0f, 0f, -_wheel.transform.localEulerAngles.z);
            Assert.Less(Quaternion.Angle(settled, cabin.localRotation), 0.01f, "It should end upright.");
        }

        [UnityTest]
        public IEnumerator QuestionMarkCabin_PlaysTheNoticeSound_WithoutRocking()
        {
            // That the notice opens is PrototypeNoticeTests' job; this checks how it sounds and moves.
            SelectCabin(3);
            _wheel.StartSelectedLevel();

            CollectionAssert.AreEqual(new[] { _notInPrototype }, _played);
            Assert.AreEqual(-1, _wheel.WobblingCabin, "Only locked cabins rock.");
            yield break;
        }

        [UnityTest]
        public IEnumerator ClosingTheNotice_PlaysTheMenuClick()
        {
            SelectCabin(3);
            _wheel.StartSelectedLevel();
            _played.Clear();

            // What Escape does: press OK, with the same click as the menu's buttons.
            _dialog.PressClose();

            CollectionAssert.AreEqual(new[] { _menuClick }, _played);
            Assert.IsFalse(_dialog.IsOpen);
            yield break;
        }

        [UnityTest]
        public IEnumerator PlayableCabinWithoutAScene_PlaysTheNotAvailableSound()
        {
            // Cabin 1 is playable, but its level isn't made yet, so it's treated like "???".
            SelectCabin(0);
            _wheel.StartSelectedLevel();

            CollectionAssert.AreEqual(new[] { _notInPrototype }, _played);
            yield break;
        }
    }
}
