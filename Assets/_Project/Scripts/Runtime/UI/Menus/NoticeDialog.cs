using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    // A simple pop-up message, closed with its button, Enter or Escape. Starts hidden.
    public class NoticeDialog : MonoBehaviour
    {
        [Tooltip("Optional. Its text is replaced by whatever is passed to Show.")]
        [SerializeField] private TMP_Text message;
        [Tooltip("Optional. Closes the dialog, and is selected while it's open so Enter works.")]
        [SerializeField] private Button closeButton;

        private int _openedOnFrame = -1;

        public bool IsOpen { get; private set; }

        // Lets other scripts ignore the same key press that just closed this dialog.
        public int ClosedOnFrame { get; private set; } = -1;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Hide);
            }

            // Awake only runs the first time the object is switched on, which may be from Show.
            // In that case it should stay visible.
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
            // Ignore keys on the frame it opened, so the key that opened it doesn't close it again.
            var keyboard = Keyboard.current;
            if (!IsOpen || keyboard == null || Time.frameCount == _openedOnFrame)
            {
                return;
            }

            // When the button is selected, Unity already presses it on Enter, so don't do it twice.
            var enterHandledByButton = closeButton != null && EventSystem.current != null
                && EventSystem.current.currentSelectedGameObject == closeButton.gameObject;

            if (keyboard.escapeKey.wasPressedThisFrame
                || (!enterHandledByButton
                    && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)))
            {
                PressClose();
            }
        }

        // Closes the dialog as if its button was clicked, so keys get the same click sound.
        public void PressClose()
        {
            if (closeButton != null && closeButton.isActiveAndEnabled && EventSystem.current != null)
            {
                ExecuteEvents.Execute(closeButton.gameObject, new BaseEventData(EventSystem.current),
                    ExecuteEvents.submitHandler);
            }

            // Still closes if there's no button, or the button didn't close it.
            Hide();
        }
    }
}
