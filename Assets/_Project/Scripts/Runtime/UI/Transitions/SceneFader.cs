using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Every scene change goes through here: fade to black, load, fade back in.
    /// It builds itself the first time it's used, so scenes don't need to include it.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        private const float FadeSeconds = 0.25f;
        private const int SortOrderAboveEverything = short.MaxValue;

        private static SceneFader _instance;

        private Image _overlay;
        private bool _fading;

        public static bool IsFading => _instance != null && _instance._fading;

        public static void Load(string sceneName)
        {
            if (IsFading)
            {
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning($"SceneFader: {sceneName} isn't in the build's scene list.");
                return;
            }

            if (_instance == null)
            {
                _instance = Create();
            }

            _instance.StartCoroutine(_instance.FadeAndLoad(sceneName));
        }

        private static SceneFader Create()
        {
            var root = new GameObject("Scene Fader");
            DontDestroyOnLoad(root);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortOrderAboveEverything;
            root.AddComponent<GraphicRaycaster>();

            var overlayObject = new GameObject("Overlay");
            overlayObject.transform.SetParent(root.transform, false);
            var rect = overlayObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var fader = root.AddComponent<SceneFader>();
            fader._overlay = overlayObject.AddComponent<Image>();
            fader._overlay.color = Color.clear;
            fader._overlay.raycastTarget = false;
            return fader;
        }

        private IEnumerator FadeAndLoad(string sceneName)
        {
            _fading = true;
            _overlay.raycastTarget = true; // Blocks clicks until the new scene is showing.

            yield return Fade(0f, 1f);
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Fade(1f, 0f);

            _overlay.raycastTarget = false;
            _fading = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            for (var elapsed = 0f; elapsed < FadeSeconds; elapsed += Time.unscaledDeltaTime)
            {
                SetAlpha(Mathf.Lerp(from, to, elapsed / FadeSeconds));
                yield return null;
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            _overlay.color = new Color(0f, 0f, 0f, alpha);
        }
    }
}
