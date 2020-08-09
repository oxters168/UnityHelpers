using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityHelpers.Shapes
{
    public class JigsawPiece : Shape2D
    {
        public JigsawSide left;
        public JigsawSide right;
        public JigsawSide top;
        public JigsawSide bottom;

        public float pieceWidth;
        public float pieceHeight;

        public override IEnumerable<Vector2> Evaluate(int numPoints)
        {
            return Evaluate(numPoints, true, true, true, true);
        }
        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float subT = (t % 0.25f) / 0.25f;
            if (t <= 0.25f)
                return top.Evaluate(subT);
            else if (t <= 0.5f)
                return right.Evaluate(subT).Rotate(90);
            else if (t <= 0.75f)
                return bottom.Evaluate(subT).Rotate(180);
            else
                return left.Evaluate(subT).Rotate(270);
        }

        public IEnumerable<Vector2> EvaluateTop(int numPoints = 5)
        {
            IEnumerable<Vector2> topSidePoints = null;
            topSidePoints = top.Evaluate(numPoints);

            return topSidePoints;
        }
        public IEnumerable<Vector2> EvaluateRight(int numPoints = 5)
        {
            IEnumerable<Vector2> rightSidePoints = null;
            rightSidePoints = right.Evaluate(numPoints).Select((initial) => initial.Rotate(90));

            return rightSidePoints;
        }
        public IEnumerable<Vector2> EvaluateBottom(int numPoints = 5)
        {
            IEnumerable<Vector2> bottomSidePoints = null;
            bottomSidePoints = bottom.Evaluate(numPoints).Select((initial) => initial.Rotate(180));

            return bottomSidePoints;
        }
        public IEnumerable<Vector2> EvaluateLeft(int numPoints = 5)
        {
            IEnumerable<Vector2> leftSidePoints = null;
            leftSidePoints = left.Evaluate(numPoints).Select((initial) => initial.Rotate(270));

            return leftSidePoints;
        }

        public IEnumerable<Vector2> Evaluate(int numPoints = 20, bool keepTop = true, bool keepRight = true, bool keepBottom = true, bool keepLeft = true)
        {
            int spreadPoints = numPoints / 4;
            int leftoverPoints = numPoints % 4;

            float halfWidth = pieceWidth / 2;
            float halfHeight = pieceHeight / 2;

            IEnumerable<Vector2> topSidePoints = null;
            if (keepTop)
            {
                topSidePoints = EvaluateTop(spreadPoints + (leftoverPoints >= 1 ? 1 : 0));
                topSidePoints = topSidePoints.Take(topSidePoints.Count() - 1);
            }
            else
                topSidePoints = new Vector2[] { new Vector2(-halfWidth, halfHeight) };

            IEnumerable<Vector2> rightSidePoints = null;
            if (keepRight)
            {
                rightSidePoints = EvaluateRight(spreadPoints + (leftoverPoints >= 2 ? 1 : 0));
                rightSidePoints = rightSidePoints.Take(rightSidePoints.Count() - 1);
            }
            else
                rightSidePoints = new Vector2[] { new Vector2(halfWidth, halfHeight) };
            
            IEnumerable<Vector2> bottomSidePoints = null;
            if (keepBottom)
            {
                bottomSidePoints = EvaluateBottom(spreadPoints + (leftoverPoints >= 3 ? 1 : 0));
                bottomSidePoints = bottomSidePoints.Take(bottomSidePoints.Count() - 1);
            }
            else
                bottomSidePoints = new Vector2[] { new Vector2(halfWidth, -halfHeight) };

            IEnumerable<Vector2> leftSidePoints = null;
            if (keepLeft)
            {
                leftSidePoints = EvaluateLeft(spreadPoints);
                leftSidePoints = leftSidePoints.Take(leftSidePoints.Count() - 1);
            }
            else
                leftSidePoints = new Vector2[] { new Vector2(-halfWidth, -halfHeight) };

            return topSidePoints.Concat(rightSidePoints).Concat(bottomSidePoints).Concat(leftSidePoints).Reverse();
        }

        //Good Ranges dip[0.08, 0.5] girth[0.06, 0.3] growth[0.5, 1] length[0.3, 0.6] dipP[0.2, 1] growthP[0.88, 1]
        public static JigsawPiece GenerateRandom(float pieceWidth, float pieceHeight,
            float seed = 1337, float posX = 0, float posY = 0,
            float dipRangeStart = 0.08f, float dipRangeEnd = 0.5f,
            float girthRangeStart = 0.06f, float girthRangeEnd = 0.3f,
            float growthRangeStart = 0.5f, float growthRangeEnd = 1,
            float lenghRangeStart = 0.3f, float lengthRangeEnd = 0.6f,
            float dipPRangeStart = 0.2f, float dipPRangeEnd = 1,
            float growthPRangeStart = 0.88f, float growthPRangeEnd = 1
        )
        {
            JigsawPiece randomizedBizaz = new JigsawPiece();
            randomizedBizaz.pieceWidth = pieceWidth;
            randomizedBizaz.pieceHeight = pieceHeight;
            
            var pieceNoise = Mathf.PerlinNoise(posX + pieceWidth * seed * 1879, posY + pieceHeight * seed * 1033);
            var bizProtrude = Mathf.RoundToInt(pieceNoise * 10) % 2 == 0;
            var bizDip = (dipRangeEnd - dipRangeStart) * pieceNoise + dipRangeStart;
            var bizGirth = (girthRangeEnd - girthRangeStart) * pieceNoise + girthRangeStart;
            var bizGrowth = (growthPRangeEnd - growthRangeStart) * pieceNoise + growthRangeStart;
            var bizLength = (lengthRangeEnd - lenghRangeStart) * pieceNoise + lenghRangeStart;
            var bizDipP = (dipPRangeEnd - dipPRangeStart) * pieceNoise + dipPRangeStart;
            var bizGrowthP = (growthPRangeEnd - growthPRangeStart) * pieceNoise + growthPRangeStart;
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

            pieceNoise = Mathf.PerlinNoise(posX + pieceWidth * seed * 1607, posY + pieceHeight * seed * 1571);
            bizProtrude = Mathf.RoundToInt(pieceNoise * 10) % 2 == 0;
            bizDip = (dipRangeEnd - dipRangeStart) * pieceNoise + dipRangeStart;
            bizGirth = (girthRangeEnd - girthRangeStart) * pieceNoise + girthRangeStart;
            bizGrowth = (growthPRangeEnd - growthRangeStart) * pieceNoise + growthRangeStart;
            bizLength = (lengthRangeEnd - lenghRangeStart) * pieceNoise + lenghRangeStart;
            bizDipP = (dipPRangeEnd - dipPRangeStart) * pieceNoise + dipPRangeStart;
            bizGrowthP = (growthPRangeEnd - growthPRangeStart) * pieceNoise + growthPRangeStart;
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

            pieceNoise = Mathf.PerlinNoise(posX + pieceWidth * seed * 2113, posY + pieceHeight * seed * 461);
            bizProtrude = Mathf.RoundToInt(pieceNoise * 10) % 2 == 0;
            bizDip = (dipRangeEnd - dipRangeStart) * pieceNoise + dipRangeStart;
            bizGirth = (girthRangeEnd - girthRangeStart) * pieceNoise + girthRangeStart;
            bizGrowth = (growthPRangeEnd - growthRangeStart) * pieceNoise + growthRangeStart;
            bizLength = (lengthRangeEnd - lenghRangeStart) * pieceNoise + lenghRangeStart;
            bizDipP = (dipPRangeEnd - dipPRangeStart) * pieceNoise + dipPRangeStart;
            bizGrowthP = (growthPRangeEnd - growthPRangeStart) * pieceNoise + growthPRangeStart;
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

            pieceNoise = Mathf.PerlinNoise(posX + pieceWidth * seed * 569, posY + pieceHeight * seed * 3209);
            bizProtrude = Mathf.RoundToInt(pieceNoise * 10) % 2 == 0;
            bizDip = (dipRangeEnd - dipRangeStart) * pieceNoise + dipRangeStart;
            bizGirth = (girthRangeEnd - girthRangeStart) * pieceNoise + girthRangeStart;
            bizGrowth = (growthPRangeEnd - growthRangeStart) * pieceNoise + growthRangeStart;
            bizLength = (lengthRangeEnd - lenghRangeStart) * pieceNoise + lenghRangeStart;
            bizDipP = (dipPRangeEnd - dipPRangeStart) * pieceNoise + dipPRangeStart;
            bizGrowthP = (growthPRangeEnd - growthPRangeStart) * pieceNoise + growthPRangeStart;
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