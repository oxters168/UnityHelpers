using UnityEngine;
using UnityEditor;

namespace UnityHelpers
{
    /// <summary>
    /// Drawer for the RequireInterface attribute.
    /// Source: https://www.patrykgalach.com/2020/01/27/assigning-interface-in-unity-inspector/?cn-reloaded=1
    /// </summary>
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        /// <summary>
        /// Overrides GUI drawing for the attribute.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Check if this is reference type property.
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                // Get attribute parameters.
                var requiredAttribute = this.attribute as RequireInterfaceAttribute;
                label.text += " (" + requiredAttribute.requiredType + ")";

                // Begin drawing property field.
                EditorGUI.BeginProperty(position, label, property);
                // Draw property field.
                var givenObject = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(GameObject), true);
                if (givenObject != null)
                {
                    GameObject castedObject = null;
                    if (givenObject is GameObject)
                        castedObject = ((GameObject)givenObject);
                    if (castedObject != null)
                    {
                        var interfaceComponent = castedObject.GetComponent(requiredAttribute.requiredType);
                        if (interfaceComponent != null)
                        {
                            property.objectReferenceValue = givenObject;
                        }
                        else
                            Debug.LogError("Given object must implement the " + requiredAttribute.requiredType + " interface");
                    }
                }
                else
                {
                    property.objectReferenceValue = null;
                }
                // Finish drawing property field.
                EditorGUI.EndProperty();
            }
            else
            {
                // If field is not reference, show error message.
                // Save previous color and change GUI to red.
                var previousColor = GUI.color;
                GUI.color = Color.red;
                // Display label with error message.
                EditorGUI.LabelField(position, label, new GUIContent("Property is not a reference type"));
                // Revert color change.
                GUI.color = previousColor;
            }
        }
    }
}