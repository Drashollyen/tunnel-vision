using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TunnelVision.Editor
{
    /// <summary>
    /// Applies the project's pixel art import settings (see CONTRIBUTING) to the
    /// selected sprites, or to every texture inside the selected folders.
    /// </summary>
    public static class PixelArtImport
    {
        public const int PixelsPerUnit = 16;

        const string MenuPath = "Tools/Tunnel Vision/Apply Pixel Art Settings";

        [MenuItem(MenuPath)]
        static void ApplyToSelection()
        {
            var paths = TexturePathsInSelection();
            if (paths.Count == 0)
            {
                Debug.LogWarning("Select some sprites or a folder in the Project window first.");
                return;
            }

            var changed = 0;
            try
            {
                AssetDatabase.StartAssetEditing();
                for (var i = 0; i < paths.Count; i++)
                {
                    EditorUtility.DisplayProgressBar("Pixel art settings", paths[i], (float)i / paths.Count);
                    if (AssetImporter.GetAtPath(paths[i]) is TextureImporter importer)
                    {
                        Apply(importer);
                        importer.SaveAndReimport();
                        changed++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"Applied pixel art settings to {changed} textures.");
        }

        [MenuItem(MenuPath, true)]
        static bool CanApply() => Selection.assetGUIDs.Length > 0;

        /// <summary>Sets 16 pixels per unit, Point filtering and no compression. Keeps sprite mode and slices.</summary>
        public static void Apply(TextureImporter importer)
        {
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
            }

            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
        }

        static List<string> TexturePathsInSelection()
        {
            var paths = new SortedSet<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.IsValidFolder(path))
                {
                    foreach (var found in AssetDatabase.FindAssets("t:Texture2D", new[] { path }))
                    {
                        paths.Add(AssetDatabase.GUIDToAssetPath(found));
                    }
                }
                else if (AssetImporter.GetAtPath(path) is TextureImporter)
                {
                    paths.Add(path);
                }
            }

            return new List<string>(paths);
        }
    }
}
