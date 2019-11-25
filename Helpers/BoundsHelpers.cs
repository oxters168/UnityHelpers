using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
    public static class BoundsHelpers
    {
        /// <summary>
        /// Gets all the renderers or colliders in the transform and gets their total bounds.
        /// </summary>
        /// <param name="transform">The root transform of the object</param>
        /// <param name="worldSpace">An option to return the bounds' center to be relative or absolute</param>
        /// <param name="fromColliders">If set to true, will get the total bounds from colliders rather than renderers</param>
        /// <param name="includeDisabled">If set to true includes renderers or colliders that are on disabled gameobjects</param>
        /// <returns>A bounds that encapsulates the entire model</returns>
        public static Bounds GetTotalBounds(this Transform transform, bool worldSpace = true, bool fromColliders = false, bool includeDisabled = false)
        {
            Bounds totalBounds = new Bounds();

            List<Bounds> innerBounds = new List<Bounds>();
            if (fromColliders)
            {
                foreach (var collider in transform.GetComponentsInChildren<Collider>(true))
                    if (includeDisabled || collider.gameObject.activeSelf)
                        innerBounds.Add(collider.bounds);
            }
            else
            {
                foreach (var renderer in transform.GetComponentsInChildren<Renderer>(true))
                    if (includeDisabled || renderer.gameObject.activeSelf)
                        innerBounds.Add(renderer.bounds);
            }
            totalBounds = Combine(innerBounds.ToArray());
            if (!worldSpace)
                totalBounds.center = transform.InverseTransformPoint(totalBounds.center);

            return totalBounds;
        }
        /// <summary>
        /// Gets only the current transform's renderer's bounds.
        /// </summary>
        /// <param name="transform">The transform of the object</param>
        /// <param name="worldSpace">An option to return the bounds' center to be relative or absolute</param>
        /// <returns>A bounds that encapsulates only the given transform's model</returns>
        public static Bounds GetBounds(this Transform transform, bool worldSpace = true)
        {
            Bounds singleBounds = new Bounds();

            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
                singleBounds = renderer.bounds;
            if (!worldSpace)
                singleBounds.center = transform.InverseTransformPoint(singleBounds.center);

            return singleBounds;
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
        /// <summary>
        /// Finds the objects the given point is in. Checks the children of the given object as well.
        /// </summary>
        /// <param name="rootObject">The root of the object to check</param>
        /// <param name="point">The point in world space</param>
        /// <returns>A list of all objects where the point is contained</returns>
        public static List<Transform> HasPointInTotalBounds(this Transform rootObject, Vector3 point)
        {
            List<Transform> pointPierces = new List<Transform>();

            foreach (Transform currentTransform in rootObject.GetComponentsInChildren<Transform>())
            {
                if (currentTransform.GetBounds().Contains(point))
                    pointPierces.Add(currentTransform);
            }
            return pointPierces;
        }
        public static bool HasPointInBounds(this Transform currentTransform, Vector3 point)
        {
            bool hasPoint = false;
            if (currentTransform.GetBounds().Contains(point))
                hasPoint = true;
            return hasPoint;
        }
    }
}