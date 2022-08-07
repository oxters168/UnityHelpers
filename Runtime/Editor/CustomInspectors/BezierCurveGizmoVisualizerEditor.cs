#if UNITY_EDITOR
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(BezierCurveGizmoVisualizer)), CanEditMultipleObjects]
    public class BezierCurveGizmoVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif