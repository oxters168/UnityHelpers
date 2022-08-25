#if UNITY_EDITOR
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(TankPhysics)), CanEditMultipleObjects]
    public class TankPhysicsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif