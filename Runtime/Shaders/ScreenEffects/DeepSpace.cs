using UnityEngine;

namespace UnityHelpers
{
    public class DeepSpace : MonoBehaviour
    {
        private Camera CurrentCamera { get { if (_currentCamera == null) _currentCamera = GetComponent<Camera>(); return _currentCamera; } }
        private Camera _currentCamera;

        public Shader shader;
        private Material _material;

        public float skyboxRadius = 1; //Actually the min distance
        public Color nebulaColor = Color.white;
        public float nebulaMoveMultiplier = 10; //Actually the max distance
		public float nebulaNoiseValue = 10;
        [Range(0, 1)]
        public float nebulaShrink;
        public Vector4 nebulaVelocity;

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

                if (_material.HasProperty("_SkyboxRadius"))
                {
                    _material.SetFloat("_SkyboxRadius", skyboxRadius);
                }
                if (_material.HasProperty("_NebulaColor"))
                {
                    _material.SetColor("_NebulaColor", nebulaColor);
                }
                if (_material.HasProperty("_NebulaMoveMultiplier"))
                {
                    _material.SetFloat("_NebulaMoveMultiplier", nebulaMoveMultiplier);
                }
                if (_material.HasProperty("_NebulaNoiseValue"))
                {
                    _material.SetFloat("_NebulaNoiseValue", nebulaNoiseValue);
                }
                if (_material.HasProperty("_NebulaShrink"))
                {
                    _material.SetFloat("_NebulaShrink", nebulaShrink);
                }
                if (_material.HasProperty("_NebulaVelocity"))
                {
                    _material.SetVector("_NebulaVelocity", nebulaVelocity);
                }
            }

            if (shader != null && _material != null)
            {
                Matrix4x4 matrixCameraToWorld = CurrentCamera.cameraToWorldMatrix;
                Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(CurrentCamera.projectionMatrix, false).inverse;
                Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;

                Shader.SetGlobalMatrix("_MatrixHClipToWorld", matrixHClipToWorld);

                Graphics.Blit(source, destination, _material);
            }
        }
    }
}