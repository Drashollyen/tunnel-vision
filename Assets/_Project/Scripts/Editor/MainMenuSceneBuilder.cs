using System.IO;
using BloodlinesUI;
using TMPro;
using TunnelVision.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.Editor
{
    /// <summary>
    /// Generates the MainMenu and placeholder Game scenes from Bloodlines UI prefabs and registers
    /// them in the build scene list. Run it from the menu bar: Tunnel Vision > Rebuild Menu Scenes.
    /// Re-running overwrites both scenes, so make hand edits in the scene only once you stop using this.
    /// </summary>
    public static class MainMenuSceneBuilder
    {
        public const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        public const string GameScenePath = "Assets/_Project/Scenes/Game.unity";

        private const string Kit = "Assets/ThirdParty/Bloodlines UI/";
        private const string RedButton = Kit + "Prefabs/Button/Button 1 (Red).prefab";
        private const string GrayButton = Kit + "Prefabs/Button/Button 2 (Gray).prefab";
        private const string SliderPrefab = Kit + "Prefabs/Slider (Segments)/Slider 3 (Segments).prefab";
        private const string TogglePrefab = Kit + "Prefabs/Toggle/Toggle 1 (Square).prefab";
        private const string PanelSprite = Kit + "Textures/Frame/Frame_main_menu.png";
        private const string TitleFont = Kit + "Fonts/ManufacturingConsent SDF.asset";
        private const string BodyFont = Kit + "Fonts/MedievalSharp SDF.asset";
        private const string HoverSfx = Kit + "Audio/Hover Button SFX.wav";
        private const string ClickSfx = Kit + "Audio/Click Button SFX.wav";

        private static readonly Color Background = new Color(0.03f, 0.03f, 0.03f, 1f);
        private static readonly Color Blood = new Color(0.66f, 0.05f, 0.0f, 1f);
        private static readonly Color Bone = new Color(0.85f, 0.83f, 0.8f, 1f);

        private const float ButtonWidth = 440f;
        private const float ButtonHeight = 110f;

        [MenuItem("Tunnel Vision/Rebuild Menu Scenes")]
        public static void BuildAll()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MainMenuScenePath));
            BuildGameScene();
            BuildMainMenuScene();

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(GameScenePath, true),
            };
            AssetDatabase.SaveAssets();
            Debug.Log("[Tunnel Vision] Menu scenes rebuilt and added to the build scene list.");
        }

        private static void BuildGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            EditorSceneManager.SaveScene(scene, GameScenePath);
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateEventSystem();

            var canvas = CreateCanvas();
            var menu = new GameObject("Menu", typeof(RectTransform)).GetComponent<RectTransform>();
            menu.SetParent(canvas, false);
            Stretch(menu);
            var controller = menu.gameObject.AddComponent<MainMenuController>();

            // Main panel
            var mainPanel = CreatePanel("Main Panel", menu);
            CreateText("Title", mainPanel, "Tunnel Vision", TitleFont, 110, Blood);
            var play = CreateButton("Play Button", mainPanel, RedButton, "Play");
            var options = CreateButton("Options Button", mainPanel, GrayButton, "Options");
            var quit = CreateButton("Quit Button", mainPanel, GrayButton, "Quit");

            // Options panel
            var optionsPanel = CreatePanel("Options Panel", menu);
            var optionsController = optionsPanel.gameObject.AddComponent<OptionsMenuController>();
            CreateText("Title", optionsPanel, "Options", TitleFont, 90, Blood);
            var masterVolume = CreateLabelledSlider("Master Volume", optionsPanel, "Master volume: {0}%");
            var uiVolume = CreateLabelledSlider("Interface Volume", optionsPanel, "Interface sounds: {0}%");
            var fullscreen = CreateLabelledToggle("Fullscreen", optionsPanel, "Fullscreen");
            var uiSounds = CreateLabelledToggle("Interface Sounds", optionsPanel, "Interface sounds");
            var footer = CreateRow("Footer", optionsPanel, 30f);
            var reset = CreateButton("Reset Button", footer, GrayButton, "Reset", 300f);
            var back = CreateButton("Back Button", footer, RedButton, "Back", 300f);

            // Bloodlines UI sound manager: plays hover/click SFX and owns the interface volume + on/off.
            CreateSoundManager(uiVolume, uiSounds);

            Wire(controller, "gameSceneName", "Game");
            Wire(controller, "mainPanel", mainPanel.gameObject);
            Wire(controller, "optionsPanel", optionsPanel.gameObject);
            Wire(controller, "mainFirstSelected", play);
            Wire(controller, "optionsFirstSelected", masterVolume);
            Wire(optionsController, "masterVolumeSlider", masterVolume);
            Wire(optionsController, "fullscreenToggle", fullscreen);

            AddClick(play, controller.Play);
            AddClick(options, controller.OpenOptions);
            AddClick(quit, controller.Quit);
            AddClick(back, controller.CloseOptions);
            AddClick(reset, optionsController.ResetToDefaults);

            optionsPanel.gameObject.SetActive(false);
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        }

        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            go.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetAsLastSibling();
        }

        private static RectTransform CreateCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

            // Scale with the screen so the menu looks the same at any resolution.
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var root = (RectTransform)go.transform;
            var background = new GameObject("Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root, false);
            Stretch((RectTransform)background.transform);
            background.GetComponent<Image>().color = Background;
            return root;
        }

        /// <summary>
        /// A centered, 9-sliced frame that grows and shrinks to fit whatever is placed inside it.
        /// </summary>
        private static RectTransform CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PanelSprite);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 0.5f;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(110, 110, 80, 90);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
        }

        private static RectTransform CreateRow(string name, Transform parent, float spacing)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, string fontPath, float size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string prefabPath, string label, float width = ButtonWidth)
        {
            var go = InstantiatePrefab(prefabPath, parent, name);
            SetPreferredSize(go, width, ButtonHeight);

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 52;
            text.textWrappingMode = TextWrappingModes.NoWrap;

            var sfx = go.GetComponent<ButtonSFX>();
            if (sfx != null)
            {
                Wire(sfx, "hoverSound", AssetDatabase.LoadAssetAtPath<AudioClip>(HoverSfx));
                Wire(sfx, "clickSound", AssetDatabase.LoadAssetAtPath<AudioClip>(ClickSfx));
            }

            return go.GetComponent<Button>();
        }

        private static Slider CreateLabelledSlider(string name, Transform parent, string format)
        {
            var group = new GameObject(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
            group.transform.SetParent(parent, false);
            var layout = group.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var label = CreateText("Label", group.transform, string.Format(format, 80), BodyFont, 40, Bone);
            label.alignment = TextAlignmentOptions.Left;

            var go = InstantiatePrefab(SliderPrefab, group.transform, "Slider");
            SetPreferredSize(go, 620f, 46f);
            var slider = go.GetComponent<Slider>();

            var sync = group.AddComponent<SliderTextSynchronizer>();
            Wire(sync, "slider", slider);
            Wire(sync, "textComponent", label);
            Wire(sync, "textFormat", format);
            return slider;
        }

        private static Toggle CreateLabelledToggle(string name, Transform parent, string labelText)
        {
            var row = CreateRow(name, parent, 24f);
            var go = InstantiatePrefab(TogglePrefab, row, "Toggle");
            SetPreferredSize(go, 50f, 50f);
            var label = CreateText("Label", row, labelText, BodyFont, 40, Bone);
            label.alignment = TextAlignmentOptions.Left;
            SetPreferredSize(label.gameObject, 546f, -1f);
            return go.GetComponent<Toggle>();
        }

        private static void CreateSoundManager(Slider volume, Toggle enabled)
        {
            var go = new GameObject("Sound Manager", typeof(AudioSource), typeof(SoundManager));
            var source = go.GetComponent<AudioSource>();
            source.playOnAwake = false;
            var manager = go.GetComponent<SoundManager>();
            Wire(manager, "audioSource", source);
            Wire(manager, "volumeSlider", volume);
            Wire(manager, "soundToggle", enabled);
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new FileNotFoundException($"Bloodlines UI prefab not found: {path}");
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            return go;
        }

        private static void SetPreferredSize(GameObject go, float width, float height)
        {
            if (!go.TryGetComponent<LayoutElement>(out var element))
            {
                element = go.AddComponent<LayoutElement>();
            }


            element.preferredWidth = width;
            element.preferredHeight = height;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void AddClick(Button button, UnityAction action)
        {
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Wire(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
