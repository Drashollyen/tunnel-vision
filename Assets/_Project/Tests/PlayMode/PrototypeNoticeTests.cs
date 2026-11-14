using System.Collections;
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
    /// Builds a small wheel and the "not in the prototype" notice in a scene of their own,
    /// so these tests don't depend on the Level Select scene or saved progress.
    /// </summary>
    public class PrototypeNoticeTests
    {
        private const int CabinCount = 5;

        private NoticeDialog _dialog;
        private TMP_Text _message;
        private Button _close;
        private FerrisWheelSelect _wheel;
        private RectTransform[] _cabins;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("PrototypeNoticeTest");

            new GameObject("EventSystem", typeof(EventSystem));
            var canvas = new GameObject("Canvas", typeof(Canvas)).transform;

            // The dialog: a dim background with a message and a close button.
            var dialogObject = new GameObject("Prototype Notice", typeof(RectTransform), typeof(Image));
            dialogObject.transform.SetParent(canvas, false);
            dialogObject.SetActive(false);
            _message = new GameObject("Message", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            _message.transform.SetParent(dialogObject.transform, false);
            _close = new GameObject("Close Button", typeof(RectTransform), typeof(Image), typeof(Button))
                .GetComponent<Button>();
            _close.transform.SetParent(dialogObject.transform, false);
            _dialog = dialogObject.AddComponent<NoticeDialog>();
            Set(_dialog, "message", _message);
            Set(_dialog, "closeButton", _close);

            // The wheel: five cabins, each with a Body and a Label like the real ones.
            var wheelObject = new GameObject("Wheel", typeof(RectTransform));
            wheelObject.transform.SetParent(canvas, false);
            wheelObject.SetActive(false);
            _cabins = new RectTransform[CabinCount];
            var scenes = new string[CabinCount];
            for (var i = 0; i < CabinCount; i++)
            {
                var cabin = new GameObject("Cabin" + (i + 1), typeof(RectTransform));
                cabin.transform.SetParent(wheelObject.transform, false);
                var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
                body.transform.SetParent(cabin.transform, false);
                var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
                label.transform.SetParent(cabin.transform, false);
                label.text = "Level " + (i + 1);
                _cabins[i] = cabin.GetComponent<RectTransform>();
                scenes[i] = string.Empty; // Nothing loads, even for a playable cabin.
            }

            _wheel = wheelObject.AddComponent<FerrisWheelSelect>();
            Set(_wheel, "cabins", _cabins);
            Set(_wheel, "levelScenes", scenes);
            Set(_wheel, "levelNames", new[] { "Big Top", "Stalls", "Arcade", "Rides", "House Of Mirrors" });
            Set(_wheel, "prototypeLevelCount", 3);
            Set(_wheel, "prototypeNotice", _dialog);
            wheelObject.SetActive(true);
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
        }

        private static void Set(object target, string field, object value)
        {
            target.GetType()
                .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private void SelectCabin(int index)
        {
            Set(_wheel, "_steps", index);
        }

        private string LabelOf(int cabin) => _cabins[cabin].Find("Label").GetComponent<TMP_Text>().text;

        [UnityTest]
        public IEnumerator CabinsPastThePrototype_ShowQuestionMarks()
        {
            yield return null; // Let Start run.

            Assert.AreEqual("Level 1", LabelOf(0));
            Assert.AreEqual("Level 3", LabelOf(2));
            Assert.AreEqual("?", LabelOf(3));
            Assert.AreEqual("?", LabelOf(4));
        }

        [UnityTest]
        public IEnumerator Notice_StartsHidden()
        {
            yield return null;

            Assert.IsFalse(_dialog.IsOpen);
            Assert.IsFalse(_dialog.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator PickingAQuestionMarkCabin_OpensTheNotice()
        {
            yield return null;
            SelectCabin(3);
            _wheel.StartSelectedLevel();
            yield return null;

            Assert.IsTrue(_dialog.IsOpen);
            Assert.IsTrue(_dialog.gameObject.activeInHierarchy);
            Assert.AreEqual(FerrisWheelSelect.NotInPrototypeMessage, _message.text);
            Assert.AreEqual(_close.gameObject, EventSystem.current.currentSelectedGameObject,
                "The close button should be selected so Enter closes it.");
        }

        [UnityTest]
        public IEnumerator PickingAPrototypeCabin_DoesNotOpenTheNotice()
        {
            yield return null;
            SelectCabin(0);
            _wheel.StartSelectedLevel();
            yield return null;

            Assert.IsFalse(_dialog.IsOpen);
        }

        [UnityTest]
        public IEnumerator CloseButton_HidesTheNotice()
        {
            yield return null;
            SelectCabin(4);
            _wheel.StartSelectedLevel();
            yield return null;

            _close.onClick.Invoke();

            Assert.IsFalse(_dialog.IsOpen);
            Assert.IsFalse(_dialog.gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator WheelDoesNotTurn_WhileTheNoticeIsOpen()
        {
            yield return null;
            SelectCabin(3);
            _wheel.StartSelectedLevel();
            var rotationWhenOpened = _wheel.transform.localRotation;

            // The wheel was told to point at cabin 4 but the notice blocks its Update.
            yield return new WaitForSecondsRealtime(0.5f);

            Assert.AreEqual(rotationWhenOpened, _wheel.transform.localRotation);
        }

        [UnityTest]
        public IEnumerator WithoutANotice_QuestionMarkCabinOnlyWarns()
        {
            yield return null;
            Set(_wheel, "prototypeNotice", null);
            SelectCabin(3);

            LogAssert.Expect(LogType.Warning, "FerrisWheelSelect: no Prototype Notice set, so the \"?\" cabin does nothing.");
            _wheel.StartSelectedLevel();
        }
    }
}
