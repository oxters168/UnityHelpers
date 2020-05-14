using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(BallPhysics)), CanEditMultipleObjects]
    public class BallPhysicsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}