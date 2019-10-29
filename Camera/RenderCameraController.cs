using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// Use this to get a render texture to fit properly in a camera's view.
    /// </summary>
    [ExecuteAlways]
    public class RenderCameraController : MonoBehaviour
    {
        private float prevRenderTexture = float.MinValue;
        [Range(0, 10000)]
        public float renderTextureDistance;
        public Transform renderTexture;
        private float prevFOV = float.MinValue, prevAspect = float.MinValue;
        private Camera renderCamera;

        private void Start()
        {
            renderCamera = GetComponent<Camera>();
        }
        private void Update()
        {
            if (prevRenderTexture != renderTextureDistance || prevFOV != renderCamera.fieldOfView || prevAspect != renderCamera.aspect)
            {
                ResetRenderTexturePosition();
                prevRenderTexture = renderTextureDistance;
                prevFOV = renderCamera.fieldOfView;
                prevAspect = renderCamera.aspect;
            }
        }

        private void ResetRenderTexturePosition()
        {
            Vector2 size = CameraHelpers.PerspectiveFrustum(renderCamera.fieldOfView, renderTextureDistance, renderCamera.aspect);

            if (renderCamera.aspect >= 1)
                renderTexture.localScale = new Vector3(size.x, size.x, 1);
            else
                renderTexture.localScale = new Vector3(size.y, size.y, 1);

            renderTexture.localPosition = Vector3.forward * renderTextureDistance;
            renderTexture.localRotation = Quaternion.identity;
        }
    }
}