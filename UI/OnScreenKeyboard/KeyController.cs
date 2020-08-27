using UnityEngine;
using UnityEngine.UI;
#if (TextMeshPro)
using TMPro;
#endif

namespace UnityHelpers
{
    [RequireComponent(typeof(Button))]
    public class KeyController : MonoBehaviour
    {
        public string lowercaseValue;
        public string uppercaseValue;

        #if (TextMeshPro)
        public TextMeshProUGUI label;
        #else
        public Text label;
        #endif

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
			if (onKeyClicked != null)
            	onKeyClicked.Invoke(lowercaseValue);
        }
    }
}