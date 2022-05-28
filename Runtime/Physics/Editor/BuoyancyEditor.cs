using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(Buoyancy)), CanEditMultipleObjects]
    public class BuoyancyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}