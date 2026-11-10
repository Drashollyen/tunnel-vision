using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// Scales a button up slightly while it's hovered or selected with the keyboard, and eases back.
    /// Added to every button by InterfaceSounds. Disabled buttons don't react.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ButtonHover : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private const float HoverScale = 1.06f;
        private const float EaseSpeed = 18f;

        private Button _button;
        private Vector3 _restScale;
        private bool _hovered;
        private bool _keyboardSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _restScale = transform.localScale;
        }

        private void OnDisable()
        {
            _hovered = false;
            _keyboardSelected = false;
            transform.localScale = _restScale;
        }

        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;

        public void OnPointerExit(PointerEventData eventData) => _hovered = false;

        // A mouse click selects the button too, and that shouldn't leave it enlarged after the mouse moves away.
        public void OnSelect(BaseEventData eventData) => _keyboardSelected = !(eventData is PointerEventData);

        public void OnDeselect(BaseEventData eventData) => _keyboardSelected = false;

        private void Update()
        {
            var raised = (_hovered || _keyboardSelected) && _button.IsInteractable();
            var target = raised ? _restScale * HoverScale : _restScale;
            var blend = 1f - Mathf.Exp(-EaseSpeed * Time.unscaledDeltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, target, blend);
        }
    }
}
