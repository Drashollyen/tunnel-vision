using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Added to each control by <see cref="InterfaceSounds"/> when the scene starts. Dropdowns copy
    /// their list items from a template while the menu is open, so the copies get this too. That's
    /// why it looks for the sounds when it needs them instead of being given them up front.
    /// </summary>
    public class InterfaceSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        /// <summary>Greyed out controls and closed dropdown boxes stay quiet on hover. List items don't.</summary>
        public bool PlaysHoverSound => IsInteractable && GetComponent<TMP_Dropdown>() == null;

        public bool IsInteractable
        {
            get
            {
                var selectable = GetComponent<Selectable>();
                return selectable == null || selectable.IsInteractable();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (PlaysHoverSound)
            {
                Sounds()?.PlayHover();
            }
        }

        public void OnPointerClick(PointerEventData eventData) => PlayPress();

        public void OnSubmit(BaseEventData eventData) => PlayPress();

        private void PlayPress()
        {
            if (IsInteractable)
            {
                Sounds()?.PlayClick();
            }
            else
            {
                Sounds()?.PlayDisabledClick();
            }
        }

        private InterfaceSounds Sounds() => GetComponentInParent<InterfaceSounds>(true);
    }
}
