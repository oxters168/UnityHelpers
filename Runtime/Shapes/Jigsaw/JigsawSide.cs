using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityHelpers.Shapes
{
    public class JigsawSide : Shape2D
    {
        public ThreePointBezier2D leftBiz;
        public ThreePointBezier2D middleLeftBiz;
        public ThreePointBezier2D middleRightBiz;
        public ThreePointBezier2D rightBiz;

        public override Vector2 Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float subT = (t % 0.25f) / 0.25f;
            if (t <= 0.25f)
                return leftBiz.Evaluate(subT);
            else if (t <= 0.5f)
                return middleLeftBiz.Evaluate(subT);
            else if (t <= 0.75f)
                return middleRightBiz.Evaluate(subT);
            else
                return rightBiz.Evaluate(subT);
        }
        public override IEnumerable<Vector2> Evaluate(int numPoints)
        {
            IEnumerable<Vector2> bizPoints = new List<Vector2>();
            bizPoints = bizPoints.Concat(leftBiz.Evaluate(numPoints));
            bizPoints = bizPoints.Concat(middleLeftBiz.Evaluate(numPoints));
            bizPoints = bizPoints.Concat(middleRightBiz.Evaluate(numPoints));
            bizPoints = bizPoints.Concat(rightBiz.Evaluate(numPoints));
            return bizPoints;
        }
        public static JigsawSide CreateGenitalia(float pieceWidth, float pieceHeight, bool protrude = true, float percentSideDip = 0.2f, float girthPercent = 0.3f, float growthPercent = 0.5f, float lengthPercent = 0.5f, float dipPlacement = 0.9f, float growthPlacement = 0.9f)
        {
            var jigSide = new JigsawSide();
            float halfWidth = pieceWidth / 2;
            float halfHeight = pieceHeight / 2;

            float maxDippage = halfHeight * 0.9f;
            float dipAmount = maxDippage * Mathf.Clamp01(percentSideDip); //The dip that happens before the penis

            float maxGirth = halfWidth * 0.9f;
            float girthAmount = maxGirth * Mathf.Clamp01(girthPercent); //How wide the penis is at the base
            float growthAmount = maxGirth * Mathf.Clamp01(growthPercent); //How wide the penis can grow on the way to the tip

            float maxLength = halfHeight * 0.9f;
            float lengthAmount = maxLength * Mathf.Clamp01(lengthPercent); //How long the penis is

            float dipPos = Mathf.Lerp(halfWidth, girthAmount, Mathf.Clamp01(dipPlacement));
            float growthPos = Mathf.Lerp(0, lengthAmount, Mathf.Clamp01(growthPlacement));

            dipAmount = dipAmount * (protrude ? 1 : -1);
            lengthAmount = lengthAmount * (protrude ? 1 : -1);
            growthPos = growthPos * (protrude ? 1 : -1);

            jigSide.leftBiz = new ThreePointBezier2D
            {
                pointA = new Vector2(-halfWidth, halfHeight),
                pointB = new Vector2(-dipPos, halfHeight - dipAmount),
                pointC = new Vector2(-girthAmount, halfHeight),
            };
            jigSide.rightBiz = new ThreePointBezier2D
            {
                pointA = new Vector2(girthAmount, halfHeight),
                pointB = new Vector2(dipPos, halfHeight - dipAmount),
                pointC = new Vector2(halfWidth, halfHeight),
            };

            jigSide.middleLeftBiz = new ThreePointBezier2D
            {
                pointA = new Vector2(-girthAmount, halfHeight),
                pointB = new Vector2(-growthAmount, halfHeight + growthPos),
                pointC = new Vector2(0, halfHeight + lengthAmount),
            };
            jigSide.middleRightBiz = new ThreePointBezier2D
            {
                pointA = new Vector2(0, halfHeight + lengthAmount),
                pointB = new Vector2(growthAmount, halfHeight + growthPos),
                pointC = new Vector2(girthAmount, halfHeight),
            };

            return jigSide;
        }
        public JigsawSide CreateSpouse(float pieceHeight)
        {
            JigsawSide spouse = new JigsawSide();
            spouse.leftBiz = new ThreePointBezier2D
            {
                pointA = leftBiz.pointA,
                pointB = new Vector2(leftBiz.pointB.x, pieceHeight - leftBiz.pointB.y),
                pointC = leftBiz.pointC,
            };
            spouse.rightBiz = new ThreePointBezier2D
            {
                pointA = rightBiz.pointA,
                pointB = new Vector2(rightBiz.pointB.x, pieceHeight - rightBiz.pointB.y),
                pointC = rightBiz.pointC,
            };

            spouse.middleLeftBiz = new ThreePointBezier2D
            {
                pointA = middleLeftBiz.pointA,
                pointB = new Vector2(middleLeftBiz.pointB.x, pieceHeight - middleLeftBiz.pointB.y),
                pointC = new Vector2(middleLeftBiz.pointC.x, pieceHeight - middleLeftBiz.pointC.y),
            };
            spouse.middleRightBiz = new ThreePointBezier2D
            {
                pointA = new Vector2(middleRightBiz.pointA.x, pieceHeight - middleRightBiz.pointA.y),
                pointB = new Vector2(middleRightBiz.pointB.x, pieceHeight - middleRightBiz.pointB.y),
                pointC = middleRightBiz.pointC,
            };

            return spouse;
        }
    }
}