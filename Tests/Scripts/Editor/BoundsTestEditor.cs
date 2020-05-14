using UnityEditor;

namespace UnityHelpers
{
    [CustomEditor(typeof(BoundsTest)), CanEditMultipleObjects]
    public class BoundsTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}