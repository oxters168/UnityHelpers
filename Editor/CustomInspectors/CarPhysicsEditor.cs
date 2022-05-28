using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(CarPhysics)), CanEditMultipleObjects]
    public class CarPhysicsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}