using UnityEngine;
using MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using g3;

using TriangleNet.Geometry;

namespace UnityHelpers
{
    public static class MeshHelpers
    {
        public const int MAX_VERTICES = 65534;
        public enum TriangleSearchType { all, any, none, }
        
        /// <summary>
        /// Triangulates a concave polygon using Triangle.Net
        /// 
        /// Source: https://forum.unity.com/threads/using-triangle-net-with-unity-5-triangulation-of-meshes-made-easy.442072/
        /// </summary>
        /// <param name="points">The points making up the polygon</param>
        /// <param name="holes">Any holes within the polygon</param>
        /// <param name="outIndices">The indices array making up the triangles</param>
        /// <param name="outVertices">The new vertices array of the polygon</param>
        public static void TriangulateConcavePolygon(this IEnumerable<Vector2> points, IEnumerable<IEnumerable<Vector2>> holes, out IEnumerable<int> outIndices, out IEnumerable<Vector2> outVertices)
        {
            var poly = new Polygon();

            for (int i = 0; i < points.Count(); i++)
            {
                var currentPoint = points.ElementAt(i);
                var nextPoint = points.ElementAt((i + 1) % points.Count());
                poly.Add(new Vertex(currentPoint.x, currentPoint.y));
                poly.Add(new Segment(new Vertex(currentPoint.x, currentPoint.y), new Vertex(nextPoint.x, nextPoint.y)));
            }

            if (holes != null)
            {
                var nestedVertices = holes.Select((subHole) => subHole.Select((point) => new Vertex(point.x, point.y)));
                foreach (var vertices in nestedVertices)
                    poly.Add(new Contour(vertices), true);
            }

            var mesh = poly.Triangulate();

            List<Vector2> tempVertices = new List<Vector2>();
            List<int> tempIndices = new List<int>();
            foreach (var triangle in mesh.Triangles)
            {
                for (int j = 2; j >= 0; j--)
                {
                    bool found = false;
                    for (int k = 0; k < tempVertices.Count; k++)
                    {
                        if (tempVertices[k].x == triangle.GetVertex(j).X && tempVertices[k].y == triangle.GetVertex(j).Y)
                        {
                            tempIndices.Add(k);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        tempVertices.Add(new Vector3((float)triangle.GetVertex(j).X, (float)triangle.GetVertex(j).Y));
                        tempIndices.Add(tempVertices.Count - 1);
                    }
                }
            }

            outVertices = tempVertices;
            outIndices = tempIndices;
        }
        /// <summary>
        /// Triangulates a concave polygon using Triangle.Net
        /// 
        /// Source: https://forum.unity.com/threads/using-triangle-net-with-unity-5-triangulation-of-meshes-made-easy.442072/
        /// </summary>
        /// <param name="points">The points making up the polygon</param>
        /// <param name="outIndices">The indices array making up the triangles</param>
        /// <param name="outVertices">The new vertices array of the polygon</param>
        public static void TriangulateConcavePolygon(this IEnumerable<Vector2> points, out IEnumerable<int> outIndices, out IEnumerable<Vector2> outVertices)
        {
            TriangulateConcavePolygon(points, null, out outIndices, out outVertices);
        }

        /// <summary>
        /// Triangulates the given polygon using the ear clip method.
        /// The algorithm supports concave polygons, but not polygons with holes, or multiple polygons at once.
        /// The direction the face depends on the order of the given vertices.
        /// Simply reversing the order will flip the direction.
        /// 
        /// Source: https://wiki.unity3d.com/index.php/Triangulator
        /// </summary>
        /// <param name="points">The points that make up the polygon</param>
        /// <returns>The indices array making up the triangles</returns>
        public static IEnumerable<int> TriangulatePolygonWithEarClipping(this IEnumerable<Vector2> points)
        {
            List<Vector2> m_points = points.ToList();
            List<int> indices = new List<int>();

            int n = m_points.Count;
            if (n < 3)
                return indices.ToArray();
    
            int[] V = new int[n];
            if (Area(m_points) > 0)
            {
                for (int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for (int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }
    
            int nv = n;
            int count = 2 * nv;
            for (int v = nv - 1; nv > 2; )
            {
                if ((count--) <= 0)
                    return indices.ToArray();
    
                int u = v;
                if (nv <= u)
                    u = 0;
                v = u + 1;
                if (nv <= v)
                    v = 0;
                int w = v + 1;
                if (nv <= w)
                    w = 0;
    
                if (Snip(m_points, u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    for (s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }
    
            indices.Reverse();
            return indices.ToArray();
        }
        private static float Area(List<Vector2> m_points)
        {
            int n = m_points.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = m_points[p];
                Vector2 qval = m_points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }
        private static bool Snip(List<Vector2> m_points, int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector2 A = m_points[V[u]];
            Vector2 B = m_points[V[v]];
            Vector2 C = m_points[V[w]];
            if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for (p = 0; p < n; p++)
            {
                if ((p == u) || (p == v) || (p == w))
                    continue;
                Vector2 P = m_points[V[p]];
                if (P.IsPointInTriangle(A, B, C))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Checks to see if the given points lies inside the given triangle
        /// </summary>
        /// <param name="P">The point in question</param>
        /// <param name="A">First corner of the triangle</param>
        /// <param name="B">Second corner of the triangle</param>
        /// <param name="C">Third corner of the triangle</param>
        /// <returns>True if the point is inside the triangle, false otherwise</returns>
        public static bool IsPointInTriangle (this Vector2 P, Vector2 A, Vector2 B, Vector2 C) {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;
    
            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;
    
            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;
    
            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }

        /// <summary>
        /// Checks if a triangle's points are in clockwise order
        /// 
        /// Sources: https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        ///          https://en.wikipedia.org/wiki/Determinant
        /// </summary>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        /// <param name="pointC">Third point</param>
        /// <returns>True if clockwise and false if not</returns>
        public static bool IsTriangleOrientedClockwise(Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            // | x1 y1 1 |
            // | x2 y2 1 | = (x1 * y2 * 1) + (y1 * 1 * x3) + (1 * x2 * y3) - (1 * y2 * x3) - (y1 * x2 * 1) - (x1 * 1 * y3)
            // | x3 y3 1 |

            float determinant = (pointA.x * pointB.y * 1) + (pointA.y * 1 * pointC.x) + (1 * pointB.x * pointC.y) - (1 * pointB.y * pointC.x) - (pointA.y * pointB.x * 1) - (pointA.x * 1 * pointC.y);
            return determinant < -float.Epsilon;
        }
        /// <summary>
        /// Checks if a triangle's points are in clockwise order
        /// 
        /// Sources: https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        ///          https://en.wikipedia.org/wiki/Determinant
        /// </summary>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        /// <param name="pointC">Third point</param>
        /// <returns>True if clockwise and false if not</returns>
        public static bool IsTriangleOrientedAntiClockwise(Vector2 pointA, Vector2 pointB, Vector2 pointC)
        {
            // | x1 y1 1 |
            // | x2 y2 1 | = (x1 * y2 * 1) + (y1 * 1 * x3) + (1 * x2 * y3) - (1 * y2 * x3) - (y1 * x2 * 1) - (x1 * 1 * y3)
            // | x3 y3 1 |

            float determinant = (pointA.x * pointB.y * 1) + (pointA.y * 1 * pointC.x) + (1 * pointB.x * pointC.y) - (1 * pointB.y * pointC.x) - (pointA.y * pointB.x * 1) - (pointA.x * 1 * pointC.y);
            return determinant > float.Epsilon;
        }

        /// <summary>
        /// Appends the vertices, triangle, normals, uv, uv2, uv3, uv4 of the other mesh to the current mesh.
        /// </summary>
        /// <param name="current">The mesh to append to</param>
        /// <param name="other">The mesh that will be appended</param>
        /// <param name="position">Position of TRS matrix</param>
        /// <param name="rotation">Rotation of TRS matrix</param>
        /// <param name="scale">Scale of TRS matrix</param>
        /// <returns>False if the mesh being appended to cannot fit what is being appended</returns>
        public static bool Append(this MeshData current, MeshData other, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (current.vertices.Length + other.vertices.Length < MAX_VERTICES)
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
                return true;
            }
            else
                Debug.LogWarning("MeshHelpers: Could not append mesh to other mesh");

            return false;
        }
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
            if (current.vertices.Length + other.vertices.Length < MAX_VERTICES)
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
                return true;
            }
            else
                Debug.LogWarning("MeshHelpers: Could not append mesh to other mesh");

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
        /// Shifts all the indices within the enumerable by the given amount
        /// </summary>
        /// <param name="triangles">The triangles sequence</param>
        /// <param name="shiftAmount">The amount to shift by</param>
        /// <returns>A shifted triangle sequence</returns>
        public static IEnumerable<int> ShiftTriangleIndices(this IEnumerable<int> triangles, int shiftAmount)
        {
            return triangles.Select((index) => index + shiftAmount);
        }
        /// <summary>
        /// Finds all triangles with values within the given range
        /// </summary>
        /// <param name="triangles">The triangles sequence</param>
        /// <param name="startIndex">The beginning of the range [inclusive]</param>
        /// <param name="endIndex">The end of the range [inclusive]</param>
        /// <param name="searchType">If set to all then all three of the triangle values must be within the range. If set to any then only one has to comply for the triangle to make the cut. If set to none then none of the triangle values have to be within the range to be added.</param>
        /// <returns>A subset of the original triangles sequence</returns>
        public static IEnumerable<int> FindTriangles(this IEnumerable<int> triangles, int startIndex, int endIndex, TriangleSearchType searchType = TriangleSearchType.all)
        {
            int[] currentTriangle = new int[3];
            List<int> containedTriangles = new List<int>();
            int currentIndex = 0;
            bool hadGoodIndex = false;
            bool hadBadIndex = false;
            bool toBeAdded = false;
            var enumerator = triangles.GetEnumerator();
            while (enumerator.MoveNext())
            {
                int subIndex = currentIndex % 3;
                if (currentIndex > 0 && subIndex == 0 && toBeAdded)
                {
                    containedTriangles.AddRange(currentTriangle);
                    hadGoodIndex = false;
                    hadBadIndex = false;
                }

                if (enumerator.Current >= startIndex && enumerator.Current <= endIndex)
                    hadGoodIndex = true;
                else
                    hadBadIndex = true;
                
                currentTriangle[subIndex] = enumerator.Current;
                currentIndex++;
                toBeAdded = ((searchType == TriangleSearchType.all && !hadBadIndex) || (searchType == TriangleSearchType.any && hadGoodIndex) || (searchType == TriangleSearchType.none && !hadGoodIndex));
            }
            //The while loop misses the last triangle, so we add it as long as there actually were triangles to begin with
            if (triangles.Count() > 0 && toBeAdded)
                containedTriangles.AddRange(currentTriangle);

            return containedTriangles;
        }

        /// <summary>
        /// Checks if a point is on the surface of the object's mesh. This method goes through all the triangles of the mesh.
        /// This variant is faster, but it is less accurate. Works better with meshes that have triangles with consistently small areas around the size of the given range.
        /// </summary>
        /// <param name="currentObject">The object to be checked. Must have a MeshFilter component attached.</param>
        /// <param name="point">The point to be checked.</param>
        /// <param name="range">The distance from the point to check. (Clamped between 0.1 and MaxValue)</param>
        /// <returns>True if the point is on the surface of the mesh up to a certain range.</returns>
        public static bool IsPointOnSurface(this Transform currentObject, Vector3 point, float range)
        {
            bool onSurface = false;
            MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                onSurface = mesh.IsPointOnSurface(currentObject.InverseTransformPoint(point), range);
            }
            return onSurface;
        }
        /// <summary>
        /// Checks if a point is on the surface of the mesh. This method goes through all the triangles of the mesh.
        /// This variant is faster, but it is less accurate. Works better with meshes that have triangles with consistently small areas around the size of the given range.
        /// </summary>
        /// <param name="mesh">The mesh to be checked.</param>
        /// <param name="point">The point to be checked.</param>
        /// <param name="range">The distance from the point to check. (Clamped between 0.1 and MaxValue)</param>
        /// <returns>True if the point is on the surface of the mesh up to a certain range.</returns>
        public static bool IsPointOnSurface(this Mesh mesh, Vector3 point, float range)
        {
            bool onSurface = false;

            Bounds pointBounds = new Bounds(point, Vector3.one * Mathf.Clamp(range, 0.1f, float.MaxValue));

            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 vertexA = mesh.vertices[mesh.triangles[i + 0]];
                Vector3 vertexB = mesh.vertices[mesh.triangles[i + 1]];
                Vector3 vertexC = mesh.vertices[mesh.triangles[i + 2]];

                if (pointBounds.Contains(vertexA) || pointBounds.Contains(vertexB) || pointBounds.Contains(vertexC) || pointBounds.Contains(CalculateTriangleCenter(vertexA, vertexB, vertexC)))
                    onSurface = true;
            }
            return onSurface;
        }
        /// <summary>
        /// Checks if a point is on the surface of the object's mesh. This method goes through all the triangles of the mesh.
        /// </summary>
        /// <param name="currentObject">The object to be checked. Must have a MeshFilter component attached.</param>
        /// <param name="point">The point to be checked.</param>
        /// <returns>True if the point is on the surface of the mesh.</returns>
        public static bool IsPointOnSurface(this Transform currentObject, Vector3 point)
        {
            bool onSurface = false;
            MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                onSurface = mesh.IsPointOnSurface(currentObject.InverseTransformPoint(point));
            }
            return onSurface;
        }
        /// <summary>
        /// Checks if a point is on the surface of the mesh. This method goes through all the triangles of the mesh.
        /// </summary>
        /// <param name="mesh">The mesh to be checked.</param>
        /// <param name="point">The point to be checked.</param>
        /// <returns>True if the point is on the surface of the mesh.</returns>
        public static bool IsPointOnSurface(this Mesh mesh, Vector3 point)
        {
            bool onSurface = false;
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                Vector3 triangleA = mesh.vertices[mesh.triangles[i + 0]];
                Vector3 triangleB = mesh.vertices[mesh.triangles[i + 1]];
                Vector3 triangleC = mesh.vertices[mesh.triangles[i + 2]];

                if (PointInTriangle(triangleA, triangleB, triangleC, point))
                {
                    onSurface = true;
                }
            }
            return onSurface;
        }
        /// <summary>
        /// Checks if a point is inside of the object's mesh. This function is slow due to the need to build a spatial data structure using geometry3Sharp.
        /// </summary>
        /// <param name="currentObject">The object to be checked. Must have a MeshFilter component attached.</param>
        /// <param name="point">The point in question.</param>
        /// <returns>Whether the point is inside the mesh.</returns>
        public static bool IsPointInside(this Transform currentObject, Vector3 point)
        {
            bool isInside = false;
            MeshFilter meshFilter = currentObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                Mesh mesh = meshFilter.sharedMesh;
                isInside = mesh.IsPointInside(currentObject.InverseTransformPoint(point));
            }

            return isInside;
        }
        /// <summary>
        /// Checks if a point is inside of a mesh. This function is slow due to the need to build a spatial data structure using geometry3Sharp.
        /// </summary>
        /// <param name="mesh">The mesh to be checked.</param>
        /// <param name="point">The point in question.</param>
        /// <returns>Whether the point is inside the mesh.</returns>
        public static bool IsPointInside(this Mesh mesh, Vector3 point)
        {
            var mesh3 = GenerateDynamicMesh(mesh.vertices, mesh.triangles, mesh.normals);
            var spatial = new DMeshAABBTree3(mesh3);
            spatial.Build();
            return spatial.IsInside(new Vector3d(point.x, point.y, point.z));
        }
        /// <summary>
        /// Gets the center of a triangle given it's three points.
        /// </summary>
        /// <param name="vertexA">First point of the triangle</param>
        /// <param name="vertexB">Second point of the triangle</param>
        /// <param name="vertexC">Third point of the triangle</param>
        /// <returns>The center of the triangle</returns>
        public static Vector3 CalculateTriangleCenter(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            return (vertexA + vertexB + vertexC) / 3f;
        }
        /// <summary>
        /// Calculates the normal of the triangle given it's three points.
        /// </summary>
        /// <param name="vertexA">First point of the triangle</param>
        /// <param name="vertexB">Second point of the triangle</param>
        /// <param name="vertexC">Third point of the triangle</param>
        /// <returns>The direction perpendicular to the triangle's face</returns>
        public static Vector3 CalculateTriangleNormal(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            return Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
        }
        /// <summary>
        /// Calculates the area of the triangle given it's three points.
        /// </summary>
        /// <param name="vertexA">First point of the triangle</param>
        /// <param name="vertexB">Second point of the triangle</param>
        /// <param name="vertexC">Third point of the triangle</param>
        /// <returns>The surface area of the face of the triangle</returns>
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

        /// <summary>
        /// Reduces the triangle count of the given mesh.
        /// </summary>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        /// <param name="normals">The normals of the mesh.</param>
        /// <param name="trianglePercent">The percent of the original triangle count the final triangle count should be.</param>
        /// <returns>The decimated mesh.</returns>
        public static MeshData DecimateByTriangleCount(IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Vector3> normals, float trianglePercent = 0.5f)
        {
            var mesh = GenerateDynamicMesh(vertices, triangles, normals);
            var reducer = new Reducer(mesh);
            reducer.ReduceToTriangleCount(Mathf.RoundToInt(Mathf.Clamp01(trianglePercent) * (triangles.Count() / 3)));
            return new MeshData(reducer.Mesh);
        }
        /// <summary>
        /// Reduces the vertex count of the given mesh.
        /// </summary>
        /// <param name="vertices">The vertices of the mesh.</param>
        /// <param name="triangles">The triangles of the mesh.</param>
        /// <param name="normals">The normals of the mesh.</param>
        /// <param name="vertexPercent">The percent of the original vertex count the final vertex count should be.</param>
        /// <returns>The decimated mesh.</returns>
        public static MeshData DecimateByVertexCount(IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Vector3> normals, float vertexPercent = 0.5f)
        {
            var mesh = GenerateDynamicMesh(vertices, triangles, normals);
            var reducer = new Reducer(mesh);
            reducer.ReduceToVertexCount(Mathf.RoundToInt(Mathf.Clamp01(vertexPercent) * vertices.Count()));
            return new MeshData(reducer.Mesh);
        }
        private static DMesh3 GenerateDynamicMesh(IEnumerable<Vector3> vertices, IEnumerable<int> triangles, IEnumerable<Vector3> normals)
        {
            return DMesh3Builder.Build(vertices.Select(vertex => new Vector3f(vertex.x, vertex.y, vertex.z)), triangles, normals.Select(vector => new Vector3f(vector.x, vector.y, vector.z)));
        }

        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="vertices">The original vertices of a mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>The output mesh data of the convex hull</returns>
        public static MeshData GenerateConvexHull(this Vector3[] vertices, out ConvexHullCreationResultOutcome resultOutcome, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            MeshData convexMesh = new MeshData();

            List<MIVertex> convexVertices = vertices.Select(point => new MIVertex(point)).ToList();
            var creation = ConvexHull.Create(convexVertices, PlaneDistanceTolerance);
            resultOutcome = creation.Outcome;

            var result = creation.Result;

            List<int> triangles = new List<int>();
            List<MIVertex> resultVertices = result.Points.ToList();
            foreach (var face in result.Faces)
            {
                triangles.Add(resultVertices.IndexOf(face.Vertices[0]));
                triangles.Add(resultVertices.IndexOf(face.Vertices[1]));
                triangles.Add(resultVertices.IndexOf(face.Vertices[2]));
            }

            convexMesh.vertices = result.Points.Select(point => point.ToVec()).ToArray();
            convexMesh.triangles = triangles.ToArray();
            //convexMesh.RecalculateNormals();
            //convexMesh.RecalculateBounds();

            return convexMesh;
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="original">The original mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>The output mesh data of the convex hull</returns>
        public static MeshData GenerateConvexHull(this Mesh original, out ConvexHullCreationResultOutcome resultOutcome, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            return original.vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="vertices">The original vertices of a mesh</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>The output mesh data of the convex hull</returns>
        public static MeshData GenerateConvexHull(this Vector3[] vertices, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            ConvexHullCreationResultOutcome resultOutcome;
            return vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="original">The original mesh</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>The output mesh data of the convex hull</returns>
        public static MeshData GenerateConvexHull(this Mesh original, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            ConvexHullCreationResultOutcome resultOutcome;
            return original.vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="original">The original mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>A mesh that represents the convex hull of the original mesh</returns>
        public static Mesh ToConvexHull(this Mesh original, out ConvexHullCreationResultOutcome resultOutcome, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            Mesh convexMesh = new Mesh();
            var meshData = original.vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
            convexMesh.vertices = meshData.vertices;
            convexMesh.triangles = meshData.triangles;
            convexMesh.RecalculateNormals();
            convexMesh.RecalculateBounds();

            return convexMesh;
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="original">The original mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>A mesh that represents the convex hull of the original mesh</returns>
        public static Mesh ToConvexHull(this Mesh original, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            Mesh convexMesh = new Mesh();
            ConvexHullCreationResultOutcome resultOutcome;
            var meshData = original.vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
            convexMesh.vertices = meshData.vertices;
            convexMesh.triangles = meshData.triangles;
            convexMesh.RecalculateNormals();
            convexMesh.RecalculateBounds();

            return convexMesh;
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="vertices">The original vertices of a mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>A mesh that represents the convex hull of the original mesh</returns>
        public static Mesh ToConvexHull(this Vector3[] vertices, out ConvexHullCreationResultOutcome resultOutcome, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            Mesh convexMesh = new Mesh();
            var meshData = vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
            convexMesh.vertices = meshData.vertices;
            convexMesh.triangles = meshData.triangles;
            convexMesh.RecalculateNormals();
            convexMesh.RecalculateBounds();

            return convexMesh;
        }
        /// <summary>
        /// Creates a convex hull given the original vertices of a mesh.
        /// </summary>
        /// <param name="vertices">The original vertices of a mesh</param>
        /// <param name="resultOutcome">The outcome of the process (successfull or unsuccessful)</param>
        /// <param name="PlaneDistanceTolerance">The plane distance tolerance</param>
        /// <returns>A mesh that represents the convex hull of the original mesh</returns>
        public static Mesh ToConvexHull(this Vector3[] vertices, double PlaneDistanceTolerance = Constants.DefaultPlaneDistanceTolerance)
        {
            Mesh convexMesh = new Mesh();
            ConvexHullCreationResultOutcome resultOutcome;
            var meshData = vertices.GenerateConvexHull(out resultOutcome, PlaneDistanceTolerance);
            convexMesh.vertices = meshData.vertices;
            convexMesh.triangles = meshData.triangles;
            convexMesh.RecalculateNormals();
            convexMesh.RecalculateBounds();

            return convexMesh;
        }

        /// <summary>
        /// For triangulation with the ear clipping function
        /// </summary>
        private class VertexEarInfo
        {
            public int originalIndex;
            public Vector2 point;
            public VertexEarInfo nextVertex;
            public VertexEarInfo prevVertex;
            public bool isReflex;
            public bool isConvex;
        }

        /// <summary>
        /// The vertex proxy between MIConvexHull and Unity.
        /// </summary>
        private class MIVertex : IVertex
        {
            public double[] Position { get; set; }
            public MIVertex(double x, double y, double z)
            {
                Position = new double[3] { x, y, z };
            }
            public MIVertex(Vector3 ver)
            {
                Position = new double[3] { ver.x, ver.y, ver.z };
            }
            public Vector3 ToVec()
            {
                return new Vector3((float)Position[0], (float)Position[1], (float)Position[2]);
            }
        }

        /// <summary>
        /// The mesh data class is used to help with the creation of
        /// meshes. The main reason for having a separate class is to
        /// allow the manipulation of meshes on separate threads.
        /// </summary>
        public class MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;
            public Vector3[] normals;
            public Color[] colors;
            public Vector2[] uv;
            public Vector2[] uv2;
            public Vector2[] uv3;
            public Vector2[] uv4;

            public MeshData()
            {
                vertices = new Vector3[0];
                triangles = new int[0];
                normals = new Vector3[0];
                colors = new Color[0];
                uv = new Vector2[0];
                uv2 = new Vector2[0];
                uv3 = new Vector2[0];
                uv4 = new Vector2[0];
            }
            public MeshData(Mesh _mesh) : this(_mesh.vertices, _mesh.triangles, _mesh.normals, _mesh.colors, _mesh.uv)
            {
                uv2 = _mesh.uv2;
                uv3 = _mesh.uv3;
                uv4 = _mesh.uv4;
            }
            public MeshData(IEnumerable<Vector3> _vertices, IEnumerable<int> _triangles) : this()
            {
                vertices = _vertices.ToArray();
                triangles = _triangles.ToArray();
            }
            public MeshData(IEnumerable<Vector3> _vertices, IEnumerable<int> _triangles, IEnumerable<Vector3> _normals) : this(_vertices, _triangles)
            {
                normals = _normals.ToArray();
            }
            public MeshData(IEnumerable<Vector3> _vertices, IEnumerable<int> _triangles, IEnumerable<Vector3> _normals, IEnumerable<Color> _colors) : this(_vertices, _triangles, _normals)
            {
                colors = _colors.ToArray();
            }
            public MeshData(IEnumerable<Vector3> _vertices, IEnumerable<int> _triangles, IEnumerable<Vector3> _normals, IEnumerable<Vector2> _uv) : this(_vertices, _triangles, _normals)
            {
                uv = _uv.ToArray();
            }
            public MeshData(IEnumerable<Vector3> _vertices, IEnumerable<int> _triangles, IEnumerable<Vector3> _normals, IEnumerable<Color> _colors, IEnumerable<Vector2> _uv) : this(_vertices, _triangles, _normals, _colors)
            {
                uv = _uv.ToArray();
            }
            public MeshData(DMesh3 _mesh) : this()
            {
                _mesh = new DMesh3(_mesh, true);
                vertices = _mesh.VertexIndices().Select(vID => { var vertex = _mesh.GetVertexf(vID); return new Vector3(vertex.x, vertex.y, vertex.z); }).ToArray();
                triangles = _mesh.TriangleIndices().SelectMany(tID => _mesh.GetTriangle(tID).array).ToArray();
            }
            public void Dispose()
            {
                vertices = null;
                triangles = null;
                normals = null;
                uv = null;
                uv2 = null;
                uv3 = null;
                uv4 = null;
            }

            public Mesh GenerateMesh()
            {
                Mesh mesh = new Mesh();
                mesh.name = "Custom Mesh";

                mesh.Clear();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.colors = colors;
                if (uv.Length > 0)
                    mesh.uv = uv;
                if (uv2.Length > 0)
                    mesh.uv2 = uv2;
                if (uv3.Length > 0)
                    mesh.uv3 = uv3;
                if (uv4.Length > 0)
                    mesh.uv4 = uv4;
                if (normals.Length == vertices.Length)
                    mesh.normals = normals;
                else
                {
                    Debug.LogWarning("MeshData: Normals not same length as vertices, recalculating normals");
                    mesh.RecalculateNormals();
                }

                return mesh;
            }
            public System.Collections.IEnumerator DebugNormals()
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Debug.DrawRay(vertices[i], normals[i], Color.blue, 10000);
                    if (i % 100 == 0)
                        yield return null;
                }
            }
        }
    }
}