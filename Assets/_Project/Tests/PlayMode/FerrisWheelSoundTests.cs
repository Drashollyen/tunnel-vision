using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>
    /// Checks the level select wheel plays the right sound for each action. Builds its own
    /// small wheel and notice, and listens to InterfaceSounds.ClipPlayed to see what went off.
    /// </summary>
    public class FerrisWheelSoundTests
    {
        private const int CabinCount = 5;
        private const string ProgressKey = "HighestLevelUnlocked";

        private FerrisWheelSelect _wheel;
        private NoticeDialog _dialog;
        private Button _close;
        private AudioClip _turn, _start, _locked, _notInPrototype, _back, _menuClick;
        private readonly List<AudioClip> _played = new List<AudioClip>();
        private bool _hadProgress;
        private int _savedProgress;

        [UnitySetUp]
        public IEnumerator Build()
        {
            // Only cabin 1 is unlocked, whatever this machine has saved.
            _hadProgress = PlayerPrefs.HasKey(ProgressKey);
            _savedProgress = PlayerPrefs.GetInt(ProgressKey, 0);
            PlayerPrefs.SetInt(ProgressKey, 0);

            yield return IsolatedScene.Create("FerrisWheelSoundTest");

            _turn = Clip("Turn");
            _start = Clip("Start");
            _locked = Clip("Locked");
            _notInPrototype = Clip("NotInPrototype");
            _back = Clip("Back");
            _menuClick = Clip("MenuClick");

            new GameObject("EventSystem", typeof(EventSystem));
            var canvas = new GameObject("Canvas", typeof(Canvas)).transform;

            var dialogObject = new GameObject("Prototype Notice", typeof(RectTransform), typeof(Image));
            dialogObject.transform.SetParent(canvas, false);
            dialogObject.SetActive(false);
            _close = new GameObject("Close Button", typeof(RectTransform), typeof(Image), typeof(Button))
                .GetComponent<Button>();
            _close.transform.SetParent(dialogObject.transform, false);
            _dialog = dialogObject.AddComponent<NoticeDialog>();
            Set(_dialog, "closeButton", _close);
            // The menu's button sounds, which also give the OK button its click.
            var sounds = canvas.gameObject.AddComponent<InterfaceSounds>();
            Set(sounds, "clickClip", _menuClick);

            var wheelObject = new GameObject("Wheel", typeof(RectTransform));
            wheelObject.transform.SetParent(canvas, false);
            wheelObject.SetActive(false);
            var cabins = new RectTransform[CabinCount];
            for (var i = 0; i < CabinCount; i++)
            {
                var cabin = new GameObject("Cabin" + (i + 1), typeof(RectTransform));
                cabin.transform.SetParent(wheelObject.transform, false);
                var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
                label.transform.SetParent(cabin.transform, false);
                cabins[i] = cabin.GetComponent<RectTransform>();
            }

            _wheel = wheelObject.AddComponent<FerrisWheelSelect>();
            Set(_wheel, "cabins", cabins);
            Set(_wheel, "levelScenes", new string[CabinCount]); // Nothing loads.
            Set(_wheel, "prototypeLevelCount", 3);
            Set(_wheel, "prototypeNotice", _dialog);
            Set(_wheel, "turnSound", _turn);
            Set(_wheel, "startSound", _start);
            Set(_wheel, "lockedSound", _locked);
            Set(_wheel, "notInPrototypeSound", _notInPrototype);
            Set(_wheel, "backSound", _back);
            wheelObject.SetActive(true);

            _played.Clear();
            InterfaceSounds.ClipPlayed += Record;
            yield return null; // Let Start run.
        }

        [TearDown]
        public void Clean()
        {
            InterfaceSounds.ClipPlayed -= Record;
            IsolatedScene.Close();

            if (_hadProgress)
            {
                PlayerPrefs.SetInt(ProgressKey, _savedProgress);
            }
            else
            {
                PlayerPrefs.DeleteKey(ProgressKey);
            }
        }

        private void Record(AudioClip clip) => _played.Add(clip);

        private static AudioClip Clip(string name) => AudioClip.Create(name, 441, 1, 44100, false);

        private static void Set(object target, string field, object value)
        {
            target.GetType()
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private void SelectCabin(int index)
        {
            Set(_wheel, "_steps", index);
            Set(_wheel, "_angle", index * 360f / CabinCount);
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
        public IEnumerator QuestionMarkCabin_DoesNotRock()
        {
            SelectCabin(3);
            _wheel.StartSelectedLevel();

            Assert.AreEqual(-1, _wheel.WobblingCabin);
            yield break;
        }

        [UnityTest]
        public IEnumerator QuestionMarkCabin_PlaysTheNoticeSound()
        {
            SelectCabin(3);
            _wheel.StartSelectedLevel();

            CollectionAssert.AreEqual(new[] { _notInPrototype }, _played);
            Assert.IsTrue(_dialog.IsOpen);
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
        public IEnumerator PlayableCabinWithoutAScene_StaysSilent()
        {
            // Cabin 1 is playable, but nothing would load, so there's nothing to confirm.
            SelectCabin(0);
            _wheel.StartSelectedLevel();

            CollectionAssert.IsEmpty(_played);
            yield break;
        }
    }
}
