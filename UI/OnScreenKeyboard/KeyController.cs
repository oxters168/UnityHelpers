using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityHelpers
{
    [RequireComponent(typeof(Button))]
    public class KeyController : MonoBehaviour
    {
        public string lowercaseValue;
        public string uppercaseValue;

        public TextMeshProUGUI label;

        public event KeyClickedHandler onKeyClicked;
        public delegate void KeyClickedHandler(string value);

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(Clicked);
        }
        void Update()
        {
            label.text = lowercaseValue;
        }

        private void Clicked()
        {
            onKeyClicked?.Invoke(lowercaseValue);
        }
    }
}