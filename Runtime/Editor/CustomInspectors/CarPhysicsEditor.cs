#if UNITY_EDITOR
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(CarPhysics)), CanEditMultipleObjects]
    public class CarPhysicsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif