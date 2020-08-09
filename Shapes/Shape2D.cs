using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers.Shapes
{
    public abstract class Shape2D
    {
        public abstract Vector2 Evaluate(float t);
        public abstract IEnumerable<Vector2> Evaluate(int numPoints);
    }
}