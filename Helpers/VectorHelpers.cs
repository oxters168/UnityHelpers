using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    public static class VectorHelpers
    {
        private static Matrix4x4 OrthoX = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
        private static Matrix4x4 OrthoY = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));

        /// <summary>
        /// Checks if all values are equal to zero
        /// </summary>
        /// <param name="vec">The vector in question</param>
        /// <returns>True if all values are zero</returns>
        public static bool IsZero(this Vector2 vec)
        {
            return Mathf.Abs(vec.x) <= float.Epsilon && Mathf.Abs(vec.y) <= float.Epsilon;
        }
        /// <summary>
        /// Checks if all values are equal to zero
        /// </summary>
        /// <param name="vec">The vector in question</param>
        /// <returns>True if all values are zero</returns>
        public static bool IsZero(this Vector3 vec)
        {
            return Mathf.Abs(vec.x) <= float.Epsilon && Mathf.Abs(vec.y) <= float.Epsilon && Mathf.Abs(vec.z) <= float.Epsilon;
        }

        /// <summary>
        /// Get the axis of the given transform that is most aligned
        /// with the provided direction in world space
        /// </summary>
        /// <param name="orientedObject">The object in question</param>
        /// <param name="direction">The direction to compare to</param>
        /// <returns>An axis belonging to the object</returns>
        public static Vector3 GetAxisAlignedTo(this Vector3 direction, Transform orientedObject)
        {
            Vector3[] objectAxes = new Vector3[] { orientedObject.forward, orientedObject.right, orientedObject.up, -orientedObject.forward, -orientedObject.right, -orientedObject.up };
            return direction.GetDirectionMostAlignedTo(objectAxes);
        }
        /// <summary>
        /// Get the direction that is most aligned
        /// with the provided direction
        /// </summary>
        /// <param name="direction">The direction to compare to</param>
        /// <returns>A world axis</returns>
        public static Vector3 GetDirectionMostAlignedTo(this Vector3 direction, params Vector3[] directions)
        {
            Vector3 alignedAxis = Vector3.zero;

            if (directions.Length > 0)
            {
                alignedAxis = directions[0];
                float closestDot = Vector3.Dot(alignedAxis, direction);
                for (int i = 1; i < directions.Length; i++)
                {
                    var currentAxis = directions[i];
                    float otherDot = Vector3.Dot(currentAxis, direction);
                    if (otherDot > closestDot)
                    {
                        alignedAxis = currentAxis;
                        closestDot = otherDot;
                    }
                }
            }

            return alignedAxis;
        }

        /// <summary>
        /// Source: https://stackoverflow.com/questions/3684269/component-of-a-quaternion-rotation-around-an-axis
        /// Retrieves the vectors orthogonal to the given direction vector in 3D space
        /// </summary>
        /// <param name="vector">The direction vector</param>
        /// <param name="normal1">The first normal of the given vector</param>
        /// <param name="normal2">The second normal of the given vector</param>
        public static void FindOrthoNormals(this Vector3 vector, out Vector3 normal1, out Vector3 normal2)
        {
            Vector3 w = OrthoX.MultiplyPoint(vector);
            float dot = Vector3.Dot(vector, w);
            if (Mathf.Abs(dot) > 0.6)
            {
                w = OrthoY.MultiplyPoint(vector);
            }
            w.Normalize();

            normal1 = Vector3.Cross(vector, w);
            normal1.Normalize();
            normal2 = Vector3.Cross(vector, normal1);
            normal2.Normalize();
        }

        /// <summary>
        /// Compares two vectors value for value
        /// </summary>
        /// <param name="first">The first vector</param>
        /// <param name="second">The second vector</param>
        /// <returns>True if all values are equal, false otherwise</returns>
        public static bool EqualTo(this Vector2Int first, Vector2Int second)
        {
            return first.x == second.x && first.y == second.y;
        }
        /// <summary>
        /// Compares two vectors value for value
        /// </summary>
        /// <param name="first">The first vector</param>
        /// <param name="second">The second vector</param>
        /// <returns>True if all values are equal, false otherwise</returns>
        public static bool EqualTo(this Vector3Int first, Vector3Int second)
        {
            return first.x == second.x && first.y == second.y && first.z == second.z;
        }
        /// <summary>
        /// Compares two vectors value for value
        /// </summary>
        /// <param name="first">The first vector</param>
        /// <param name="second">The second vector</param>
        /// <param name="tolerance">The maximum amount of difference any given value can be (inclusive)</param>
        /// <returns>True if all values are within the threshold, false otherwise</returns>
        public static bool EqualTo(this Vector2 first, Vector2 second, float tolerance = float.Epsilon)
        {
            return Mathf.Abs(first.x - second.x) <= tolerance && Mathf.Abs(first.y - second.y) <= tolerance;
        }
        /// <summary>
        /// Compares two vectors value for value
        /// </summary>
        /// <param name="first">The first vector</param>
        /// <param name="second">The second vector</param>
        /// <param name="tolerance">The maximum amount of difference any given value can be (inclusive)</param>
        /// <returns>True if all values are within the threshold, false otherwise</returns>
        public static bool EqualTo(this Vector3 first, Vector3 second, float tolerance = float.Epsilon)
        {
            return Mathf.Abs(first.x - second.x) <= tolerance && Mathf.Abs(first.y - second.y) <= tolerance && Mathf.Abs(first.z - second.z) <= tolerance;
        }
        /// <summary>
        /// /// Generates a vector3 whose x and z values equals the given vector2's x and y values.
        /// </summary>
        /// <param name="point">The original point</param>
        /// <param name="yValue">The resulting vector3's y value</param>
        /// <returns>A vector3 value</returns>
        public static Vector3 ToXZVector3(this Vector2 point, float yValue = 0)
        {
            return new Vector3(point.x, yValue, point.y);
        }

        /// <summary>
        /// Rotates the given vector clockwise on an orthogonal axis.
        /// 
        /// By: XenoRo
        /// Source: https://answers.unity.com/questions/661383/whats-the-most-efficient-way-to-rotate-a-vector2-o.html
        /// </summary>
        /// <param name="vector">The original vector</param>
        /// <param name="degrees">The angle amount to rotate</param>
        /// <param name="pivot">A pivot to rotate about</param>
        /// <returns>The rotated vector</returns>
        public static Vector2 Rotate(this Vector2 vector, float degrees, Vector2 pivot = default(Vector2))
        {
            vector -= pivot;
            vector = Quaternion.Euler(0, 0, -degrees) * vector;
            vector += pivot;
            return vector; 
        }
 
        /// <summary>
        /// Calculates a point on the bezier curve based on the given control points and a percent value
        /// </summary>
        /// <param name="controlPoints">The points that shape the bezier curve</param>
        /// <param name="t">A value between 0 and 1 representing the percent travelled along the bezier curve</param>
        /// <returns>A point on the bezier curve</returns>
        public static Vector3 Bezier(this IEnumerable<Vector3> controlPoints, float t)
        {
            IEnumerable<Vector3> decayingPoints = controlPoints;
            while (decayingPoints.Count() > 1)
                decayingPoints = decayingPoints.SelectEveryPair((first, second) =>
                {
                    var difference = second - first;
                    var direction = difference.normalized;
                    var distance = difference.magnitude;
                    return first + direction * distance * t;
                });
                
            return decayingPoints.First();
        }
        /// <summary>
        /// Calculates a point on the bezier curve based on the given control points and a percent value
        /// </summary>
        /// <param name="controlPoints">The points that shape the bezier curve</param>
        /// <param name="t">A value between 0 and 1 representing the percent travelled along the bezier curve</param>
        /// <returns>A point on the bezier curve</returns>
        public static Vector2 Bezier(this IEnumerable<Vector2> controlPoints, float t)
        {
            return controlPoints.Select(point => new Vector3(point.x, point.y)).Bezier(t).xy();
        }
        /// <summary>
        /// Transforms a point from a transform's local space to another transform's local space directly
        /// </summary>
        /// <param name="transform">The transform whose space the point is originally represented in</param>
        /// <param name="otherTransform">The transform whose space you'd like the point to be represented in</param>
        /// <param name="point">The point in terms of the original transform's space</param>
        /// <returns>The transformed point</returns>
        public static Vector3 TransformPointToAnotherSpace(this Transform transform, Transform otherTransform, Vector3 point)
        {
            var localToLocalMatrix = otherTransform.worldToLocalMatrix * transform.localToWorldMatrix;
            return localToLocalMatrix.MultiplyPoint(point);
        }
        /// <summary>
        /// Transforms a point from a transform's local space to another transform's local space directly
        /// </summary>
        /// <param name="transform">The transform whose space the point is originally represented in</param>
        /// <param name="otherTransform">The transform whose space you'd like the point to be represented in</param>
        /// <param name="point">The point in terms of the original transform's space</param>
        /// <returns>The transformed direction</returns>
        public static Vector3 TransformDirectionToAnotherSpace(this Transform transform, Transform otherTransform, Vector3 direction)
        {
            var localToLocalMatrix = otherTransform.worldToLocalMatrix * transform.localToWorldMatrix;
            return localToLocalMatrix.MultiplyVector(direction);
        }

        /// <summary>
        /// Applies absolute value to all values of the given vector3
        /// </summary>
        /// <param name="original">The original vector3</param>
        /// <returns>Absolute valued vector3</returns>
        public static Vector3 Abs(this Vector3 original)
        {
            return new Vector3(Mathf.Abs(original.x), Mathf.Abs(original.y), Mathf.Abs(original.z));
        }
        /// <summary>
        /// Applies absolute value to all values of the given vector2
        /// </summary>
        /// <param name="original">The original vector2</param>
        /// <returns>Absolute valued vector2</returns>
        public static Vector2 Abs(this Vector2 original)
        {
            return new Vector2(Mathf.Abs(original.x), Mathf.Abs(original.y));
        }

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
        /// Calculates the shortest signed angle to reach a direction from another direction
        /// </summary>
        /// <param name="from">The start direction</param>
        /// <param name="to">The end direction</param>
        /// <returns>The shortest signed angle between the directions</returns>
        public static float GetShortestSignedAngle(this Vector2 from, Vector2 to)
        {
            float requestedAngle = Vector2.SignedAngle(to, Vector2.up);
            float currentAngle = Vector2.SignedAngle(from, Vector2.up);
            Quaternion currentUpOrientation = Quaternion.AngleAxis(currentAngle, Vector3.up);
            Quaternion requestedUpOrientation = Quaternion.AngleAxis(requestedAngle, Vector3.up);
            Quaternion orientationDiff = requestedUpOrientation * Quaternion.Inverse(currentUpOrientation);
            orientationDiff = orientationDiff.Shorten();
            float angleDiff;
            Vector3 axis;
            orientationDiff.ToAngleAxis(out angleDiff, out axis);
            return angleDiff * Mathf.Sign(Vector3.Dot(axis, Vector3.up));
        }
        /// <summary>
        /// Calculates the shortest signed angle to reach a direction from another direction where both directions lie on the same plane
        /// </summary>
        /// <param name="from">The start direction</param>
        /// <param name="to">The end direction</param>
        /// <param name="planeNormal">The normal of the plane the two directions lie on</param>
        /// <returns>The shortest signed angle between the directions</returns>
        public static float GetShortestSignedAngle(this Vector3 from, Vector3 to, Vector3 planeNormal)
        {
            Quaternion currentOrientation = Quaternion.LookRotation(from, planeNormal);
            Quaternion requestedOrientation = Quaternion.LookRotation(from, planeNormal);
            Quaternion orientationDiff = requestedOrientation * Quaternion.Inverse(currentOrientation);
            orientationDiff = orientationDiff.Shorten();
            float angleDiff;
            Vector3 axis;
            orientationDiff.ToAngleAxis(out angleDiff, out axis);
            return angleDiff * Mathf.Sign(Vector3.Dot(axis, planeNormal));
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