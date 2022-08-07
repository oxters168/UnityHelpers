#if UNITY_EDITOR
using UnityEditor;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(Buoyancy)), CanEditMultipleObjects]
    public class BuoyancyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif