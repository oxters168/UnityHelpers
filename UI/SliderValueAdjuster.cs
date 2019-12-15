using UnityEngine;
using UnityEngine.UI;

namespace UnityHelpers
{
    [ExecuteAlways]
    public class SliderValueAdjuster : MonoBehaviour
    {
        private Slider slider;
        private bool sliderErrored;

        [Range(0, 10)]
        public uint decimalPlaces;

        void Update()
        {
            if (slider == null)
                slider = GetComponent<Slider>();

            if (slider != null)
            {
                sliderErrored = false;
                slider.value = MathHelpers.SetDecimalPlaces(slider.value, decimalPlaces);
            }
            else if (!sliderErrored)
            {
                Debug.LogError("SliderValueAdjuster(" + transform.name + "): Could not find slider component, this script should be attached to the same object that has the slider component");
                sliderErrored = true;
            }
        }
    }
}