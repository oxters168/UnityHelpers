using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityHelpers.Shapes;

namespace UnityHelpers.Tests
{
    public class JigsawExample : MonoBehaviour
    {
        public Material faceMaterial;
        public Material sideMaterial;
        public Material bottomMaterial;

        [Space(10)]
        public int columns = 10;
        public int rows = 10;
        public float width = 2;
        public float height = 2;
        public float depth = 0.02f;

        [Space(10)]
        public int bezierSmoothness = 5;

        [Space(10)]
        public int seed = 1337;
        public bool generate;
        [Range(0, 1)]
        public float percentDrawn;
        private IEnumerable<Vector3> totalVerts;

        void Start()
        {
            StartCoroutine(JigsawPuzzle.Generate(columns, rows, width, height, depth, bezierSmoothness, seed, transform, null, faceMaterial, sideMaterial, bottomMaterial, true));
            //JigsawPuzzle.Generate(columns, rows, width, height, depth, bezierSmoothness, transform, faceMaterial, sideMaterial, bottomMaterial, true);
        }

        void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            if (generate)
            {
                float pieceWidth = width / columns;
                float pieceHeight = height / rows;
                JigsawPiece jigBiz = JigsawPiece.GenerateRandom(pieceWidth, pieceHeight, seed);
                totalVerts = jigBiz.Evaluate(bezierSmoothness * 4).Select(point => point.ToXZVector3());
            }
            if (totalVerts != null)
            {
                int drawCount = Mathf.FloorToInt((totalVerts.Count() + 1) * percentDrawn);
                for (int i = 1; i < drawCount; i++)
                {
                    Gizmos.color = new Color(0, ((float)i) / (totalVerts.Count() - 1), 0);
                    var prevIndex = (i - 1) % totalVerts.Count();
                    var currentIndex = i % totalVerts.Count();
                    var start = totalVerts.ElementAt(prevIndex);
                    var end = totalVerts.ElementAt(currentIndex);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}