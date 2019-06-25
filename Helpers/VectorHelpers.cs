using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class VectorHelpers
{
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
    /// Vector3 add function
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
        return Average((IEnumerable<Vector3>)vectors);
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
}
