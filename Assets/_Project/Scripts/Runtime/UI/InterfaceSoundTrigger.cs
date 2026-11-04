using UnityEngine;
using UnityEngine.EventSystems;

namespace TunnelVision.UI
{
    /// <summary>Added to each control by <see cref="InterfaceSounds"/> when the scene starts.</summary>
    public class InterfaceSoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        private InterfaceSounds _sounds;

        public void Initialize(InterfaceSounds sounds)
        {
            _sounds = sounds;
        }

        public void OnPointerEnter(PointerEventData eventData) => _sounds.PlayHover();

        public void OnPointerClick(PointerEventData eventData) => _sounds.PlayClick();

        public void OnSubmit(BaseEventData eventData) => _sounds.PlayClick();
    }
}
