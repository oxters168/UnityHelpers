#if UNITY_EDITOR
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(BallPhysics)), CanEditMultipleObjects]
    public class BallPhysicsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif