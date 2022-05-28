using UnityEngine;
using System.IO;

namespace UnityHelpers
{
    public class Snapshotter : MonoBehaviour
    {
        public int width = 2048, height = 2048, depth = 16;

        private Camera theCamera;
        private RenderTexture renderTexture;
        private Texture2D saveTexture;

        private string directory, fileName;

        public void Generate(string dir, string textureName)
        {
            directory = dir;
            fileName = textureName;

            theCamera = GetComponent<Camera>();
            if (theCamera != null)
            {
                renderTexture = new RenderTexture(width, height, depth);
                saveTexture = new Texture2D(renderTexture.width, renderTexture.height);

                RenderAndSave();

                RenderTexture.active = null;
                theCamera.targetTexture = null;

                DestroyImmediate(saveTexture);
                DestroyImmediate(renderTexture);
            }
            else
                Debug.LogError("Snapshotter: Could not find camera");
        }

        private void RenderAndSave()
        {
            RenderToTexture(theCamera, renderTexture, saveTexture);
            File.WriteAllBytes(Path.Combine(directory, fileName + ".png"), saveTexture.EncodeToPNG());
        }
        private static void RenderToTexture(Camera theCamera, RenderTexture renderTexture, Texture2D saveTexture)
        {
            theCamera.targetTexture = renderTexture;
            theCamera.Render();

            RenderTexture.active = renderTexture;
            Rect renderRect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            saveTexture.ReadPixels(renderRect, 0, 0);
        }
    }
}