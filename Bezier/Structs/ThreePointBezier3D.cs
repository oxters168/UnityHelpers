using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
    public struct ThreePointBezier3D
    {
        public Vector3 pointA { get { return GetPointA(); } set { SetPointA(value); } }
        public Vector3 pointB { get { return GetPointB(); } set { SetPointB(value); } }
        public Vector3 pointC { get { return GetPointC(); } set { SetPointC(value); } }

        private Vector3[] controlPoints;

        /// <summary>
        /// Evaluates one point on the bezier curve
        /// </summary>
        /// <param name="t">The percent travelled on the curve</param>
        /// <returns>A point on the bezier curve</returns>
        public Vector3 Evaluate(float t)
        {
            return controlPoints.Bezier(t);
        }
        /// <summary>
        /// Evaluates the bezier over many t values and returns the results as an enumerable
        /// </summary>
        /// <param name="smoothness">The amount of lines that make up the curve (enumerable count will be plus one)</param>
        /// <returns>An enumerable of points on the bezier curve</returns>
        public IEnumerable<Vector3> EvaluateCurve(int smoothness = 5)
        {
            var copiedPoints = controlPoints;
            var precision = (1f / smoothness);
            var tValues = ((float)0).Extend((currentValue) => currentValue += precision, smoothness);
            return tValues.Select((t) => copiedPoints.Bezier(t));
        }

        public Vector3 GetPointA()
        {
            return GetControlPoint(0);
        }
        public void SetPointA(Vector3 value)
        {
            SetControlPoint(0, value);
        }
        public Vector3 GetPointB()
        {
            return GetControlPoint(1);
        }
        public void SetPointB(Vector3 value)
        {
            SetControlPoint(1, value);
        }
        public Vector3 GetPointC()
        {
            return GetControlPoint(2);
        }
        public void SetPointC(Vector3 value)
        {
            SetControlPoint(2, value);
        }

        private Vector3 GetControlPoint(int index)
        {
            CheckCPInit();
            return controlPoints[index];
        }
        private void SetControlPoint(int index, Vector3 value)
        {
            CheckCPInit();
            controlPoints[index] = value;
        }
        private void CheckCPInit()
        {
            if (controlPoints == null)
                controlPoints = new Vector3[3];
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