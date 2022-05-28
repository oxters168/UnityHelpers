using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityHelpers
{
    [CustomEditor(typeof(Snapshotter))]
    public class SnapshotterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var snapshotter = (Snapshotter)target;
            if (GUILayout.Button("Generate"))
            {
                string fullpath = EditorUtility.SaveFilePanel("Save Snapshot", "", "Snapshot", "png");
                if (!string.IsNullOrEmpty(fullpath))
                {
                    snapshotter.Generate(Path.GetDirectoryName(fullpath), Path.GetFileNameWithoutExtension(fullpath));
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}