#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UnityHelpers
{
    [CustomEditor(typeof(SetCenterOfMass)), CanEditMultipleObjects]
    public class SetCenterOfMassEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        void OnSceneGUI()
        {
            var currentSelf = target as SetCenterOfMass;

            EditorGUI.BeginChangeCheck();
            var newCenterOfMass = currentSelf.transform.InverseTransformPoint(Handles.PositionHandle(currentSelf.transform.TransformPoint(currentSelf.centerOfMass), Quaternion.identity));
            newCenterOfMass = newCenterOfMass.SetDecimalPlaces(3);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed center of mass");
                currentSelf.centerOfMass = newCenterOfMass;
            }
        }
    }
}
#endif