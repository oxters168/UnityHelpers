using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers.Shapes
{
    public abstract class Shape3D
    {
        public abstract Vector3 Evaluate(float t);
        public abstract IEnumerable<Vector3> Evaluate(int numPoints);
    }
}