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
    public static Vector3 Average(params Vector3[] vectors)
    {
        return Average((IEnumerable<Vector3>)vectors);
    }

    public static Vector3 Add(this Vector3 v1, Vector3 v2)
    {
        return v1 + v2;
    }

    public static Vector3 Average(this IEnumerable<Vector3> vectors)
    {
        return vectors.Aggregate(Add) / vectors.Count();
    }
}
