using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(BezierCurveGizmoVisualizer)), CanEditMultipleObjects]
    public class BezierCurveGizmoVisualizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}