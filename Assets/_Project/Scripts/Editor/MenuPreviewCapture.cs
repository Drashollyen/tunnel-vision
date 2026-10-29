using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.Editor
{
    /// <summary>
    /// Renders the MainMenu scene's panels to PNGs (Docs/Screenshots) for PR descriptions and docs.
    /// Works in batch mode too: -executeMethod TunnelVision.Editor.MenuPreviewCapture.CaptureAll
    /// </summary>
    public static class MenuPreviewCapture
    {
        private const string OutputFolder = "Docs/Screenshots";

        [MenuItem("Tunnel Vision/Capture Menu Previews")]
        public static void CaptureAll()
        {
            EditorSceneManager.OpenScene(MainMenuSceneBuilder.MainMenuScenePath);
            var main = GameObject.Find("Canvas/Menu/Main Panel");
            var options = FindInactive("Canvas/Menu", "Options Panel");

            Directory.CreateDirectory(OutputFolder);
            foreach (var size in new[] { new Vector2Int(1920, 1080), new Vector2Int(1280, 1024) })
            {
                main.SetActive(true);
                options.SetActive(false);
                Capture($"{OutputFolder}/main-menu-{size.x}x{size.y}.png", size);

                main.SetActive(false);
                options.SetActive(true);
                Capture($"{OutputFolder}/options-{size.x}x{size.y}.png", size);
            }

            // Discard the temporary panel toggling.
            EditorSceneManager.OpenScene(MainMenuSceneBuilder.MainMenuScenePath);
            Debug.Log($"[Tunnel Vision] Menu previews written to {OutputFolder}.");
        }

        private static void Capture(string path, Vector2Int size)
        {
            var camera = Camera.main;
            var canvas = Object.FindAnyObjectByType<Canvas>();
            var texture = new RenderTexture(size.x, size.y, 24);

            // Overlay canvases don't render through cameras, so temporarily switch to camera space.
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 5f;
            camera.targetTexture = texture;
            camera.aspect = (float)size.x / size.y;

            Canvas.ForceUpdateCanvases();
            foreach (var rect in canvas.GetComponentsInChildren<RectTransform>())
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }

            camera.Render();
            RenderTexture.active = texture;
            var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
            image.Apply();
            File.WriteAllBytes(path, image.EncodeToPNG());

            RenderTexture.active = null;
            camera.targetTexture = null;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(texture);
        }

        private static GameObject FindInactive(string parentPath, string child)
        {
            return GameObject.Find(parentPath).transform.Find(child).gameObject;
        }
    }
}
