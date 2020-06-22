using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

namespace UnityHelpers
{
    public class KeyboardController : MonoBehaviour
    {
        private StringBuilder builtOutput = new StringBuilder();

        private GameObject currentlySelected;
        private InputField inputField;
        private TMP_InputField tmpInputField;
        private int caretPosition, selectionAnchorPosition, selectionFocusPosition;

        private KeyController[] keyboardKeys;

        void Start()
        {
            keyboardKeys = GetComponentsInChildren<KeyController>();
            foreach (var key in keyboardKeys)
                key.onKeyClicked += Append;
        }
        void Update()
        {
            RefreshInputFields();
            CheckCaret();
        }

        private void CheckCaret()
        {
            var eventSystemSelection = EventSystem.current.currentSelectedGameObject;
            if (inputField != null && eventSystemSelection == inputField.gameObject)
            {
                caretPosition = inputField.caretPosition;
                selectionAnchorPosition = inputField.selectionAnchorPosition;
                selectionFocusPosition = inputField.selectionFocusPosition;
            }
            else if (tmpInputField != null && eventSystemSelection == tmpInputField.gameObject)
            {
                caretPosition = tmpInputField.caretPosition;
                selectionAnchorPosition = tmpInputField.selectionAnchorPosition;
                selectionFocusPosition = tmpInputField.selectionFocusPosition;
            }

            //Debug.Log("Caret Pos: " + caretPosition + " SAP: " + selectionAnchorPosition + " SFP: " + selectionFocusPosition);
        }
        private void RefreshInputFields()
        {
            var eventSystemSelection = EventSystem.current.currentSelectedGameObject;
            if (eventSystemSelection != currentlySelected)
            {
				KeyController button = null;
				if (eventSystemSelection != null)
					button = eventSystemSelection.GetComponent<KeyController>(); //Should change this to be keyboard key component when created

                if (button == null)
                {
                    currentlySelected = eventSystemSelection;

					if (inputField != null)
                    	inputField.onValueChanged.RemoveListener(InputFieldValueChanged);
					if (tmpInputField != null)
						tmpInputField.onValueChanged.RemoveListener(InputFieldValueChanged);

                    inputField = null;
                    tmpInputField = null;

                    if (eventSystemSelection != null)
                    {
                        inputField = eventSystemSelection.GetComponent<InputField>();
                        if (inputField == null)
                            tmpInputField = eventSystemSelection.GetComponent<TMP_InputField>();

                        if (inputField != null)
                            SetText(inputField.text);
                        else if (tmpInputField != null)
                            SetText(tmpInputField.text);

						if (inputField != null)
                        	inputField.onValueChanged.AddListener(InputFieldValueChanged);
						if (tmpInputField != null)
							tmpInputField.onValueChanged.AddListener(InputFieldValueChanged);
                    }
                }
            }

            if (inputField != null)
                inputField.text = builtOutput.ToString();
            else if (tmpInputField != null)
                tmpInputField.text = builtOutput.ToString();
        }
        private void InputFieldValueChanged(string value)
        {
            SetText(value);
        }
        public void SetText(string value)
        {
			builtOutput.Remove (0, builtOutput.Length);
            //builtOutput.Clear();
            builtOutput.Append(value);
        }

        public void Append(string value)
        {
            int index = Mathf.Min(caretPosition, selectionAnchorPosition);
            int selectionAmount = Mathf.Abs(caretPosition - selectionAnchorPosition);
            //Debug.Log("Removing from " + index + " for " + selectionAmount);
            builtOutput.Remove(index, selectionAmount);
            builtOutput.Insert(index, value);
            EventSystem.current.SetSelectedGameObject(currentlySelected);

            StartCoroutine(ReturnCaretPosition(index + 1));
        }
        private IEnumerator ReturnCaretPosition(int index)
        {
            yield return null;

            if (inputField != null)
            {
                inputField.caretPosition = index;
                inputField.selectionAnchorPosition = index;
                inputField.selectionFocusPosition = index;
                inputField.ForceLabelUpdate();
            }
            else if (tmpInputField != null)
            {
                tmpInputField.caretPosition = index;
                tmpInputField.selectionAnchorPosition = index;
                tmpInputField.selectionFocusPosition = index;
                tmpInputField.ForceLabelUpdate();
            }
        }
    }
}