using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class NumberDropTests
    {
        private GameObject _canvas;
        private TMP_Text _text;
        private NumberDrop _drop;

        [SetUp]
        public void Build()
        {
            _canvas = new GameObject("Canvas", typeof(Canvas));
            var line = new GameObject("Line", typeof(RectTransform));
            line.transform.SetParent(_canvas.transform, false);
            _text = line.AddComponent<TextMeshProUGUI>();
            _drop = line.AddComponent<NumberDrop>();
        }

        [TearDown]
        public void Clean()
        {
            Object.Destroy(_canvas);
        }

        private TMP_Text Overlay(string name) => _text.transform.Find(name).GetComponent<TMP_Text>();

        [Test]
        public void Split_SeparatesTheLeadingNumber()
        {
            Assert.AreEqual(("2", " Lives Remaining"), NumberDrop.Split("2 Lives Remaining"));
            Assert.AreEqual(("7", ""), NumberDrop.Split("7"));
        }

        [UnityTest]
        public IEnumerator AtFirst_OldNumberShowsAndNewOneIsHidden()
        {
            _drop.Play("3 Lives Remaining", "2 Lives Remaining");
            yield return null;

            Assert.IsTrue(_drop.IsAnimating);
            Assert.IsTrue(Overlay("Old Number").gameObject.activeSelf);
            Assert.AreEqual(1f, Overlay("Old Number").alpha, 0.01f);
            Assert.AreEqual(0f, Overlay("New Number").alpha, 0.01f);
            Assert.AreEqual("2 Lives Remaining", _text.GetParsedText(), "The line itself is already right.");
        }

        [UnityTest]
        public IEnumerator Midway_OldNumberHasDropped()
        {
            _drop.Play("3 Lives Remaining", "2 Lives Remaining");
            yield return new WaitForSecondsRealtime(NumberDrop.Pause + NumberDrop.FallSeconds * 0.6f);

            var old = Overlay("Old Number");
            Assert.Less(old.rectTransform.anchoredPosition.y, -5f, "The old number should be falling.");
            Assert.Less(old.alpha, 1f, "The old number should be fading.");
        }

        [UnityTest]
        public IEnumerator Afterwards_LineSettlesWithTheNewNumber()
        {
            _drop.Play("3 Lives Remaining", "2 Lives Remaining");
            yield return new WaitForSecondsRealtime(NumberDrop.TotalSeconds + 0.1f);

            Assert.IsFalse(_drop.IsAnimating);
            Assert.AreEqual("2 Lives Remaining", _text.text);
            Assert.IsFalse(Overlay("Old Number").gameObject.activeSelf);
            Assert.IsFalse(Overlay("New Number").gameObject.activeSelf);
        }

        [UnityTest]
        public IEnumerator ShowPlain_StopsTheAnimation()
        {
            _drop.Play("3 Lives Remaining", "2 Lives Remaining");
            yield return null;

            _drop.ShowPlain("Press Enter For Level Select");
            yield return null;

            Assert.AreEqual("Press Enter For Level Select", _text.text);
            Assert.IsFalse(Overlay("Old Number").gameObject.activeSelf);
            Assert.IsFalse(Overlay("New Number").gameObject.activeSelf);
        }
    }
}
