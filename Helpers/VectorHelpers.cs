using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    public static class VectorHelpers
    {
        /// <summary>
        /// Calculates the percent a vector's direction is close to another vector's direction (1 for same, -1 for opposite, and 0 for perpendicular (so basically Vector3.dot but more correct in-betweens)).
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="otherVector">The second vector</param>
        /// <returns>A value between -1 and 1 representing the angle between the two vector directions.</returns>
        public static float PercentDirection(this Vector3 vector, Vector3 otherVector)
        {
            return -(Vector3.Angle(vector.normalized, otherVector.normalized) / 90 - 1);
        }

        /// <summary>
        /// Calculates the signed angle between fromDirection and the direction between the two points on the given axis.
        /// </summary>
        /// <param name="point">The start point.</param>
        /// <param name="otherPoint">The end point.</param>
        /// <param name="fromDirection">The direction to compare to.</param>
        /// <param name="axis">The axis to measure on.</param>
        /// <returns>The angle between the direction generated from the points and the given direction.</returns>
        public static float SignedAngle(this Vector3 point, Vector3 otherPoint, Vector3 fromDirection, Vector3 axis)
        {
            Vector3 obstacleOffset = otherPoint - point;
            return Vector3.SignedAngle(fromDirection, obstacleOffset.normalized, axis);
        }

        /// <summary>
        /// Gets just the x and y values of the vector
        /// </summary>
        /// <param name="vector">Original vector</param>
        /// <returns>Vector2 with requested values</returns>
        public static Vector2 xy(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }
        /// <summary>
        /// Gets just the x and z values of the vector
        /// </summary>
        /// <param name="vector">Original vector</param>
        /// <returns>Vector2 with requested values</returns>
        public static Vector2 xz(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }
        /// <summary>
        /// Gets just the y and z values of the vector
        /// </summary>
        /// <param name="vector">Original vector</param>
        /// <returns>Vector2 with requested values</returns>
        public static Vector2 yz(this Vector3 vector)
        {
            return new Vector2(vector.y, vector.z);
        }
        /// <summary>
        /// Flattens the given vector to a plane
        /// </summary>
        /// <param name="vector">Original vector</param>
        /// <param name="planeNormal">Normal of plane</param>
        /// <returns>Flattened vector</returns>
        public static Vector3 Planar(this Vector3 vector, Vector3 planeNormal)
        {
            return Quaternion.AngleAxis(90 - Vector3.Angle(vector, Vector3.up), Vector3.Cross(planeNormal, vector)) * vector;
        }
        /// <summary>
        /// Shifts a point to a surface
        /// </summary>
        /// <param name="point">The point to be shifted</param>
        /// <param name="pointAlreadyOnSurface">A point already on the surface</param>
        /// <param name="surfaceNormal">The surface's normal</param>
        /// <returns>The original point shifted to the surface</returns>
        public static Vector3 ProjectPointToSurface(this Vector3 point, Vector3 pointAlreadyOnSurface, Vector3 surfaceNormal)
        {
            Vector3 offset = Vector3.Project(point - pointAlreadyOnSurface, surfaceNormal);

            return point - offset;
        }
        /// <summary>
        /// Multiplies two Vector3s value for value (x*x, y*y, z*z)
        /// </summary>
        /// <param name="first">The first vector3</param>
        /// <param name="second">The second vector3</param>
        /// <returns>The product of the two vector3s</returns>
        public static Vector3 Multiply(this Vector3 first, Vector3 second)
        {
            return new Vector3(first.x * second.x, first.y * second.y, first.z * second.z);
        }
        /// <summary>
        /// Multiplies two Vector2s value for value (x*x, y*y)
        /// </summary>
        /// <param name="first">The first vector2</param>
        /// <param name="second">The second vector2</param>
        /// <returns>The product of the two vector2s</returns>
        public static Vector2 Multiply(this Vector2 first, Vector2 second)
        {
            return new Vector2(first.x * second.x, first.y * second.y);
        }
        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>The sum of the two vectors</returns>
        public static Vector3 Add(this Vector3 v1, Vector3 v2)
        {
            return v1 + v2;
        }
        /// <summary>
        /// Gets the average of a list vector3s
        /// </summary>
        /// <param name="vectors">To be averaged</param>
        /// <returns>The average vector</returns>
        public static Vector3 Average(params Vector3[] vectors)
        {
            return vectors.Average();
        }
        /// <summary>
        /// Gets the average of a list vector3s
        /// </summary>
        /// <param name="vectors">To be averaged</param>
        /// <returns>The average vector</returns>
        public static Vector3 Average(this IEnumerable<Vector3> vectors)
        {
            return vectors.Aggregate(Add) / vectors.Count();
        }
        /// <summary>
        /// Turns all NaN values to 0
        /// </summary>
        /// <param name="vector3">The Vector3 in question</param>
        /// <returns>The fixed Vector3</returns>
        public static Vector3 FixNaN(this Vector3 vector3)
        {
            if (float.IsNaN(vector3.x))
                vector3.x = 0;
            if (float.IsNaN(vector3.y))
                vector3.y = 0;
            if (float.IsNaN(vector3.z))
                vector3.z = 0;

            return vector3;
        }

        /// <summary>
        /// Adds two vectors
        /// </summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>The sum of the two vectors</returns>
        public static Vector2 Add(this Vector2 v1, Vector2 v2)
        {
            return v1 + v2;
        }
        /// <summary>
        /// Gets the average of a list of vector2s
        /// </summary>
        /// <param name="vectors">To be averaged</param>
        /// <returns>The average vector</returns>
        public static Vector2 Average(params Vector2[] vectors)
        {
            return vectors.Average();
        }
        /// <summary>
        /// Gets the average of a list of vector2s
        /// </summary>
        /// <param name="vectors">To be averaged</param>
        /// <returns>The average vector</returns>
        public static Vector2 Average(this IEnumerable<Vector2> vectors)
        {
            return vectors.Aggregate(Add) / vectors.Count();
        }

        /// <summary>
        /// Clamps all values of the vector to min and max.
        /// </summary>
        /// <param name="vector2">The original Vector2</param>
        /// <param name="min">The clamp mininmum</param>
        /// <param name="max">The clamp maximum</param>
        /// <returns>A clamped Vector2</returns>
        public static Vector2 Clamp(this Vector2 vector2, float min, float max)
        {
            return new Vector2(Mathf.Clamp(vector2.x, min, max), Mathf.Clamp(vector2.y, min, max));
        }
        /// <summary>
        /// Clamps all values of the vector to min and max.
        /// </summary>
        /// <param name="vector3">The original Vector3</param>
        /// <param name="min">The clamp mininmum</param>
        /// <param name="max">The clamp maximum</param>
        /// <returns>A clamped Vector3</returns>
        public static Vector3 Clamp(this Vector3 vector3, float min, float max)
        {
            return new Vector3(Mathf.Clamp(vector3.x, min, max), Mathf.Clamp(vector3.y, min, max), Mathf.Clamp(vector3.z, min, max));
        }

        /// <summary>
        /// Turns a square input vector2 to circle input.
        /// The range of x and y should be from -1 to 1.
        /// Source: https://amorten.com/blog/2017/mapping-square-input-to-circle-in-unity/
        /// </summary>
        /// <param name="vector2">The original Vector2</param>
        /// <returns>An adjusted Vector2</returns>
        public static Vector2 ToCircle(this Vector2 vector2)
        {
            return new Vector2(vector2.x * Mathf.Sqrt(1 - vector2.y * vector2.y * 0.5f), vector2.y * Mathf.Sqrt(1 - vector2.x * vector2.x * 0.5f));
        }
        /// <summary>
        /// Turns a circle input vector2 to square input.
        /// The range of x and y should be from -1 to 1.
        /// Source: https://forum.unity.com/threads/making-a-square-vector2-fit-a-circle-vector2.422352/
        /// </summary>
        /// <param name="vector2">The original Vector2</param>
        /// <returns>An adjusted Vector2</returns>
        public static Vector2 ToSquare(this Vector2 vector2)
        {
            const float COS_45 = 0.70710678f;

            if (vector2.sqrMagnitude < float.Epsilon) // Or < EPSILON, Or < inner circle threshold. Your choice.
                return Vector2.zero;

            Vector2 normal = vector2.normalized;
            float x, y;

            if (normal.x != 0 && normal.y >= -COS_45 && normal.y <= COS_45)
                x = normal.x >= 0 ? vector2.x / normal.x : -vector2.x / normal.x;
            else
                x = vector2.x / Mathf.Abs(normal.y);

            if (normal.y != 0 && normal.x >= -COS_45 && normal.x <= COS_45)
                y = normal.y >= 0 ? vector2.y / normal.y : -vector2.y / normal.y;
            else
                y = vector2.y / Mathf.Abs(normal.x);

            return new Vector2(x, y);
        }

        /// <summary>
        /// Sets the x, y, and z values' decimal places
        /// </summary>
        /// <param name="vector3">The original Vector3</param>
        /// <param name="places">Number of decimal places to keep</param>
        /// <returns>The vector with it's values' decimal places adjusted</returns>
        public static Vector3 SetDecimalPlaces(this Vector3 vector3, uint places)
        {
            return new Vector3(MathHelpers.SetDecimalPlaces(vector3.x, places), MathHelpers.SetDecimalPlaces(vector3.y, places), MathHelpers.SetDecimalPlaces(vector3.z, places));
        }
        /// <summary>
        /// Sets the x and y values' decimal places
        /// </summary>
        /// <param name="vector2">The original Vector2</param>
        /// <param name="places">Number of decimal places to keep</param>
        /// <returns>The vector with it's values' decimal places adjusted</returns>
        public static Vector2 SetDecimalPlaces(this Vector2 vector2, uint places)
        {
            return new Vector2(MathHelpers.SetDecimalPlaces(vector2.x, places), MathHelpers.SetDecimalPlaces(vector2.y, places));
        }
    }
}