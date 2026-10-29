using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Plays the hover sound when the pointer enters this control or keyboard/gamepad navigation
    /// moves to it, and the click sound when it is clicked or submitted.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class UiSelectableSounds : MonoBehaviour, IPointerEnterHandler, ISelectHandler, IPointerClickHandler, ISubmitHandler
    {
        private Selectable _selectable;

        private bool Interactable => _selectable.IsInteractable();

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Interactable)
            {
                PlayHover();
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            // Only navigation moves carry axis data; skip selections made by code (e.g. when a panel opens).
            if (Interactable && eventData is AxisEventData)
            {
                PlayHover();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Interactable && eventData.button == PointerEventData.InputButton.Left)
            {
                PlayClick();
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (Interactable)
            {
                PlayClick();
            }
        }

        private static void PlayHover()
        {
            var player = UiSoundPlayer.Instance;
            if (player != null)
            {
                player.PlayHover();
            }
        }

        private static void PlayClick()
        {
            var player = UiSoundPlayer.Instance;
            if (player != null)
            {
                player.PlayClick();
            }
        }
    }
}
