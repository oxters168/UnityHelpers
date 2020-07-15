using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public struct JigsawPiece
    {
        public JigsawSide left;
        public JigsawSide right;
        public JigsawSide top;
        public JigsawSide bottom;

        public IEnumerable<Vector2> EvaluateTop(int bezierSmoothness = 5)
        {
            IEnumerable<Vector2> topSidePoints = null;
            topSidePoints = top.EvaluateAll(bezierSmoothness);

            return topSidePoints;
        }
        public IEnumerable<Vector2> EvaluateRight(int bezierSmoothness = 5)
        {
            IEnumerable<Vector2> rightSidePoints = null;
            rightSidePoints = right.EvaluateAll(bezierSmoothness).Select((initial) => initial.Rotate(90));

            return rightSidePoints;
        }
        public IEnumerable<Vector2> EvaluateBottom(int bezierSmoothness = 5)
        {
            IEnumerable<Vector2> bottomSidePoints = null;
            bottomSidePoints = bottom.EvaluateAll(bezierSmoothness).Select((initial) => initial.Rotate(180));

            return bottomSidePoints;
        }
        public IEnumerable<Vector2> EvaluateLeft(int bezierSmoothness = 5)
        {
            IEnumerable<Vector2> leftSidePoints = null;
            leftSidePoints = left.EvaluateAll(bezierSmoothness).Select((initial) => initial.Rotate(270));

            return leftSidePoints;
        }

        public IEnumerable<Vector2> EvaluateAll(float pieceWidth, float pieceHeight, int bezierSmoothness = 5, bool keepTop = true, bool keepRight = true, bool keepBottom = true, bool keepLeft = true)
        {
            float halfWidth = pieceWidth / 2;
            float halfHeight = pieceHeight / 2;

            IEnumerable<Vector2> topSidePoints = null;
            if (keepTop)
            {
                topSidePoints = EvaluateTop(bezierSmoothness);
                topSidePoints = topSidePoints.Take(topSidePoints.Count() - 1);
            }
            else
                topSidePoints = new Vector2[] { new Vector2(-halfWidth, halfHeight) };

            IEnumerable<Vector2> rightSidePoints = null;
            if (keepRight)
            {
                rightSidePoints = EvaluateRight(bezierSmoothness);
                rightSidePoints = rightSidePoints.Take(rightSidePoints.Count() - 1);
            }
            else
                rightSidePoints = new Vector2[] { new Vector2(halfWidth, halfHeight) };
            
            IEnumerable<Vector2> bottomSidePoints = null;
            if (keepBottom)
            {
                bottomSidePoints = EvaluateBottom(bezierSmoothness);
                bottomSidePoints = bottomSidePoints.Take(bottomSidePoints.Count() - 1);
            }
            else
                bottomSidePoints = new Vector2[] { new Vector2(halfWidth, -halfHeight) };

            IEnumerable<Vector2> leftSidePoints = null;
            if (keepLeft)
            {
                leftSidePoints = EvaluateLeft(bezierSmoothness);
                leftSidePoints = leftSidePoints.Take(leftSidePoints.Count() - 1);
            }
            else
                leftSidePoints = new Vector2[] { new Vector2(-halfWidth, -halfHeight) };

            return topSidePoints.Concat(rightSidePoints).Concat(bottomSidePoints).Concat(leftSidePoints).Reverse();
        }

        //Good Ranges dip[0.08, 0.5] girth[0.06, 0.3] growth[0.5, 1] length[0.3, 0.6] dipP[0.2, 1] growthP[0.88, 1]
        public static JigsawPiece GenerateRandom(float pieceWidth, float pieceHeight,
            float dipRangeStart = 0.08f, float dipRangeEnd = 0.5f,
            float girthRangeStart = 0.06f, float girthRangeEnd = 0.3f,
            float growthRangeStart = 0.5f, float growthRangeEnd = 1,
            float lenghRangeStart = 0.3f, float lengthRangeEnd = 0.6f,
            float dipPRangeStart = 0.2f, float dipPRangeEnd = 1,
            float growthPRangeStart = 0.88f, float growthPRangeEnd = 1
        )
        {
            JigsawPiece randomizedBizaz = new JigsawPiece();

            var bizProtrude = Random.Range(0, 2) != 0;
            var bizDip = Random.Range(dipRangeStart, dipRangeEnd);
            var bizGirth = Random.Range(girthRangeStart, girthRangeEnd);
            var bizGrowth = Random.Range(growthPRangeStart, growthPRangeEnd);
            var bizLength = Random.Range(lenghRangeStart, lengthRangeEnd);
            var bizDipP = Random.Range(dipPRangeStart, dipPRangeEnd);
            var bizGrowthP = Random.Range(growthPRangeStart, growthPRangeEnd);
            randomizedBizaz.left = JigsawSide.CreateGenitalia
            (
                pieceHeight, pieceWidth,
                bizProtrude,
                bizDip,
                bizGirth,
                bizGrowth,
                bizLength,
                bizDipP,
                bizGrowthP
            );

            bizProtrude = Random.Range(0, 2) != 0;
            bizDip = Random.Range(dipRangeStart, dipRangeEnd);
            bizGirth = Random.Range(girthRangeStart, girthRangeEnd);
            bizGrowth = Random.Range(growthPRangeStart, growthPRangeEnd);
            bizLength = Random.Range(lenghRangeStart, lengthRangeEnd);
            bizDipP = Random.Range(dipPRangeStart, dipPRangeEnd);
            bizGrowthP = Random.Range(growthPRangeStart, growthPRangeEnd);
            randomizedBizaz.right = JigsawSide.CreateGenitalia
            (
                pieceHeight, pieceWidth,
                bizProtrude,
                bizDip,
                bizGirth,
                bizGrowth,
                bizLength,
                bizDipP,
                bizGrowthP
            );

            bizProtrude = Random.Range(0, 2) != 0;
            bizDip = Random.Range(dipRangeStart, dipRangeEnd);
            bizGirth = Random.Range(girthRangeStart, girthRangeEnd);
            bizGrowth = Random.Range(growthPRangeStart, growthPRangeEnd);
            bizLength = Random.Range(lenghRangeStart, lengthRangeEnd);
            bizDipP = Random.Range(dipPRangeStart, dipPRangeEnd);
            bizGrowthP = Random.Range(growthPRangeStart, growthPRangeEnd);
            randomizedBizaz.top = JigsawSide.CreateGenitalia
            (
                pieceWidth, pieceHeight,
                bizProtrude,
                bizDip,
                bizGirth,
                bizGrowth,
                bizLength,
                bizDipP,
                bizGrowthP
            );

            bizProtrude = Random.Range(0, 2) != 0;
            bizDip = Random.Range(dipRangeStart, dipRangeEnd);
            bizGirth = Random.Range(girthRangeStart, girthRangeEnd);
            bizGrowth = Random.Range(growthPRangeStart, growthPRangeEnd);
            bizLength = Random.Range(lenghRangeStart, lengthRangeEnd);
            bizDipP = Random.Range(dipPRangeStart, dipPRangeEnd);
            bizGrowthP = Random.Range(growthPRangeStart, growthPRangeEnd);
            randomizedBizaz.bottom = JigsawSide.CreateGenitalia
            (
                pieceWidth, pieceHeight,
                bizProtrude,
                bizDip,
                bizGirth,
                bizGrowth,
                bizLength,
                bizDipP,
                bizGrowthP
            );

            return randomizedBizaz;
        }
    }
}