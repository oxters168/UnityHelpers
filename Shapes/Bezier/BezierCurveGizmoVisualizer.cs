using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public class BezierCurveGizmoVisualizer : MonoBehaviour
    {
        public Transform[] bezierControlPoints;
        [Tooltip("Number of points to be drawn in the curve")]
        public int numPoints = 10;

        [Space(10)]
        public bool displayCurves = true;
        public bool displayControlPointConnections = true;

        void OnDrawGizmos()
        {
            if (bezierControlPoints != null && !bezierControlPoints.Any((bezierControlPoint) => bezierControlPoint == null))
            {
                var controlPoints = bezierControlPoints.Select((bezierControlPoint) => bezierControlPoint.position);
                var precision = (1f / (numPoints - 1));
                var tValues = ((float)0).Extend((currentValue) => currentValue += precision, numPoints - 1);
                var bezierPoints = tValues.Select((t) => controlPoints.Bezier(t));

                if (displayControlPointConnections)
                {
                    Gizmos.color = Color.yellow;
                    for (int i = 1; i < controlPoints.Count(); i++)
                    {
                        var currentPoint = controlPoints.ElementAt(i - 1);
                        var nextPoint = controlPoints.ElementAt(i);
                        Gizmos.DrawLine(currentPoint, nextPoint);
                    }
                }
                if (displayCurves)
                {
                    Gizmos.color = Color.green;
                    for (int i = 1; i < bezierPoints.Count(); i++)
                    {
                        var currentPoint = bezierPoints.ElementAt(i - 1);
                        var nextPoint = bezierPoints.ElementAt(i);
                        Gizmos.DrawLine(currentPoint, nextPoint);
                    }
                }
            }
        }
    }
}