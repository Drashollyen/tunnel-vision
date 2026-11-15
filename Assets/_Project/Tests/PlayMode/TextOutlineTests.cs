using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;

namespace TunnelVision.Tests
{
    public class TextOutlineTests
    {
        private GameObject _text;

        [TearDown]
        public void Clean()
        {
            Object.Destroy(_text);
        }

        [Test]
        public void Outline_IsAppliedToTheText()
        {
            _text = new GameObject("Text", typeof(RectTransform));
            var tmp = _text.AddComponent<TextMeshProUGUI>();
            var outline = _text.AddComponent<TextOutline>();

            Assert.AreEqual(outline.Width, tmp.outlineWidth, 0.001f);
            Assert.AreEqual((Color32)outline.Color, tmp.outlineColor);
            Assert.Greater(tmp.outlineWidth, 0f, "The outline should be visible by default.");
        }
    }
}
