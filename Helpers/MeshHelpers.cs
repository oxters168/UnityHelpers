using System.Linq;
using UnityEngine;

namespace UnityHelpers
{
    public static class MeshHelpers
    {
        public const int MAX_VERTICES = 65534;

        /// <summary>
        /// Appends the vertices, triangle, normals, uv, uv2, uv3, uv4 of the other mesh to the current mesh.
        /// </summary>
        /// <param name="current">The mesh to append to</param>
        /// <param name="other">The mesh that will be appended</param>
        /// <param name="position">Position of TRS matrix</param>
        /// <param name="rotation">Rotation of TRS matrix</param>
        /// <param name="scale">Scale of TRS matrix</param>
        /// <returns>False if the mesh being appended to cannot fit what is being appended</returns>
        public static bool Append(this Mesh current, Mesh other, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (other != null && current.vertices.Length + other.vertices.Length < MAX_VERTICES)
            {
                Vector3[] manipulatedVertices = ManipulateVertices(other.vertices, position, rotation, scale);

                int[] correctedTriangles = new int[other.triangles.Length];
                for (int i = 0; i < correctedTriangles.Length; i++)
                    correctedTriangles[i] = other.triangles[i] + current.vertices.Length;

                current.vertices = current.vertices.Concat(manipulatedVertices).ToArray();
                current.triangles = current.triangles.Concat(correctedTriangles).ToArray();
                current.normals = current.normals.Concat(other.normals).ToArray();
                current.uv = current.uv.Concat(other.uv).ToArray();
                current.uv2 = current.uv2.Concat(other.uv2).ToArray();
                current.uv3 = current.uv3.Concat(other.uv3).ToArray();
                current.uv4 = current.uv4.Concat(other.uv4).ToArray();
                //current.vertices = Merge(current.vertices, manipulatedVertices);
                //current.triangles = Merge(current.triangles, correctedTriangles);
                //current.normals = Merge(current.normals, other.normals);
                //current.uv = Merge(current.uv, other.uv);
                //current.uv2 = Merge(current.uv2, other.uv2);
                //current.uv3 = Merge(current.uv3, other.uv3);
                //current.uv4 = Merge(current.uv4, other.uv4);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Manipulates all vertices of an array using a TRS matrix
        /// </summary>
        /// <param name="original">The vertices to be manipulated</param>
        /// <param name="position">The position of the TRS</param>
        /// <param name="rotation">The rotation of the TRS</param>
        /// <param name="scale">The scale of the TRS</param>
        /// <returns>The manipulated vertices</returns>
        public static Vector3[] ManipulateVertices(this Vector3[] original, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (!rotation.IsValid())
                rotation = rotation.FixNaN();

            Vector3[] manipulated = new Vector3[original.Length];
            Matrix4x4 manipulator = Matrix4x4.identity;
            manipulator.SetTRS(position, rotation, scale);
            int i = 0;
            while (i < original.Length)
            {
                manipulated[i] = manipulator.MultiplyPoint3x4(original[i]);
                i++;
            }
            return manipulated;
        }
        /// <summary>
        /// Checks if a point is on the surface of the object's mesh. This method goes through all the triangles of the mesh.
        /// This variant is faster, but it is less accurate. Works better with meshes that have triangles with consistently small areas around the size of the given range.
        /// </summary>
        /// <param name="currentObject">The object to be checked. Must have a MeshFilter component attached.</param>
        /// <param name="point">The point to be checked.</param>
        /// <param name="range">The distance from the point to check. (Clamped between 0.1 and MaxValue)</param>
        /// <returns>True if the point is on the surface of the mesh up to a certain range.</returns>
        public static bool IsPointOnSurfaceOf(this Transform currentObject, Vector3 point, float range)
        {
            bool onSurface = false;
            MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Bounds pointBounds = new Bounds(point, Vector3.one * Mathf.Clamp(range, 0.1f, float.MaxValue));

                Mesh mesh = meshFilter.sharedMesh;
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    Vector3 vertexA = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
                    Vector3 vertexB = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
                    Vector3 vertexC = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);

                    if (pointBounds.Contains(vertexA) || pointBounds.Contains(vertexB) || pointBounds.Contains(vertexC) || pointBounds.Contains(CalculateTriangleCenter(vertexA, vertexB, vertexC)))
                    {
                        onSurface = true;
                    }
                }
            }
            return onSurface;
        }
        /// <summary>
        /// Checks if a point is on the surface of the object's mesh. This method goes through all the triangles of the mesh.
        /// </summary>
        /// <param name="currentObject">The object to be checked. Must have a MeshFilter component attached.</param>
        /// <param name="point">The point to be checked.</param>
        /// <returns>True if the point is on the surface of the mesh.</returns>
        public static bool IsPointOnSurfaceOf(this Transform currentObject, Vector3 point)
        {
            bool onSurface = false;
            MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                for (int i = 0; i < mesh.triangles.Length; i += 3)
                {
                    Vector3 triangleA = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 0]]);
                    Vector3 triangleB = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 1]]);
                    Vector3 triangleC = currentObject.transform.TransformPoint(mesh.vertices[mesh.triangles[i + 2]]);

                    if (PointInTriangle(triangleA, triangleB, triangleC, point))
                    {
                        onSurface = true;
                    }
                }
            }
            return onSurface;
        }
        public static Vector3 CalculateTriangleCenter(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            return (vertexA + vertexB + vertexC) / 3f;
        }
        public static Vector3 CalculateTriangleNormal(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            return Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
        }
        public static float CalculateTriangleArea(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            float distanceAB = Vector3.Distance(vertexA, vertexB);
            float distanceAC = Vector3.Distance(vertexA, vertexC);
            return (distanceAB * distanceAC * Mathf.Sin(Vector3.Angle(vertexB - vertexA, vertexC - vertexA) * Mathf.Deg2Rad)) / 2f;
        }
        /// <summary>
        /// Checks if a point is in a triangle.
        /// </summary>
        /// <param name="triangleA">First triangle vertex.</param>
        /// <param name="triangleB">Second triangle vertex.</param>
        /// <param name="triangleC">Third triangle vertex.</param>
        /// <param name="point">The point to be checked.</param>
        /// <returns>True if the point is in the triangle, false otherwise.</returns>
        public static bool PointInTriangle(Vector3 triangleA, Vector3 triangleB, Vector3 triangleC, Vector3 point)
        {
            if (SameSide(point, triangleA, triangleB, triangleC) && SameSide(point, triangleB, triangleA, triangleC) && SameSide(point, triangleC, triangleA, triangleB))
            {
                Vector3 vc1 = Vector3.Cross(triangleA - triangleB, triangleA - triangleC);
                if (Mathf.Abs(Vector3.Dot((triangleA - point).normalized, vc1.normalized)) <= .01f)
                    return true;
            }

            return false;
        }
        private static bool SameSide(Vector3 p1, Vector3 p2, Vector3 A, Vector3 B)
        {
            Vector3 cp1 = Vector3.Cross(B - A, p1 - A);
            Vector3 cp2 = Vector3.Cross(B - A, p2 - A);
            if (Vector3.Dot(cp1, cp2) >= 0) return true;
            return false;
        }
    }
}