using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public static class JigsawPuzzle
    {
        public static IEnumerator Generate(int columns, int rows, float puzzleWidth, float puzzleHeight, float puzzleDepth, int edgeSmoothness = 5, float seed = 1337, Transform parent = null, GameObject[] piecesOutput = null, Material _faceMaterial = null, Material _sideMaterial = null, Material _bottomMaterial = null, bool addBoxCollider = false)
        {
            float pieceWidth = puzzleWidth / columns;
            float pieceHeight = puzzleHeight / rows;
            float halfWidth = pieceWidth / 2;
            float halfHeight = pieceHeight / 2;
            float halfDepth = puzzleDepth / 2;

            Vector3 topBackLeftCorner = new Vector3(-halfWidth, halfDepth, -halfHeight);
            Vector3 topBackRightCorner = new Vector3(halfWidth, halfDepth, -halfHeight);
            Vector3 topFrontRightCorner = new Vector3(halfWidth, halfDepth, halfHeight);
            Vector3 topFrontLeftCorner = new Vector3(-halfWidth, halfDepth, halfHeight);
            Vector3 bottomFrontLeftCorner = new Vector3(-halfWidth, -halfDepth, halfHeight);
            Vector3 bottomFrontRightCorner = new Vector3(halfWidth, -halfDepth, halfHeight);
            Vector3 bottomBackRightCorner = new Vector3(halfWidth, -halfDepth, -halfHeight);
            Vector3 bottomBackLeftCorner = new Vector3(-halfWidth, -halfDepth, -halfHeight);

            var backFlat = new Vector3[] { topBackRightCorner, topBackLeftCorner, bottomBackRightCorner, bottomBackLeftCorner };
            var frontFlat = new Vector3[] { topFrontLeftCorner, topFrontRightCorner, bottomFrontLeftCorner, bottomFrontRightCorner };
            var rightFlat = new Vector3[] { topFrontRightCorner, topBackRightCorner, bottomFrontRightCorner, bottomBackRightCorner };
            var leftFlat = new Vector3[] { topBackLeftCorner, topFrontLeftCorner, bottomBackLeftCorner, bottomFrontLeftCorner };

            var firstMaterial = _faceMaterial;
            var secondMaterial = _bottomMaterial;
            var thirdMaterial = _sideMaterial;
            if (firstMaterial == null)
            {
                firstMaterial = new Material(Shader.Find("Standard"));
                firstMaterial.color = Color.green;
            }
            if (secondMaterial == null)
            {
                secondMaterial = new Material(Shader.Find("Standard"));
                secondMaterial.color = Color.red;
            }
            if (thirdMaterial == null)
            {
                thirdMaterial = new Material(Shader.Find("Standard"));
                thirdMaterial.color = Color.yellow;
            }

            JigsawPiece[,] piecesMap = new JigsawPiece[columns, rows];
            GameObject[] pieces = null;
            if (piecesOutput != null)
            {
                if (piecesOutput.Length == columns * rows)
                    pieces = piecesOutput;
                else
                    Debug.LogError("Given pieces array is not large enough to store all the jigsaw pieces, must be of length equal to " + columns + " * " + rows);
            }

            Dictionary<int, Vector2[]> simpleUVs = new Dictionary<int, Vector2[]>();
            Dictionary<int, List<int>> simpleTriangulations = new Dictionary<int, List<int>>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    float xOffset = (puzzleWidth / 2 - halfWidth);
                    float zOffset = (puzzleHeight / 2 - halfHeight);
                    float xPos = col * pieceWidth - xOffset;
                    float zPos = row * pieceHeight - zOffset;
                    
                    JigsawPiece currentPiece = JigsawPiece.GenerateRandom(pieceWidth, pieceHeight, seed, xPos, zPos);
                    if (col > 0)
                        currentPiece.left = piecesMap[col - 1, row].right.CreateSpouse(pieceWidth);
                    if (row > 0)
                        currentPiece.bottom = piecesMap[col, row - 1].top.CreateSpouse(pieceHeight);
                    piecesMap[col, row] = currentPiece;

                    int pieceIndex = row * columns + col;
                    GameObject pieceObject = new GameObject("Piece#" + pieceIndex);
                    pieceObject.transform.localPosition = new Vector3(xPos, halfDepth, zPos);
                    var meshPart = pieceObject.AddComponent<MeshPart>();
                    if (parent != null)
                        pieceObject.transform.SetParent(parent);
                    if (pieces != null)
                        pieces[pieceIndex] = pieceObject;
                    if (addBoxCollider)
                    {
                        var boxCollider = pieceObject.AddComponent<BoxCollider>();
                        boxCollider.size = new Vector3(pieceWidth, puzzleDepth, pieceHeight);
                    }
                                    
                    bool hasTop = row < rows - 1;
                    bool hasRight = col < columns - 1;
                    bool hasBottom = row > 0;
                    bool hasLeft = col > 0;
                    var twoDeePoints = currentPiece.EvaluateAll(pieceWidth, pieceHeight, edgeSmoothness, hasTop, hasRight, hasBottom, hasLeft);
                    IEnumerable<int> indices;
                    IEnumerable<Vector2> triangulatedVertices;
                    twoDeePoints.TriangulateConcavePolygon(out indices, out triangulatedVertices);
                    var faceUV = triangulatedVertices.Select(point => (point + new Vector2(xPos + (puzzleWidth / 2), zPos + (puzzleHeight / 2))).Multiply(new Vector2(1 / puzzleWidth, 1 / puzzleHeight)));
                    
                    //Top side
                    meshPart.AddVertices(triangulatedVertices.Select((point) => point.ToXZVector3(halfDepth)), indices, faceUV);
                    //Bottom side
                    meshPart.AddVertices(triangulatedVertices.Select((point) => point.ToXZVector3(-halfDepth)), indices.Reverse(), faceUV, 1);

                    IEnumerable<Vector3> sideVertices;
                    List<int> sideTriangles;
                    Vector2[] sideUVs;

                    //Back side
                    sideVertices = GetSide(hasBottom, (piece, smoothness) => piece.EvaluateBottom(smoothness), currentPiece, edgeSmoothness, halfDepth, backFlat);
                    sideUVs = GetSimpleUVNation(simpleUVs, sideVertices.Count());
                    sideTriangles = GetSimpleTriangulation(simpleTriangulations, sideUVs.Length / 2);
                    meshPart.AddVertices(sideVertices, sideTriangles, sideUVs, 2);
                    //Front side
                    sideVertices = GetSide(hasTop, (piece, smoothness) => piece.EvaluateTop(smoothness), currentPiece, edgeSmoothness, halfDepth, frontFlat);
                    sideUVs = GetSimpleUVNation(simpleUVs, sideVertices.Count());
                    sideTriangles = GetSimpleTriangulation(simpleTriangulations, sideUVs.Length / 2);
                    meshPart.AddVertices(sideVertices, sideTriangles, sideUVs, 2);
                    //Right side
                    sideVertices = GetSide(hasRight, (piece, smoothness) => piece.EvaluateRight(smoothness), currentPiece, edgeSmoothness, halfDepth, rightFlat);
                    sideUVs = GetSimpleUVNation(simpleUVs, sideVertices.Count());
                    sideTriangles = GetSimpleTriangulation(simpleTriangulations, sideUVs.Length / 2);
                    meshPart.AddVertices(sideVertices, sideTriangles, sideUVs, 2);
                    //Left side
                    sideVertices = GetSide(hasLeft, (piece, smoothness) => piece.EvaluateLeft(smoothness), currentPiece, edgeSmoothness, halfDepth, leftFlat);
                    sideUVs = GetSimpleUVNation(simpleUVs, sideVertices.Count());
                    sideTriangles = GetSimpleTriangulation(simpleTriangulations, sideUVs.Length / 2);
                    meshPart.AddVertices(sideVertices, sideTriangles, sideUVs, 2);

                    Dictionary<int, Material> matsDick = new Dictionary<int, Material>();
                    matsDick[0] = firstMaterial;
                    matsDick[1] = secondMaterial;
                    matsDick[2] = thirdMaterial;

                    meshPart.SetMaterials(matsDick);

                    yield return null;
                }
            }
        }
        private static List<int> GetSimpleTriangulation(Dictionary<int, List<int>> cached, int halfVerticesLength)
        {
            List<int> sideTriangles;
            if (!cached.ContainsKey(halfVerticesLength))
            {
                sideTriangles = new List<int>();
                SimpleTriangulation(sideTriangles, halfVerticesLength);
                cached[halfVerticesLength] = sideTriangles;
            }
            else
                sideTriangles = cached[halfVerticesLength];
            
            return sideTriangles;
        }
        private static void SimpleTriangulation(List<int> trianglesList, int halfVerticesLength)
        {
            for (int i = 0; i < halfVerticesLength - 1; i++)
            {
                int cornerA = i;
                int cornerB = i + halfVerticesLength;
                int cornerC = i + 1;
                int cornerD = i + halfVerticesLength + 1;

                trianglesList.AddRange(new int[] { cornerA, cornerB, cornerC, cornerC, cornerB, cornerD });
            }
        }
        private static Vector2[] GetSimpleUVNation(Dictionary<int, Vector2[]> cached, int verticesLength)
        {
            Vector2[] sideUVs;
            if (!cached.ContainsKey(verticesLength))
            {
                sideUVs = new Vector2[verticesLength];
                SimpleUVNation(sideUVs);
                cached[verticesLength] = sideUVs;
            }
            else
                sideUVs = cached[verticesLength];
            
            return sideUVs;
        }
        private static void SimpleUVNation(Vector2[] uvArray)
        {
            int halfArrayLength = uvArray.Length / 2;
            for (int i = 0; i < halfArrayLength; i++)
            {
                uvArray[i] = new Vector2(i, 1);
                uvArray[i + halfArrayLength] = new Vector2(i, 0);
            }
        }
        private static IEnumerable<Vector3> GetSide(bool hasSide, System.Func<JigsawPiece, int, IEnumerable<Vector2>> evaluation, JigsawPiece currentPiece, int edgeSmoothness, float halfDepth, Vector3[] fallback)
        {
            IEnumerable<Vector3> sideVertices;
            if (hasSide)
            {
                var twoDee = evaluation(currentPiece, edgeSmoothness);
                sideVertices = twoDee.Select(point => point.ToXZVector3(halfDepth)).Concat(twoDee.Select(point => point.ToXZVector3(-halfDepth)));
            }
            else
                sideVertices = fallback;

            return sideVertices;
        }
    }
}