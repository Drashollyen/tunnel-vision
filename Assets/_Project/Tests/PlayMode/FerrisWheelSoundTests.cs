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
        private AudioClip _turn, _start, _locked, _notInPrototype, _back, _closeNotice;
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
            _closeNotice = Clip("CloseNotice");

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
            Set(_dialog, "closeSound", _closeNotice);

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
        public IEnumerator QuestionMarkCabin_PlaysTheNoticeSound()
        {
            SelectCabin(3);
            _wheel.StartSelectedLevel();

            CollectionAssert.AreEqual(new[] { _notInPrototype }, _played);
            Assert.IsTrue(_dialog.IsOpen);
            yield break;
        }

        [UnityTest]
        public IEnumerator ClosingTheNotice_PlaysTheCloseSound()
        {
            SelectCabin(3);
            _wheel.StartSelectedLevel();
            _played.Clear();

            _close.onClick.Invoke();

            CollectionAssert.AreEqual(new[] { _closeNotice }, _played);
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
