using System.Collections;
using System.Linq;
using System.Reflection;
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
    /// Builds a small wheel and the "not in the prototype" notice in a scene of their own,
    /// so these tests don't depend on the Level Select scene or saved progress.
    /// </summary>
    public class PrototypeNoticeTests
    {
        private NoticeDialog _dialog;
        private TMP_Text _message;
        private Button _close;
        private FerrisWheelSelect _wheel;
        private RectTransform[] _cabins;

        [UnitySetUp]
        public IEnumerator Build()
        {
            yield return IsolatedScene.Create("PrototypeNoticeTest");

            var rig = TestWheel.Build();
            _wheel = rig.Wheel;
            _cabins = rig.Cabins;
            _dialog = rig.Dialog;
            _message = rig.Message;
            _close = rig.Close;
            // Not in the build, so SceneFader only warns and nothing loads.
            Set(_wheel, "levelScenes", Enumerable.Repeat("NoSuchSceneForTests", TestWheel.CabinCount).ToArray());
            Set(_wheel, "levelNames", new[] { "Big Top", "Stalls", "Arcade", "Rides", "House Of Mirrors" });
            rig.SwitchOn();
        }

        [TearDown]
        public void Clean()
        {
            IsolatedScene.Close();
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
            Assert.AreEqual("???", LabelOf(3));
            Assert.AreEqual("???", LabelOf(4));
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

            LogAssert.Expect(LogType.Warning, "FerrisWheelSelect: no Prototype Notice set, so the \"???\" cabin does nothing.");
            _wheel.StartSelectedLevel();
        }

        [UnityTest]
        public IEnumerator UnlockedCabinWithNoLevelYet_OpensTheNotice()
        {
            yield return null;
            var scenes = (string[])typeof(FerrisWheelSelect)
                .GetField("levelScenes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_wheel);
            scenes[0] = string.Empty; // Cabin 1 is always unlocked; pretend its level isn't made yet.
            SelectCabin(0);
            _wheel.StartSelectedLevel();
            yield return null;

            Assert.IsTrue(_dialog.IsOpen);
            Assert.AreEqual(FerrisWheelSelect.NotInPrototypeMessage, _message.text);
        }
    }
}
