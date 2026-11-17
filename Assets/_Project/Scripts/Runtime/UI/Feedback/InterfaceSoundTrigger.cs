using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Plays the hover and click sounds for one control. InterfaceSounds adds it to every control
    // when the scene starts. A dropdown copies its list items while it's open, so the copies get
    // this too; that's why it looks for the sounds when it needs them instead of storing them.
    public class InterfaceSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        // Greyed out controls and closed dropdown boxes stay quiet on hover. Dropdown list items don't.
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
