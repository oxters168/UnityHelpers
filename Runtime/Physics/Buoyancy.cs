using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class Buoyancy : MonoBehaviour
    {
        [Tooltip("The world height of the water plane, if you'd like non-planar water then set getWaterDisplacementAt in script")]
        public float waterLevel = 0;
        /// <summary>
        /// Returns the difference between the given point's height and the water height at that point as a float.
        /// If not set, will use the waterLevel variable.
        /// </summary>
        public Func<Vector3, float> getWaterDisplacementAt;

        private Rigidbody _rigidbody;
        private Rigidbody MainBody { get { if (!_rigidbody) _rigidbody = GetComponent<Rigidbody>(); return _rigidbody; } }

        public MeshFilter[] affectedMeshes;
        public bool showUnderwaterMesh;

        //private Material lpwMaterial { get { return SettingsController.settingsInScene.seaPrefab.waterAsset.WaterScript.material; } }
        [Tooltip("Density of liquid")]
        public float rho = 1027f;

        private Dictionary<Transform, Vector3[]> localVertices = new Dictionary<Transform, Vector3[]>();
        private List<Vector3> allGlobalVertices = new List<Vector3>();
        private List<int> allTriangles = new List<int>();

        private List<TriangleData> triangleData = new List<TriangleData>();

        void Start()
        {
            ReadMeshFilters();
        }
        void OnDrawGizmos()
        {
            if (showUnderwaterMesh)
                ShowUnderwaterMesh();
        }
        void FixedUpdate()
        {
            RefreshGlobalVertices();
            CalculateTriangleData();
            ApplyForces();
        }

        private float GetWaterDisplacementAt(Vector3 position)
        {
            return getWaterDisplacementAt != null ? getWaterDisplacementAt(position) : position.y - waterLevel;
        }

        private void ReadMeshFilters()
        {
            if (affectedMeshes == null)
                affectedMeshes = GetComponentsInChildren<MeshFilter>();

            foreach (MeshFilter meshFilter in affectedMeshes)
                if (meshFilter && meshFilter.mesh != null)
                {
                    try
                    {
                        Mesh convexedMesh = meshFilter.mesh.ToConvexHull(0.5);
                        localVertices.Add(meshFilter.transform, convexedMesh.vertices);
                        allTriangles.AddRange(convexedMesh.triangles);
                    }
                    catch (System.Exception) { }
                }
        }
        private void RefreshGlobalVertices()
        {
            allGlobalVertices.Clear();
            foreach (KeyValuePair<Transform, Vector3[]> meshPair in localVertices)
                allGlobalVertices.AddRange(meshPair.Value.Select(vertex => meshPair.Key.TransformPoint(vertex)));
        }
        private void ApplyForces()
        {
            foreach (TriangleData triangle in triangleData)
            {
                Vector3 force;
                force = rho * -triangle.normal.Multiply(Physics.gravity) * GetWaterDisplacementAt(triangle.center) * triangle.area;

                MainBody.AddForceAtPosition(-Physics.gravity.normalized * force.y, triangle.center);
            }
        }
        private void CalculateTriangleData()
        {
            triangleData.Clear();

            List<VertexData> currentTrio;
            for (int i = 0; i < allTriangles.Count; i += 3)
            {
                currentTrio = new List<VertexData>();
                for (int j = 0; j < 3; j++)
                {
                    Vector3 currentVertex = allGlobalVertices[allTriangles[i + j]];

                    currentTrio.Add(new VertexData(j, currentVertex, GetWaterDisplacementAt(currentVertex)));
                }

                if (currentTrio[0].distance > 0 && currentTrio[1].distance > 0 && currentTrio[2].distance > 0)
                    continue;

                if (currentTrio[0].distance < 0 && currentTrio[1].distance < 0 && currentTrio[2].distance < 0)
                    triangleData.Add(new TriangleData(currentTrio[0].position, currentTrio[1].position, currentTrio[2].position));
                else
                {
                    currentTrio.Sort((v1, v2) => v1.distance.CompareTo(v2.distance));
                    currentTrio.Reverse();

                    if (currentTrio[1].distance > 0)
                        AddTrianglesTwoAboveWater(currentTrio);
                    else
                        AddTrianglesOneAboveWater(currentTrio);
                }
            }
        }
        //Build the new triangles where one of the old vertices is above the water
        private void AddTrianglesOneAboveWater(List<VertexData> vertexData)
        {
            //H is always at position 0
            Vector3 H = vertexData[0].position;

            //Left of H is M
            //Right of H is L

            //Find the index of M
            int M_index = vertexData[0].index - 1;
            if (M_index < 0)
            {
                M_index = 2;
            }

            //We also need the heights to water
            float h_H = vertexData[0].distance;
            float h_M = 0f;
            float h_L = 0f;

            Vector3 M = Vector3.zero;
            Vector3 L = Vector3.zero;

            //This means M is at position 1 in the List
            if (vertexData[1].index == M_index)
            {
                M = vertexData[1].position;
                L = vertexData[2].position;

                h_M = vertexData[1].distance;
                h_L = vertexData[2].distance;
            }
            else
            {
                M = vertexData[2].position;
                L = vertexData[1].position;

                h_M = vertexData[2].distance;
                h_L = vertexData[1].distance;
            }


            //Now we can calculate where we should cut the triangle to form 2 new triangles
            //because the resulting area will always form a square

            //Point I_M
            Vector3 MH = H - M;

            float t_M = -h_M / (h_H - h_M);

            Vector3 MI_M = t_M * MH;

            Vector3 I_M = MI_M + M;


            //Point I_L
            Vector3 LH = H - L;

            float t_L = -h_L / (h_H - h_L);

            Vector3 LI_L = t_L * LH;

            Vector3 I_L = LI_L + L;


            //Save the data, such as normal, area, etc      
            //2 triangles below the water  
            triangleData.Add(new TriangleData(M, I_M, I_L));
            triangleData.Add(new TriangleData(M, I_L, L));
        }
        //Build the new triangles where two of the old vertices are above the water
        private void AddTrianglesTwoAboveWater(List<VertexData> vertexData)
        {
            //H and M are above the water
            //H is after the vertice that's below water, which is L
            //So we know which one is L because it is last in the sorted list
            Vector3 L = vertexData[2].position;

            //Find the index of H
            int H_index = vertexData[2].index + 1;
            if (H_index > 2)
            {
                H_index = 0;
            }


            //We also need the heights to water
            float h_L = vertexData[2].distance;
            float h_H = 0f;
            float h_M = 0f;

            Vector3 H = Vector3.zero;
            Vector3 M = Vector3.zero;

            //This means that H is at position 1 in the list
            if (vertexData[1].index == H_index)
            {
                H = vertexData[1].position;
                M = vertexData[0].position;

                h_H = vertexData[1].distance;
                h_M = vertexData[0].distance;
            }
            else
            {
                H = vertexData[0].position;
                M = vertexData[1].position;

                h_H = vertexData[0].distance;
                h_M = vertexData[1].distance;
            }


            //Now we can find where to cut the triangle

            //Point J_M
            Vector3 LM = M - L;

            float t_M = -h_L / (h_M - h_L);

            Vector3 LJ_M = t_M * LM;

            Vector3 J_M = LJ_M + L;


            //Point J_H
            Vector3 LH = H - L;

            float t_H = -h_L / (h_H - h_L);

            Vector3 LJ_H = t_H * LH;

            Vector3 J_H = LJ_H + L;


            //Save the data, such as normal, area, etc
            //1 triangle below the water
            triangleData.Add(new TriangleData(L, J_H, J_M));
        }

        private void ShowUnderwaterMesh()
        {
            foreach (TriangleData triangle in triangleData)
            {
                Gizmos.DrawLine(triangle.vertexA, triangle.vertexB);
                Gizmos.DrawLine(triangle.vertexB, triangle.vertexC);
                Gizmos.DrawLine(triangle.vertexC, triangle.vertexA);
            }
        }

        public struct VertexData
        {
            public int index;
            public Vector3 position;
            public float distance;

            public VertexData(int _index, Vector3 _position, float _distance)
            {
                index = _index;
                position = _position;
                distance = _distance;
            }
        }
        public struct TriangleData
        {
            public Vector3 vertexA, vertexB, vertexC;
            public Vector3 center;
            public Vector3 normal;
            public float area;

            public TriangleData(Vector3 _vertexA, Vector3 _vertexB, Vector3 _vertexC)
            {
                vertexA = _vertexA;
                vertexB = _vertexB;
                vertexC = _vertexC;

                center = (vertexA + vertexB + vertexC) / 3f;
                normal = Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;

                float distanceAB = Vector3.Distance(vertexA, vertexB);
                float distanceAC = Vector3.Distance(vertexA, vertexC);
                area = (distanceAB * distanceAC * Mathf.Sin(Vector3.Angle(vertexB - vertexA, vertexC - vertexA) * Mathf.Deg2Rad)) / 2f;
            }
        }
    }
}