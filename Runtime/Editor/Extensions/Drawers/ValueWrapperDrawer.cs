#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomPropertyDrawer(typeof(ValueWrapper))]
    public class ValueWrapperDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueWrapper = property.GetTargetObjectOfProperty() as ValueWrapper;

            position.height = 16;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded)
            {
                var fieldPosition = position;
                fieldPosition.height = 16;
                fieldPosition.y += 16;

                EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("name"));
                fieldPosition.y += 16;
                EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative("storedTypes"));
                fieldPosition.y += 16;

                var possibleValues = (ValueWrapper.CommonType[])System.Enum.GetValues(typeof(ValueWrapper.CommonType));
                foreach (var possibleValue in possibleValues)
                {
                    if (valueWrapper.Contains(possibleValue))
                    {
                        EditorGUI.PropertyField(fieldPosition, property.FindPropertyRelative(possibleValue.ToString()));
                        fieldPosition.y += 16;
                    }
                }
            }
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 16;
            int defaultCount = 0;
            if (property.isExpanded)
            {
                defaultCount = 2;
                var valueWrapper = property.GetTargetObjectOfProperty() as ValueWrapper;
                var possibleValues = (ValueWrapper.CommonType[])System.Enum.GetValues(typeof(ValueWrapper.CommonType));
                foreach (var possibleValue in possibleValues)
                    if (valueWrapper.Contains(possibleValue))
                        defaultCount += 1;
            }
            height += 16 * defaultCount;
            return height;
        }
    }
}
#endif