using System.Collections.Generic;
using UnityEngine;

public static class BoundsHelpers
{
    /// <summary>
    /// Gets all the renderers in the transform and gets their total bounds.
    /// </summary>
    /// <param name="transform">The root transform of the object</param>
    /// <returns>A bounds that encapsulates the entire model</returns>
    public static Bounds Bounds(this Transform transform)
    {
        Bounds totalBounds = new Bounds();

        List<Bounds> innerBounds = new List<Bounds>();
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>(true))
            innerBounds.Add(renderer.bounds);
        totalBounds = Combine(innerBounds.ToArray());
        totalBounds.center = transform.InverseTransformPoint(totalBounds.center);

        return totalBounds;
    }
    /// <summary>
    /// Combines any number of bounds
    /// </summary>
    /// <param name="bounds">The bounds to be combined</param>
    /// <returns>A bounds that encapsulates all the given bounds</returns>
    public static Bounds Combine(params Bounds[] bounds)
    {
        Bounds combined = new Bounds();
        if (bounds.Length > 0)
        {
            combined = bounds[0];
            for (int i = 1; i < bounds.Length; i++)
                combined.Encapsulate(bounds[i]);
        }
        return combined;
    }
}
