using System.Linq;
using UnityEngine;

namespace UnityHelpers
{
    public static class MeshHelpers
    {
        public static readonly int MAX_VERTICES = 65534;

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
    }
}