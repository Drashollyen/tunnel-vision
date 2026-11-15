using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TunnelVision.Tests
{
    /// <summary>
    /// The HUD texts that sit over the level are outlined with the "Jersey10 SDF - Outline"
    /// material. The font's shader only draws an outline with OUTLINE_ON, so these check the
    /// prefabs use a material that has it, rather than just an outline width.
    /// </summary>
    public class TextOutlineMaterialTests
    {
        private const string OutlineMaterial = "Assets/_Project/Fonts/Jersey10 SDF - Outline.mat";

        [Test]
        public void OutlineMaterial_HasTheOutlineSwitchedOn()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterial);

            Assert.IsNotNull(material);
            Assert.IsTrue(material.IsKeywordEnabled("OUTLINE_ON"), "OUTLINE_ON should be on.");
            Assert.Greater(material.GetFloat("_OutlineWidth"), 0f);
            Assert.AreEqual(Color.black, material.GetColor("_OutlineColor"));
        }

        [TestCase("Assets/_Project/Prefabs/UI/RoundOverMessage.prefab", "Panel/Title")]
        [TestCase("Assets/_Project/Prefabs/UI/RoundOverMessage.prefab", "Panel/Subtitle")]
        [TestCase("Assets/_Project/Prefabs/UI/HUD.prefab", "Collectible Counter/Count")]
        [TestCase("Assets/_Project/Prefabs/UI/HUD.prefab", "Checkpoint Message")]
        [TestCase("Assets/_Project/Prefabs/UI/HUD.prefab", "Pause Menu/Overlay/Pause Panel/Header/Title")]
        [TestCase("Assets/_Project/Prefabs/UI/HUD.prefab", "Pause Menu/Overlay/Confirm Panel/Header/Title")]
        [TestCase("Assets/_Project/Prefabs/UI/HUD.prefab", "Level Complete/Overlay/Level Complete Panel/Header/Title")]
        public void Text_UsesTheOutlineMaterial(string prefabPath, string textPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var target = prefab.transform.Find(textPath);
            Assert.IsNotNull(target, textPath);

            var text = target.GetComponents<Component>().First(c => c.GetType().Name == "TextMeshProUGUI");
            var material = new SerializedObject(text).FindProperty("m_sharedMaterial").objectReferenceValue;

            Assert.AreEqual(AssetDatabase.LoadAssetAtPath<Material>(OutlineMaterial), material);
        }
    }
}
