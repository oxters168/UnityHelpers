//https://medium.com/@ProGM/show-a-draggable-point-into-the-scene-linked-to-a-vector3-field-using-the-handle-api-in-unity-bffc1a98271d
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityHelpers
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class DraggablePointDrawer : Editor
    {
        readonly GUIStyle style = new GUIStyle(); void OnEnable()
        {
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
        }
        public void OnSceneGUI()
        {
            var property = serializedObject.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType == SerializedPropertyType.Vector3)
                {
                    var field = serializedObject.targetObject.GetType().GetField(property.name);
                    if (field == null)
                        continue;

                    var draggablePoints = field.GetCustomAttributes(typeof(DraggablePoint), false) as DraggablePoint[];
                    if (draggablePoints.Length > 0)
                    {
                        Transform transform = null;
                        bool isLocal = draggablePoints[0].local;
                        if (isLocal)
                            transform = (target as MonoBehaviour).transform;

                        Vector3 value = property.vector3Value;
                        if (isLocal)
                            value = transform.TransformPoint(value);

                        Handles.Label(value, property.name);
                        var handleOutput = Handles.PositionHandle(value, Quaternion.identity);
                        property.vector3Value += handleOutput - value;

                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }
        }
    }
    #endif
}