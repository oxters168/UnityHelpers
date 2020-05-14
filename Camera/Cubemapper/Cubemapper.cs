using UnityEngine;
using System.IO;

namespace UnityHelpers
{
    public class Cubemapper : MonoBehaviour
    {
        public int width = 2048, height = 2048, depth = 16;

        private Camera theCamera;
        private RenderTexture renderTexture;
        private Texture2D saveTexture;

        private string directory, cubemapPrefix;

        public void Generate(string dir, string cubemapName)
        {
            directory = dir;
            cubemapPrefix = cubemapName;

            theCamera = GetComponent<Camera>();
            if (theCamera != null)
            {
                renderTexture = new RenderTexture(width, height, depth);
                saveTexture = new Texture2D(renderTexture.width, renderTexture.height);

                var startRotation = transform.rotation;

                RenderAndSave("ZPos");

                transform.rotation = Quaternion.AngleAxis(90, transform.up) * startRotation;
                RenderAndSave("XPos");

                transform.rotation = startRotation;
                transform.rotation = Quaternion.AngleAxis(180, transform.up) * startRotation;
                RenderAndSave("ZNeg");

                transform.rotation = startRotation;
                transform.rotation = Quaternion.AngleAxis(-90, transform.up) * startRotation;
                RenderAndSave("XNeg");

                transform.rotation = startRotation;
                transform.rotation = Quaternion.AngleAxis(-90, transform.right) * startRotation;
                RenderAndSave("YPos");

                transform.rotation = startRotation;
                transform.rotation = Quaternion.AngleAxis(90, transform.right) * startRotation;
                RenderAndSave("YNeg");

                transform.rotation = startRotation;

                RenderTexture.active = null;
                theCamera.targetTexture = null;

                DestroyImmediate(saveTexture);
                DestroyImmediate(renderTexture);
            }
            else
                Debug.LogError("Cubemapper: Could not find camera");
        }

        private void RenderAndSave(string directionName)
        {
            RenderToTexture(theCamera, renderTexture, saveTexture);
            File.WriteAllBytes(Path.Combine(directory, cubemapPrefix + "_" + directionName + ".png"), saveTexture.EncodeToPNG());
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