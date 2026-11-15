using System.Collections;
using NUnit.Framework;
using TMPro;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace TunnelVision.Tests
{
    public class TextOutlineTests
    {
        private const string FontPath = "Assets/_Project/Fonts/Jersey10 SDF.asset";

        private GameObject _canvas;

        [TearDown]
        public void Clean()
        {
            Object.Destroy(_canvas);
        }

        private TMP_Text MakeText(bool outlineFirst)
        {
            _canvas = new GameObject("Canvas", typeof(Canvas));
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(_canvas.transform, false);
            go.SetActive(false);

            // The order components are added in decides whose Awake runs first.
            if (outlineFirst)
            {
                go.AddComponent<TextOutline>();
            }

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (!outlineFirst)
            {
                go.AddComponent<TextOutline>();
            }

            go.SetActive(true);
            return tmp;
        }

        [UnityTest]
        public IEnumerator Outline_IsAppliedToTheText()
        {
            var tmp = MakeText(outlineFirst: false);
            yield return null;

            var outline = tmp.GetComponent<TextOutline>();
            Assert.AreEqual(outline.Width, tmp.outlineWidth, 0.001f);
            Assert.AreEqual((Color32)outline.Color, tmp.outlineColor);
            Assert.Greater(tmp.outlineWidth, 0f, "The outline should be visible by default.");
        }

        [UnityTest]
        public IEnumerator Outline_WorksWhenItsComponentWakesFirst()
        {
            // This order used to throw in TextMeshPro and leave the text without an outline.
            var tmp = MakeText(outlineFirst: true);
            yield return null;

            Assert.Greater(tmp.outlineWidth, 0f);
            Assert.Greater(tmp.fontMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth), 0f,
                "The outline should reach the material the text draws with.");
        }
    }
}
