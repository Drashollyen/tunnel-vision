using System.Collections;
using UnityEngine;

namespace TunnelVision.UI
{
    /// <summary>Cross-fades from one menu panel to another. Works while the game is paused.</summary>
    public class PanelSwitcher : MonoBehaviour
    {
        private const float FadeSeconds = 0.15f;

        private Coroutine _running;
        private CanvasGroup _fromGroup;
        private CanvasGroup _toGroup;

        public bool IsSwitching => _running != null;

        public void Switch(GameObject from, GameObject to)
        {
            FinishCurrentSwitch();

            _fromGroup = GroupOn(from);
            _toGroup = GroupOn(to);
            _running = StartCoroutine(CrossFade());
        }

        private IEnumerator CrossFade()
        {
            _fromGroup.blocksRaycasts = false;
            _toGroup.alpha = 0f;
            _toGroup.gameObject.SetActive(true);

            // Wait a frame first: the click that started this switch still has to play its sound,
            // and that plays the greyed out click if the button already reads as disabled.
            yield return null;
            _fromGroup.interactable = false;

            for (var elapsed = 0f; elapsed < FadeSeconds; elapsed += Time.unscaledDeltaTime)
            {
                var progress = elapsed / FadeSeconds;
                _fromGroup.alpha = 1f - progress;
                _toGroup.alpha = progress;
                yield return null;
            }

            Complete();
        }

        private void FinishCurrentSwitch()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                Complete();
            }
        }

        private void Complete()
        {
            _fromGroup.gameObject.SetActive(false);
            _fromGroup.alpha = 1f;
            _fromGroup.interactable = true;
            _fromGroup.blocksRaycasts = true;
            _toGroup.alpha = 1f;
            _running = null;
        }

        private static CanvasGroup GroupOn(GameObject panel)
        {
            return panel.TryGetComponent(out CanvasGroup group) ? group : panel.AddComponent<CanvasGroup>();
        }
    }
}
