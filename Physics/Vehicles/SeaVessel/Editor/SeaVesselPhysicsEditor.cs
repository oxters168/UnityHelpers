using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(SeaVesselPhysics)), CanEditMultipleObjects]
    public class SeaVesselPhysicsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}