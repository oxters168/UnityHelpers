using UnityEngine;

namespace UnityHelpers
{
    public class FakeObjects : MonoBehaviour
    {
        private Camera CurrentCamera { get { if (_currentCamera == null) { _currentCamera = GetComponent<Camera>(); if (_currentCamera != null) _currentCamera.depthTextureMode |= DepthTextureMode.Depth; } return _currentCamera; } }
        private Camera _currentCamera;

        public Shader shader;
        private Material _material;

        public Vector4 squareBottomLeftCorner;
        public Vector4 squareBottomRightCorner;
        public Vector4 squareTopRightCorner;
        public Vector4 squareTopLeftCorner;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (shader)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;

                if (_material.HasProperty("_SquareBottomLeftCorner"))
                {
                    _material.SetVector("_SquareBottomLeftCorner", squareBottomLeftCorner);
                }
                if (_material.HasProperty("_SquareBottomRightCorner"))
                {
                    _material.SetVector("_SquareBottomRightCorner", squareBottomRightCorner);
                }
                if (_material.HasProperty("_SquareTopRightCorner"))
                {
                    _material.SetVector("_SquareTopRightCorner", squareTopRightCorner);
                }
                if (_material.HasProperty("_SquareTopLeftCorner"))
                {
                    _material.SetVector("_SquareTopLeftCorner", squareTopLeftCorner);
                }
            }

            if (shader != null && _material != null)
            {
                _material.SetVector("_CameraPos", transform.position);

                Matrix4x4 matrixCameraToWorld = CurrentCamera.cameraToWorldMatrix;
                Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(CurrentCamera.projectionMatrix, false).inverse;
                Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;
                Shader.SetGlobalMatrix("_MatrixHClipToWorld", matrixHClipToWorld);

                Graphics.Blit(source, destination, _material);
            }
        }
    }
}