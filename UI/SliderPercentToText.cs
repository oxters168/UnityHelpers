using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class SliderPercentToText : MonoBehaviour
{
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
            int percentValue = Mathf.RoundToInt(value * 100);
            label.text = prefix + percentValue + postfix;
        }
    }
}
