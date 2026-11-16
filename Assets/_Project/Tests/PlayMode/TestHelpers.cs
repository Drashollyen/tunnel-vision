using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using TunnelVision.Player;
using TunnelVision.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.Tests
{
    /// <summary>Reads and writes a component's private Inspector fields, the way the scene would set them.</summary>
    public static class Private
    {
        private const BindingFlags Any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static FieldInfo Field(object target, string name)
        {
            // Walk up the class hierarchy, since private fields of a base class aren't found from the subclass.
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(name, Any);
                if (field != null)
                {
                    return field;
                }
            }

            throw new System.MissingFieldException(target.GetType().Name, name);
        }

        public static void Set(object target, string field, object value) => Field(target, field).SetValue(target, value);

        public static T Get<T>(object target, string field) => (T)Field(target, field).GetValue(target);
    }

    /// <summary>Loads a real scene by path in Play mode, even one that isn't in the build.</summary>
    public static class TestScenes
    {
        public static IEnumerator Load(string path)
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                path, new LoadSceneParameters(LoadSceneMode.Single));
#endif
            yield return null;
        }
    }

    /// <summary>
    /// Clears some saved PlayerPrefs (progress, teddies found) for a test and puts the player's real
    /// values back afterwards. Hold in SetUp, Restore in TearDown.
    /// </summary>
    public sealed class SavedPrefs
    {
        private readonly Dictionary<string, int?> _saved = new Dictionary<string, int?>();

        public static SavedPrefs Hold(params string[] keys)
        {
            var held = new SavedPrefs();
            foreach (var key in keys)
            {
                held._saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
                PlayerPrefs.DeleteKey(key);
            }

            return held;
        }

        public void Restore()
        {
            foreach (var pair in _saved)
            {
                if (pair.Value.HasValue)
                {
                    PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                }
                else
                {
                    PlayerPrefs.DeleteKey(pair.Key);
                }
            }
        }
    }

    /// <summary>Bits of a stand-in Nicholas for tests that don't load a real level.</summary>
    public static class TestPlayer
    {
        public const string ProgressKey = "HighestLevelUnlocked";

        /// <summary>
        /// Adds PlayerMovement with the body sprite and eye the real player always has. Call while
        /// <paramref name="player"/> is switched off, so Awake waits until the parts are filled in.
        /// </summary>
        public static PlayerMovement AddMovement(GameObject player, Rigidbody2D body, out SpriteRenderer sprite, out Transform eye)
        {
            sprite = new GameObject("BodySprite").AddComponent<SpriteRenderer>();
            sprite.transform.SetParent(player.transform, false);
            eye = new GameObject("Eyeball").transform;
            eye.SetParent(player.transform, false);

            var movement = player.AddComponent<PlayerMovement>();
            movement.rb = body;
            Private.Set(movement, "bodySprite", sprite);
            Private.Set(movement, "eyeball", eye);
            return movement;
        }

        public static PlayerMovement AddMovement(GameObject player, Rigidbody2D body) => AddMovement(player, body, out _, out _);

        /// <summary>Makes every stage of the death sequence take <paramref name="seconds"/>, so tests don't wait out the real one.</summary>
        public static void ShortenDeath(PlayerRespawn respawn, float seconds)
        {
            foreach (var field in new[] { "revealDuration", "revealHoldDuration", "fadeDuration", "blackHoldDuration" })
            {
                Private.Set(respawn, field, seconds);
            }
        }
    }

    /// <summary>
    /// A small level select wheel with the "not in the prototype" notice, like the real one: five
    /// cabins, each with a Body and a Label, under a canvas. Switched off; tests set what they need,
    /// then switch it on.
    /// </summary>
    public sealed class TestWheel
    {
        public const int CabinCount = 5;

        public FerrisWheelSelect Wheel;
        public RectTransform[] Cabins;
        public NoticeDialog Dialog;
        public TMP_Text Message;
        public Button Close;
        public Transform Canvas;

        public static TestWheel Build()
        {
            var rig = new TestWheel();
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            rig.Canvas = new GameObject("Canvas", typeof(Canvas)).transform;

            var dialogObject = new GameObject("Prototype Notice", typeof(RectTransform), typeof(Image));
            dialogObject.transform.SetParent(rig.Canvas, false);
            dialogObject.SetActive(false);
            rig.Message = new GameObject("Message", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
            rig.Message.transform.SetParent(dialogObject.transform, false);
            rig.Close = new GameObject("Close Button", typeof(RectTransform), typeof(Image), typeof(Button)).GetComponent<Button>();
            rig.Close.transform.SetParent(dialogObject.transform, false);
            rig.Dialog = dialogObject.AddComponent<NoticeDialog>();
            Private.Set(rig.Dialog, "message", rig.Message);
            Private.Set(rig.Dialog, "closeButton", rig.Close);

            var wheelObject = new GameObject("Wheel", typeof(RectTransform));
            wheelObject.transform.SetParent(rig.Canvas, false);
            wheelObject.SetActive(false);
            rig.Cabins = new RectTransform[CabinCount];
            for (var i = 0; i < CabinCount; i++)
            {
                var cabin = new GameObject("Cabin" + (i + 1), typeof(RectTransform));
                cabin.transform.SetParent(wheelObject.transform, false);
                var body = new GameObject("Body", typeof(RectTransform), typeof(Image));
                body.transform.SetParent(cabin.transform, false);
                var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
                label.transform.SetParent(cabin.transform, false);
                label.text = "Level " + (i + 1);
                rig.Cabins[i] = cabin.GetComponent<RectTransform>();
            }

            rig.Wheel = wheelObject.AddComponent<FerrisWheelSelect>();
            Private.Set(rig.Wheel, "cabins", rig.Cabins);
            Private.Set(rig.Wheel, "levelScenes", new string[CabinCount]);
            Private.Set(rig.Wheel, "prototypeLevelCount", 3);
            Private.Set(rig.Wheel, "prototypeNotice", rig.Dialog);
            return rig;
        }

        public void SwitchOn() => Wheel.gameObject.SetActive(true);
    }
}
