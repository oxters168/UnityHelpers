using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
    public struct ThreePointBezier2D
    {
        public Vector2 pointA { get { return GetPointA(); } set { SetPointA(value); } }
        public Vector2 pointB { get { return GetPointB(); } set { SetPointB(value); } }
        public Vector2 pointC { get { return GetPointC(); } set { SetPointC(value); } }

        private Vector2[] controlPoints;

        /// <summary>
        /// Evaluates one point on the bezier curve
        /// </summary>
        /// <param name="t">The percent travelled on the curve</param>
        /// <returns>A point on the bezier curve</returns>
        public Vector2 Evaluate(float t)
        {
            return controlPoints.Bezier(t);
        }
        /// <summary>
        /// Evaluates the bezier over many t values and returns the results as an enumerable
        /// </summary>
        /// <param name="smoothness">The amount of lines that make up the curve (enumerable count will be plus one)</param>
        /// <returns>An enumerable of points on the bezier curve</returns>
        public IEnumerable<Vector2> EvaluateCurve(int smoothness = 5)
        {
            var copiedPoints = controlPoints;
            var precision = (1f / smoothness);
            var tValues = ((float)0).Extend((currentValue) => currentValue += precision, smoothness);
            return tValues.Select((t) => copiedPoints.Bezier(t));
        }

        public Vector2 GetPointA()
        {
            return GetControlPoint(0);
        }
        public void SetPointA(Vector2 value)
        {
            SetControlPoint(0, value);
        }
        public Vector2 GetPointB()
        {
            return GetControlPoint(1);
        }
        public void SetPointB(Vector2 value)
        {
            SetControlPoint(1, value);
        }
        public Vector2 GetPointC()
        {
            return GetControlPoint(2);
        }
        public void SetPointC(Vector2 value)
        {
            SetControlPoint(2, value);
        }

        private Vector2 GetControlPoint(int index)
        {
            CheckCPInit();
            return controlPoints[index];
        }
        private void SetControlPoint(int index, Vector2 value)
        {
            CheckCPInit();
            controlPoints[index] = value;
        }
        private void CheckCPInit()
        {
            if (controlPoints == null)
                controlPoints = new Vector2[3];
        }

        public void DrawGizmo(int smoothness = 5, bool displayCurves = true, bool displayControlPointConnections = false)
        {
            var bezierPoints = EvaluateCurve(smoothness);

            if (displayControlPointConnections)
            {
                Gizmos.color = Color.yellow;
                for (int i = 1; i < controlPoints.Length; i++)
                {
                    var from = controlPoints[i - 1];
                    var to = controlPoints[i];
                    Gizmos.DrawLine(from, to);
                }
            }
            if (displayCurves)
            {
                Gizmos.color = Color.green;
                for (int i = 1; i < bezierPoints.Count(); i++)
                {
                    var from = bezierPoints.ElementAt(i - 1);
                    var to = bezierPoints.ElementAt(i);
                    Gizmos.DrawLine(from, to);
                }
            }
        }
    }
}