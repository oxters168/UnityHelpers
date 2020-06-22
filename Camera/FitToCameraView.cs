using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// Use this to get an object to fit properly in a camera's view.
    /// </summary>
	[ExecuteInEditMode]
    public class FitToCameraView : MonoBehaviour
    {
        [Range(0, 10000)]
        public float objectDistance;
        public Camera renderCamera;
        public bool square;
        public bool keepInFrame;

        private bool cameraErrored;

        private void Update()
        {
            if (renderCamera == null)
                renderCamera = GetComponent<Camera>();

            if (renderCamera != null)
            {
                cameraErrored = false;
                ResetRenderTexturePosition();
            }
            else if (!cameraErrored)
            {
                Debug.LogError("FitToCameraView(" + transform.name + "): Camera component not set");
                cameraErrored = true;
            }
        }

        private void ResetRenderTexturePosition()
        {
            Vector2 size = CameraHelpers.PerspectiveFrustum(renderCamera.fieldOfView, objectDistance, renderCamera.aspect);

            float sizeX = size.x, sizeY = size.y;
            if (square)
            {
                if ((keepInFrame && renderCamera.aspect < 1) || (!keepInFrame && renderCamera.aspect >= 1))
                    sizeY = size.x;
                else
                    sizeX = size.y;
            }

            var renderRectTransform = transform.GetComponent<RectTransform>();
            if (renderRectTransform != null)
            {
                sizeX /= renderRectTransform.rect.size.x;
                sizeY /= renderRectTransform.rect.size.y;
            }

            transform.localScale = new Vector3(sizeX, sizeY, transform.localScale.z);
            transform.localPosition = Vector3.forward * objectDistance;
            transform.localRotation = Quaternion.identity;
        }

        public void SetDistance(float value)
        {
            objectDistance = Mathf.Clamp(value, 0, 10000);
        }
    }
}