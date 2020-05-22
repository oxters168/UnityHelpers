using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class Grabber : MonoBehaviour
    {
        private Dictionary<string, GrabInfo> grabSpots = new Dictionary<string, GrabInfo>();
        public Transform parent;
        public float maxForce;
        public bool debug;

        void Update()
        {
            foreach (var grabSpot in grabSpots)
            {
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

                grabSpot.Value.RefreshInRange(
                    (currentGrabbableItem) =>
                    {
                        if (canGrab)
                        {
                            currentGrabbableItem.Grab(parent, maxForce);
                        }
                        else if (!grabSpot.Value.grab)
                        {
                            currentGrabbableItem.Ungrab(parent);
                        }
                    },
                    (lostItem) =>
                    {
                        //Stop grabbing any item that went out of range
                        lostItem.Ungrab(parent);
                    }
                );

                if (debug)
                    grabSpot.Value.DrawDebugSphere();
            }
        }

        public void AddGrabSpot(string name)
        {
            AddGrabSpot(name, default);
        }
        public void AddGrabSpot(string name, SpherecastInfo dimensions)
        {
            grabSpots.Add(name, new GrabInfo() { spherecastInfo = dimensions });
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
            public SpherecastInfo spherecastInfo;
            public bool grab;
            public bool grabbed;
            private List<IGrabbable> inRange = new List<IGrabbable>();
            private Transform debugSphere;

            public void RefreshInRange(System.Action<IGrabbable> enteredRange, System.Action<IGrabbable> leftRange)
            {
                var oldInRange = inRange;
                inRange = new List<IGrabbable>();
                var inCast = Physics.SphereCastAll(spherecastInfo.position, spherecastInfo.radius, spherecastInfo.direction, spherecastInfo.distance, spherecastInfo.castMask);
                foreach (var itemInCast in inCast)
                {
                    var grabbableItem = itemInCast.rigidbody.GetComponent<IGrabbable>();
                    if (grabbableItem != null)
                    {
                        inRange.Add(grabbableItem);
                        enteredRange?.Invoke(grabbableItem);
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
                    debugSphere = PoolManager.GetPool("DebugSpheres").Get();

                if (debugSphere != null)
                {
                    debugSphere.position = spherecastInfo.position + spherecastInfo.direction * spherecastInfo.distance;
                    debugSphere.localScale = Vector3.one * spherecastInfo.radius * 2;
                }
                else
                    Debug.LogError("Debugging GrabInfo requires a pool in pool manager called DebugSpheres");
            }
            public void ReturnDebugSphere()
            {
                if (debugSphere != null)
                    PoolManager.GetPool("DebugSpheres")?.Return(debugSphere);
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