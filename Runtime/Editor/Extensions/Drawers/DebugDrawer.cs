#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomPropertyDrawer(typeof(DebugAttribute))]
    public class DebugDrawer : PropertyDrawer
    {
        private Vector2 scrollPosition;
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var debugData = property.GetTargetObjectOfProperty() as string;
            if (!string.IsNullOrEmpty(debugData))
            {
                var att = this.attribute as DebugAttribute;

                Vector2 boxSize = EditorStyles.helpBox.CalcSize(new GUIContent(debugData));
                if (att.highlightable)
                    boxSize = EditorStyles.textField.CalcSize(new GUIContent(debugData));

                if (att.collapsable)
                    property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);

                if (!att.collapsable || property.isExpanded)
                {
                    if (att.highlightable)
                        GUILayout.TextArea(debugData, EditorStyles.textArea);
                    else
                        EditorGUILayout.HelpBox(debugData, att.msgType);
                }
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var debugData = property.GetTargetObjectOfProperty() as string;
            float height = base.GetPropertyHeight(property, label);
            if (string.IsNullOrEmpty(debugData))
                height = -2; //Setting to zero still gives a tiny bit of padding for some reason, so had to do -2
            return height;
        }
    }
}
#endif