using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public class FakeObjects : MonoBehaviour
    {
        private Camera CurrentCamera { get { if (_currentCamera == null) { _currentCamera = GetComponent<Camera>(); if (_currentCamera != null) _currentCamera.depthTextureMode |= DepthTextureMode.Depth; } return _currentCamera; } }
        private Camera _currentCamera;

        public Shader shader;
        private Material _material;

        public float epsilon = 0.001f;
        public float zValue;
        public Vector3 eulerRot;
        public BoxInfo box = new BoxInfo() { position = Vector3.zero, rotation = Quaternion.identity, size = Vector3.one };

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            box.rotation = Quaternion.Euler(eulerRot);
            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (shader)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (shader != null && _material != null)
            {
                var allSquares = box.SplitToSquares()
                .SelectMany(
                    (polygon) =>
                    new Vector4[]
                    {
                        //new Vector4(polygon.vertices[0].x, polygon.vertices[0].y, polygon.vertices[0].z),
                        //new Vector4(polygon.vertices[1].x, polygon.vertices[1].y, polygon.vertices[1].z),
                        //new Vector4(polygon.vertices[2].x, polygon.vertices[2].y, polygon.vertices[2].z),
                        //new Vector4(polygon.vertices[3].x, polygon.vertices[3].y, polygon.vertices[3].z),
                        polygon.vertices[0],
                        polygon.vertices[1],
                        polygon.vertices[2],
                        polygon.vertices[3],
                        polygon.CalculateNormal()
                    }
                )
                .ToArray();

                /*allSquares = new Vector4[]
                    {
                        new Vector4(-0.5f, -0.5f, zValue),
                        new Vector4(0.5f, -0.5f, zValue),
                        new Vector4(0.5f, 0.5f, zValue),
                        new Vector4(-0.5f, 0.5f, zValue),
                        Vector3.forward
                    };*/
                
                //string verticesLog = allSquares.Length + "\n";
                //foreach (var vertex in allSquares)
                //    verticesLog += vertex + "\n";
                //Debug.Log(verticesLog);

                Shader.SetGlobalVectorArray("_AllSquares", allSquares);

                _material.SetVector("_CameraPos", transform.position);
                _material.SetFloat("epsilon", epsilon);
                _material.SetFloat("radius", zValue);

                Matrix4x4 matrixCameraToWorld = CurrentCamera.cameraToWorldMatrix;
                Matrix4x4 matrixProjectionInverse = GL.GetGPUProjectionMatrix(CurrentCamera.projectionMatrix, false).inverse;
                Matrix4x4 matrixHClipToWorld = matrixCameraToWorld * matrixProjectionInverse;
                Shader.SetGlobalMatrix("_MatrixHClipToWorld", matrixHClipToWorld);

                Graphics.Blit(source, destination, _material);
            }
        }
    }

    [System.Serializable]
    public struct BoxInfo
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 size;

        public PolygonInfo[] SplitToSquares()
        {
            Vector3 cubeForward = rotation * Vector3.forward;
            Vector3 cubeUp = rotation * Vector3.up;
            Vector3 cubeRight = rotation * Vector3.right;

            Vector3 forwardExtent = cubeForward * size.z / 2;
            Vector3 rightExtent = cubeRight * size.x / 2;
            Vector3 upExtent = cubeUp * size.y / 2;

            PolygonInfo frontFace = CalculateFace(position, forwardExtent, rightExtent, upExtent);
            PolygonInfo backFace = CalculateFace(position, -forwardExtent, -rightExtent, upExtent);
            PolygonInfo leftFace = CalculateFace(position, -rightExtent, forwardExtent, upExtent);
            PolygonInfo rightFace = CalculateFace(position, rightExtent, -forwardExtent, upExtent);
            PolygonInfo topFace = CalculateFace(position, upExtent, rightExtent, -forwardExtent);
            PolygonInfo bottomFace = CalculateFace(position, -upExtent, rightExtent, forwardExtent);

            return new PolygonInfo[] { frontFace, backFace, leftFace, rightFace, topFace, bottomFace };
        }

        public static PolygonInfo CalculateFace(Vector3 center, Vector3 faceDirectionExtent, Vector3 directionRightExtent, Vector3 directionUpExtent)
        {
            Vector3 bottomLeftCorner = center + faceDirectionExtent - directionRightExtent - directionUpExtent;
            Vector3 bottomRightCorner = center + faceDirectionExtent + directionRightExtent - directionUpExtent;
            Vector3 topRightCorner = center + faceDirectionExtent + directionRightExtent + directionUpExtent;
            Vector3 topLeftCorner = center + faceDirectionExtent - directionRightExtent + directionUpExtent;

            return new PolygonInfo() { vertices = new Vector3[] { bottomLeftCorner, bottomRightCorner, topRightCorner, topLeftCorner } };
        }
    }
    [System.Serializable]
    public struct PolygonInfo
    {
        public Vector3[] vertices;

        /// <summary>
        /// Calculates the polygon's normal based on the first three points in the vertices array
        /// </summary>
        /// <returns>The normal of the polygon described by the vertices</returns>
        public Vector3 CalculateNormal()
        {
            //Source: https://discourse.mcneel.com/t/how-to-calculate-ngon-face-normal-correctly/45767/4
            Vector3 normal = Vector3.zero;
            if (vertices != null && vertices.Length >= 4)
            {
                var a = vertices[1] - vertices[0];
                var b = vertices[3] - vertices[0];
                normal = Vector3.Cross(a, b);
            }
            return normal;
        }
    }
}