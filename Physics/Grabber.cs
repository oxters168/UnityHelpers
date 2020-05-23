using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class Grabber : MonoBehaviour
    {
        private Dictionary<string, GrabInfo> grabSpots = new Dictionary<string, GrabInfo>();
        //public Transform parent;
        public float maxForce;
        public bool debug;

        void Update()
        {
            //string debugOutput = "";
            foreach (var grabSpot in grabSpots)
            {
                //debugOutput += "\n" + grabSpot.Key + "\nRadius: " + grabSpot.Value.spherecastInfo.radius;

                bool canGrab = false;
                if (grabSpot.Value.grab && !grabSpot.Value.grabbed)
                {
                    canGrab = true;
                    grabSpot.Value.grabbed = true;
                }
                else if (!grabSpot.Value.grab)
                {
                    grabSpot.Value.grabbed = false;
                }

                grabSpot.Value.grab = false;
                grabSpot.Value.RefreshInRange(
                    (currentGrabbableItem) =>
                    {
                        var currentItemBounds = currentGrabbableItem.GetTotalBounds(Space.World);
                        var itemBoundsInDirection = currentItemBounds.extents.Multiply(grabSpot.Value.spherecastInfo.direction);
                        //float itemBoundsExtents = (itemBoundsInDirection.x + itemBoundsInDirection.y + itemBoundsInDirection.z) / 3;
                        float itemBoundsExtents = itemBoundsInDirection.magnitude * 1.5f;
                        grabSpot.Value.grab = itemBoundsExtents >= grabSpot.Value.spherecastInfo.radius;
                        //debugOutput += "\n" + currentGrabbableItem.name + "\nExtents: " + itemBoundsExtents;

                        if (canGrab)
                        {
                            currentGrabbableItem.GetComponent<IGrabbable>().Grab(grabSpot.Value, maxForce);
                        }
                        else if (!grabSpot.Value.grab)
                        {
                            currentGrabbableItem.GetComponent<IGrabbable>().Ungrab(grabSpot.Value);
                        }
                    },
                    (lostItem) =>
                    {
                        //Stop grabbing any item that went out of range
                        lostItem.Ungrab(grabSpot.Value);
                    }
                );

                if (debug)
                {
                    grabSpot.Value.DrawDebugSphere();
                }
            }

            //DebugPanel.Log(gameObject.name, debugOutput);
        }

        public void AddGrabSpot(string name, Transform grabbedParentedTo)
        {
            AddGrabSpot(name, grabbedParentedTo, default);
        }
        public void AddGrabSpot(string name, Transform grabbedParentedTo, SpherecastInfo dimensions)
        {
            grabSpots.Add(name, new GrabInfo() { parent = grabbedParentedTo, spherecastInfo = dimensions });
        }
        public void SetGrabSpotGrabbing(string name, bool isGrabbing)
        {
            grabSpots[name].grab = isGrabbing;
        }
        public void SetGrabSpotDimensions(string name, SpherecastInfo dimensions)
        {
            grabSpots[name].spherecastInfo = dimensions;
        }
        public void RemoveGrabSpot(string name)
        {
            grabSpots[name].ReturnDebugSphere();
            grabSpots.Remove(name);
        }

        public class GrabInfo
        {
            public Transform parent;
            public SpherecastInfo spherecastInfo;
            public bool grab;
            public bool grabbed;
            private List<IGrabbable> inRange = new List<IGrabbable>();
            private Renderer debugSphere;

            public void RefreshInRange(System.Action<Transform> enteredRange, System.Action<IGrabbable> leftRange)
            {
                var oldInRange = inRange;
                inRange = new List<IGrabbable>();
                if (spherecastInfo.radius > 0)
                {
                    var inCast = Physics.SphereCastAll(spherecastInfo.position, spherecastInfo.radius, spherecastInfo.direction, spherecastInfo.distance, spherecastInfo.castMask);
                    foreach (var itemInCast in inCast)
                    {
                        var grabbableItem = itemInCast.rigidbody.GetComponent<IGrabbable>();
                        if (grabbableItem != null)
                        {
                            inRange.Add(grabbableItem);
                            enteredRange?.Invoke(itemInCast.transform);
                        }
                    }
                }

                foreach (var lostItem in oldInRange.Except(inRange))
                {
                    leftRange?.Invoke(lostItem);
                }
            }

            public void DrawDebugSphere()
            {
                if (debugSphere == null)
                    debugSphere = PoolManager.GetPool("DebugSpheres").Get<Renderer>();

                if (debugSphere != null)
                {
                    debugSphere.transform.position = spherecastInfo.position + spherecastInfo.direction * spherecastInfo.distance;
                    debugSphere.transform.localScale = Vector3.one * spherecastInfo.radius * 2;
                    debugSphere.material.color = grab ? new Color(1, 0, 0, 0.25f) : new Color(0, 1, 0, 0.25f);
                }
                else
                    Debug.LogError("Debugging GrabInfo requires a pool in pool manager called DebugSpheres");
            }
            public void ReturnDebugSphere()
            {
                if (debugSphere != null)
                    PoolManager.GetPool("DebugSpheres")?.Return(debugSphere.transform);
            }

            public int CountInRange()
            {
                return inRange.Count;
            }
        }
    }

    [System.Serializable]
    public struct SpherecastInfo
    {
        public LayerMask castMask;
        public Vector3 position;
        public Vector3 direction;
        public float distance;
        public float radius;
    }
}