using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public class MeshPart : MonoBehaviour
    {
        private Renderer meshRenderer;
        private MeshFilter meshFilter;
        private Mesh mesh;

        void Awake()
        {
            InitRender();
        }

        private void InitRender()
        {
            meshRenderer = GetComponent<Renderer>();
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
        }

        public void SetMaterial(Material material, int subMeshIndex)
        {
            List<Material> materials = new List<Material>();
            meshRenderer.GetSharedMaterials(materials);
            materials[subMeshIndex] = material;
            meshRenderer.materials = materials.ToArray();
        }
        public void SetMaterials(Dictionary<int, Material> matsDick)
        {
            List<Material> materials = new List<Material>();
            meshRenderer.GetSharedMaterials(materials);
            foreach (var dickPair in matsDick)
                materials[dickPair.Key] = dickPair.Value;
            meshRenderer.materials = materials.ToArray();
        }
        public void AddVertices(IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Vector2> uv, IEnumerable<Vector3> normals, int subMeshIndex = 0)
        {
            if (GetVertexCount() + vertices.Count() > MeshHelpers.MAX_VERTICES)
                throw new System.OverflowException("Exceeded the max vertex count (" + MeshHelpers.MAX_VERTICES + ")");
            
            if (subMeshIndex >= mesh.subMeshCount)
            {
                mesh.subMeshCount = subMeshIndex + 1;
                meshRenderer.materials = meshRenderer.materials.Concat(new Material[mesh.subMeshCount - meshRenderer.materials.Length]).ToArray();
            }

            int newTrianglesShift = mesh.vertexCount;
            int[] subTriangles = mesh.GetTriangles(subMeshIndex);
            Vector2[] oldUV = mesh.uv;
            Vector3[] oldNormals = mesh.normals;
            
            mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
            mesh.SetTriangles(subTriangles.Concat(triangles.ShiftTriangleIndices(newTrianglesShift)).ToArray(), subMeshIndex);
            if (normals != null)
                mesh.normals = oldNormals.Concat(normals).ToArray();
            if (uv != null)
                mesh.uv = oldUV.Concat(uv).ToArray();
        }
        public void AddVertices(IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Vector2> uv, int subMeshIndex = 0)
        {
            if (GetVertexCount() + vertices.Count() > MeshHelpers.MAX_VERTICES)
                throw new System.OverflowException("Exceeded the max vertex count (" + MeshHelpers.MAX_VERTICES + ")");
            
            if (subMeshIndex >= mesh.subMeshCount)
            {
                mesh.subMeshCount = subMeshIndex + 1;
                meshRenderer.materials = meshRenderer.materials.Concat(new Material[mesh.subMeshCount - meshRenderer.materials.Length]).ToArray();
            }

            int newTrianglesShift = mesh.vertexCount;
            int[] subTriangles = mesh.GetTriangles(subMeshIndex);
            Vector2[] oldUV = mesh.uv;
            
            mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
            mesh.SetTriangles(subTriangles.Concat(triangles.ShiftTriangleIndices(newTrianglesShift)).ToArray(), subMeshIndex);
            if (uv != null)
                mesh.uv = oldUV.Concat(uv).ToArray();

            mesh.RecalculateNormals();
        }
        public void AddVertices(IEnumerable<Vector3> vertices, IDictionary<int, IEnumerable<int>> triesDick, IEnumerable<Vector2> uv)
        {
            if (GetVertexCount() + vertices.Count() > MeshHelpers.MAX_VERTICES)
                throw new System.OverflowException("Exceeded the max vertex count (" + MeshHelpers.MAX_VERTICES + ")");
            
            int highestSubMeshIndex = triesDick.Keys.Max();
            if (highestSubMeshIndex >= mesh.subMeshCount)
            {
                mesh.subMeshCount = highestSubMeshIndex + 1;
                meshRenderer.materials = meshRenderer.materials.Concat(new Material[mesh.subMeshCount - meshRenderer.materials.Length]).ToArray();
            }

            int newTrianglesShift = mesh.vertexCount;
            Vector2[] oldUV = mesh.uv;

            mesh.vertices = mesh.vertices.Concat(vertices).ToArray();
            if (uv != null)
                mesh.uv = oldUV.Concat(uv).ToArray();

            foreach (var tryDick in triesDick)
            {
                int[] subTriangles = mesh.GetTriangles(tryDick.Key);
                mesh.SetTriangles(subTriangles.Concat(tryDick.Value.ShiftTriangleIndices(newTrianglesShift)).ToArray(), tryDick.Key);
            }

            mesh.RecalculateNormals();
        }
        public void SetVertices(IEnumerable<Vector3> vertices)
        {
            mesh.vertices = vertices.ToArray();
            mesh.RecalculateNormals();
        }
        public void SetVertices(IEnumerable<Vector3> vertices, IEnumerable<int> triangles)
        {
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
        }

        public int GetVertexCount()
        {
            return mesh.vertexCount;
        }
    }
}