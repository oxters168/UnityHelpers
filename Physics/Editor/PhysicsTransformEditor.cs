using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(PhysicsTransform)), CanEditMultipleObjects]
    public class PhysicsTransformEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}