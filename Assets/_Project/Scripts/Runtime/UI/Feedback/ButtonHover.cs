using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // Makes a button grow slightly while it's hovered or selected with the keyboard, then shrink
    // back. InterfaceSounds adds this to every button. Disabled buttons don't react.
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

        // Clicking a button selects it too. Only keyboard selection counts, so a clicked button
        // doesn't stay big after the mouse moves away.
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
