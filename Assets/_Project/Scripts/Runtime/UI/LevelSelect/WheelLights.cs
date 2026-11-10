using UnityEngine;
using UnityEngine.UI;

namespace TunnelVision.UI
{
    /// <summary>Runs a chase pattern along the bulbs under this object, like fairground lights.</summary>
    public class WheelLights : MonoBehaviour
    {
        private const int BulbsPerGroup = 3;

        [SerializeField] private Color litColor = new Color(1f, 0.92f, 0.5f);
        [SerializeField] private Color dimColor = new Color(0.55f, 0.3f, 0.22f);
        [Tooltip("How many times a second the pattern moves along by one bulb.")]
        [SerializeField] private float stepsPerSecond = 4f;

        private Image[] _bulbs;
        private int _shownStep = -1;

        private void Awake()
        {
            _bulbs = GetComponentsInChildren<Image>();
        }

        private void Update()
        {
            var step = Mathf.FloorToInt(Time.unscaledTime * stepsPerSecond) % BulbsPerGroup;
            if (step == _shownStep)
            {
                return;
            }

            _shownStep = step;
            for (var i = 0; i < _bulbs.Length; i++)
            {
                _bulbs[i].color = i % BulbsPerGroup == step ? litColor : dimColor;
            }
        }
    }
}
