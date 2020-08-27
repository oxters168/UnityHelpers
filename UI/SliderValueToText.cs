using UnityEngine;
using UnityEngine.UI;

namespace UnityHelpers
{
	[ExecuteInEditMode]
    public class SliderValueToText : MonoBehaviour
    {
        private Slider slider;
        private bool sliderErrored, labelErrored;

        #if (TextMeshPro)
        public TMPro.TextMeshProUGUI targetLabel;
        #else
        public Text targetLabel;
        #endif

        [Space(10)]

        public string prefix;
        public string postfix;

        [Space(10)]

        public bool percent;

        [Space(10)]

        public bool formatDecimalPlaces;
        [Range(0, 99)]
        public uint decimalPlaces = 2;

        private void Update()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (slider != null)
            {
                sliderErrored = false;
                UpdateText(slider.value);
            }
            else if (!sliderErrored)
            {
                Debug.LogError("SliderValueToText(" + transform.name + "): Could not find slider component, this script should be attached to the same object that has the slider component");
                sliderErrored = true;
            }
        }

        private void UpdateText(float value)
        {
            if (targetLabel != null)
            {
                labelErrored = false;

                float shownValue = value;
                if (percent)
                    shownValue = Mathf.RoundToInt(shownValue * 100);

                string valueString = shownValue.ToString();
                if (formatDecimalPlaces)
                    valueString = string.Format("{0:F" + decimalPlaces + "}", shownValue);
                targetLabel.text = prefix + valueString + postfix;
            }
            else if (!labelErrored)
            {
                Debug.LogError("SliderValueToText(" + transform.name + "): Missing target label");
                labelErrored = true;
            }
        }
    }
}