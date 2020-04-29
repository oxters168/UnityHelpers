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
        [System.Obsolete("Please use the new GetTotalBounds functions which properly get local/world space bounds")]
        public static Bounds GetTotalBounds(this Transform transform, bool worldSpace = true, bool fromColliders = false, bool includeDisabled = false)
        {
            return GetTotalBounds(transform, ~0, worldSpace, fromColliders, includeDisabled);
        }
        /// <summary>
        /// Gets all the renderers or colliders in the transform and gets their total bounds.
        /// </summary>
        /// <param name="transform">The root transform of the object</param>
        /// <param name="layer">The layers to include in bounds calculation</param>
        /// <param name="worldSpace">An option to return the bounds' center to be relative or absolute</param>
        /// <param name="fromColliders">If set to true, will get the total bounds from colliders rather than renderers</param>
        /// <param name="includeDisabled">If set to true includes renderers or colliders that are on disabled gameobjects</param>
        /// <returns>A bounds that encapsulates the entire model</returns>
        [System.Obsolete("Please use the new GetTotalBounds functions which properly get local/world space bounds")]
        public static Bounds GetTotalBounds(this Transform transform, LayerMask layer, bool worldSpace = true, bool fromColliders = false, bool includeDisabled = false)
        {
            Bounds totalBounds = new Bounds();

            List<Bounds> innerBounds = new List<Bounds>();
            if (fromColliders)
            {
                foreach (var collider in transform.GetComponentsInChildren<Collider>(true))
                    if (((1 << collider.gameObject.layer) & layer.value) != 0 && (includeDisabled || collider.gameObject.activeSelf))
                        innerBounds.Add(collider.bounds);
            }
            else
            {
                foreach (var renderer in transform.GetComponentsInChildren<Renderer>(true))
                    if (((1 << renderer.gameObject.layer) & layer.value) != 0 && (includeDisabled || renderer.gameObject.activeSelf))
                        innerBounds.Add(renderer.bounds);
            }

            if (innerBounds.Count > 0)
                totalBounds = Combine(innerBounds.ToArray());
            else
                totalBounds = new Bounds(transform.position, Vector3.zero);

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
        [System.Obsolete("Please use the new GetBounds functions which properly gets local/world space bounds")]
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
        /// Gets the total bounds of an object including all it's children
        /// </summary>
        /// <param name="root">The root transform of the object</param>
        /// <param name="space">An option to return the bounds based on local or world space (local space would be relative to the root transform)</param>
        /// <param name="includeDisabled">Includes disabled gameobjects if set to true</param>
        /// <returns>A bounds that encapsulates the entire model</returns>
        public static Bounds GetTotalBounds(this Transform root, Space space, bool includeDisabled = false)
        {
            return root.GetTotalBounds(space, ~0, includeDisabled);
        }
        /// <summary>
        /// Gets the total bounds of an object including all it's children
        /// </summary>
        /// <param name="root">The root transform of the object</param>
        /// <param name="space">An option to return the bounds based on local or world space (local space would be relative to the root transform)</param>
        /// <param name="layers">The layers to include in bounds calculation</param>
        /// <param name="includeDisabled">Includes disabled gameobjects if set to true</param>
        /// <returns>A bounds that encapsulates the entire model</returns>
        public static Bounds GetTotalBounds(this Transform root, Space space, LayerMask layers, bool includeDisabled = false)
        {
            Bounds totalBounds = default;

            List<Bounds> innerBounds = new List<Bounds>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (((1 << renderer.gameObject.layer) & layers.value) != 0 && (includeDisabled || renderer.gameObject.activeSelf))
                {
                    var currentBounds = renderer.transform.GetBounds(space);
                    if (space == Space.Self && renderer.transform != root)
                    {
                        var adjustedMin = renderer.transform.TransformPointToAnotherSpace(root, currentBounds.min);
                        var adjustedMax = renderer.transform.TransformPointToAnotherSpace(root, currentBounds.max);
                        var adjustedCenter = renderer.transform.TransformPointToAnotherSpace(root, currentBounds.center);
                        currentBounds = new Bounds(adjustedCenter, Vector3.zero);
                        currentBounds.SetMinMax(adjustedMin, adjustedMax);
                    }
                    innerBounds.Add(currentBounds);
                }
            
            if (innerBounds.Count > 0)
                totalBounds = Combine(innerBounds.ToArray());
            else
                totalBounds = new Bounds(space == Space.Self ? root.localPosition : root.position, Vector3.zero);

            return totalBounds;
        }
        /// <summary>
        /// Gets only the current transform's bounds.
        /// </summary>
        /// <param name="transform">The transform of the object</param>
        /// <param name="space">An option to return the bounds based on local or world space</param>
        /// <returns>A bounds that encapsulates only the given transform's model</returns>
        public static Bounds GetBounds(this Transform transform, Space space)
        {
            Bounds singleBounds = default;

            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (space == Space.World)
                    singleBounds = renderer.bounds;
                else
                {
                    if (renderer is SpriteRenderer)
                        singleBounds = ((SpriteRenderer)renderer).sprite.bounds;
                    else
                    {
                        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                            singleBounds = meshFilter.sharedMesh.bounds;
                    }
                }
            }

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
            else
                Debug.LogError("BoundsHelpers: No bounds to combine");

            return combined;
        }
        /// <summary>
        /// Combines two bounds together
        /// </summary>
        /// <param name="current">The first bounds</param>
        /// <param name="other">The second bounds</param>
        /// <returns>The combined bounds</returns>
        public static Bounds Combine(this Bounds current, Bounds other)
        {
            return Combine(new Bounds[] { current, other });
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