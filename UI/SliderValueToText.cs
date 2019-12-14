using UnityEngine;
using UnityEngine.UI;

namespace UnityHelpers
{
    [ExecuteAlways]
    public class SliderValueToText : MonoBehaviour
    {
        public bool percent;

        public TMPro.TextMeshProUGUI label;
        public string prefix;
        public string postfix;

        private void Start()
        {
            var slider = GetComponent<Slider>();
            if (slider != null)
                slider.onValueChanged.AddListener(UpdateText);
            else
                Debug.LogError("Script must be attached to a slider");

            UpdateText(slider.value);
        }

        private void UpdateText(float value)
        {
            if (label != null)
            {
                float shownValue = value;
                if (percent)
                    shownValue = Mathf.RoundToInt(shownValue * 100);
                label.text = prefix + shownValue + postfix;
            }
        }
    }
}