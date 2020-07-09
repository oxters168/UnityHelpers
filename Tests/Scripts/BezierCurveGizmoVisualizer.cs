using UnityEngine;
using System.Linq;

namespace UnityHelpers
{
    public class BezierCurveGizmoVisualizer : MonoBehaviour
    {
        public Transform[] bezierControlPoints;
        [Tooltip("A smaller value will make the curve smoother but will also run slower")]
        public float precision = 0.05f;

        void OnDrawGizmos()
        {
            if (bezierControlPoints != null && !bezierControlPoints.Any((bezierControlPoint) => bezierControlPoint == null))
            {
                var controlPoints = bezierControlPoints.Select((bezierControlPoint) => bezierControlPoint.position);
                var tValues = ((float)0).Extend((currentValue) => currentValue += precision, precision != 0 ? ((int)(1 / precision) + 1) : 0);
                var bezierPoints = tValues.Select((t) => controlPoints.Bezier(t));
                Gizmos.color = Color.yellow;
                for (int i = 1; i < controlPoints.Count(); i++)
                {
                    var currentPoint = controlPoints.ElementAt(i - 1);
                    var nextPoint = controlPoints.ElementAt(i);
                    Gizmos.DrawLine(currentPoint, nextPoint);
                }
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