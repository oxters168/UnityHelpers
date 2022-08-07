#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityHelpers.Editor
{
    [CustomEditor(typeof(Cubemapper))]
    public class CubemapperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var cubemapper = (Cubemapper)target;
            if (GUILayout.Button("Generate"))
            {
                string fullpath = EditorUtility.SaveFilePanelInProject("Save Cubemap", "Cubemap", "", "Choose a path to save the cubemap to");
                if (!string.IsNullOrEmpty(fullpath))
                {
                    cubemapper.Generate(Path.GetDirectoryName(fullpath), Path.GetFileNameWithoutExtension(fullpath));
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
#endif