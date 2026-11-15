using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>
    /// A simple message box over a dimmed screen, closed with its button, Enter or Escape.
    /// Goes on the dialog's root object: a full-screen dim image with the box as a child.
    /// The root starts hidden.
    /// </summary>
    public class NoticeDialog : MonoBehaviour
    {
        [Tooltip("Optional. Replaced by the text passed to Show, if any.")]
        [SerializeField] private TMP_Text message;
        [Tooltip("Optional. Closes the dialog, and is selected while it's open so Enter works.")]
        [SerializeField] private Button closeButton;

        private int _openedOnFrame = -1;

        public bool IsOpen { get; private set; }

        /// <summary>The frame the dialog last closed on, so a key that closed it isn't used twice.</summary>
        public int ClosedOnFrame { get; private set; } = -1;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            if (!IsOpen)
            {
                gameObject.SetActive(false);
            }
        }

        public void Show(string text = null)
        {
            if (message != null && !string.IsNullOrEmpty(text))
            {
                message.text = text;
            }

            IsOpen = true;
            _openedOnFrame = Time.frameCount;
            gameObject.SetActive(true);

            if (closeButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
            }
        }

        public void Hide()
        {
            if (!IsOpen)
            {
                return;
            }

            IsOpen = false;
            ClosedOnFrame = Time.frameCount;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            // The key press that opened the dialog mustn't close it again straight away.
            var keyboard = Keyboard.current;
            if (!IsOpen || keyboard == null || Time.frameCount == _openedOnFrame)
            {
                return;
            }

            // With the button selected, Enter already presses it through the EventSystem.
            var enterHandledByButton = closeButton != null && EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject == closeButton.gameObject;

            if (keyboard.escapeKey.wasPressedThisFrame
                || (!enterHandledByButton
                    && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)))
            {
                PressClose();
            }
        }

        /// <summary>
        /// Closes the dialog as if its button were pressed, so a key gets the same click sound
        /// and effects as the button. Without a button it just hides.
        /// </summary>
        public void PressClose()
        {
            if (closeButton != null && closeButton.isActiveAndEnabled && EventSystem.current != null)
            {
                ExecuteEvents.Execute(closeButton.gameObject, new BaseEventData(EventSystem.current),
                    ExecuteEvents.submitHandler);
            }

            // Covers a greyed out button, or one with nothing listening.
            Hide();
        }
    }
}
