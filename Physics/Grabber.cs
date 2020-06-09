using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class Grabber : MonoBehaviour
    {
        private Dictionary<string, GrabInfo> grabSpots = new Dictionary<string, GrabInfo>();
        //public Transform parent;
        /// <summary>
        /// The item size multiplier to be compared to the grab size when grabbing (Larger value means more easily grabbed)
        /// </summary>
        [Tooltip("The item size multiplier to be compared to the grab size when grabbing (Larger value means more easily grabbed)")]
        public float itemGrabPercentSize = 1.25f;
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
                        var itemBoundsInDirection = currentItemBounds.size.Multiply(grabSpot.Value.physicsCaster.GetDirection());
                        //float itemBoundsExtents = (itemBoundsInDirection.x + itemBoundsInDirection.y + itemBoundsInDirection.z) / 3;
                        float itemBoundsSize = itemBoundsInDirection.magnitude * itemGrabPercentSize;
                        grabSpot.Value.grab = itemBoundsSize >= grabSpot.Value.physicsCaster.GetSize();
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
                    grabSpot.Value.DrawDebugCast();
                }
            }

            //DebugPanel.Log(gameObject.name, debugOutput);
        }

        public void AddGrabSpot(string name, Transform grabbedParentedTo)
        {
            AddGrabSpot(name, grabbedParentedTo, default);
        }
        public void AddGrabSpot(string name, Transform grabbedParentedTo, ICastable dimensions)
        {
            grabSpots.Add(name, new GrabInfo() { parent = grabbedParentedTo, physicsCaster = dimensions });
        }
        public void SetGrabSpotGrabbing(string name, bool isGrabbing)
        {
            grabSpots[name].grab = isGrabbing;
        }
        public void SetGrabSpotDimensions(string name, ICastable dimensions)
        {
            grabSpots[name].physicsCaster = dimensions;
        }
        public void RemoveGrabSpot(string name)
        {
            grabSpots[name].ReturnDebugCast();
            grabSpots.Remove(name);
        }

        public class GrabInfo
        {
            public Transform parent;
            public ICastable physicsCaster;
            public bool grab;
            public bool grabbed;
            private List<IGrabbable> inRange = new List<IGrabbable>();
            private LineRenderer debugLine;

            public void RefreshInRange(System.Action<Transform> enteredRange, System.Action<IGrabbable> leftRange)
            {
                var oldInRange = inRange;
                inRange = new List<IGrabbable>();
                if (physicsCaster != null)
                {
                    var inCast = physicsCaster.CastAll();
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

            public void DrawDebugCast()
            {
                if (debugLine == null)
                    debugLine = PoolManager.GetPool("DebugLines").Get<LineRenderer>();

                if (debugLine != null)
                {
                    if (physicsCaster != null)
                    {
                        debugLine.SetPositions(
                            new Vector3[]
                            {
                                physicsCaster.GetPosition(),
                                physicsCaster.GetPosition() + physicsCaster.GetDirection() * physicsCaster.GetSize()
                            }
                        );
                        //debugLine.transform.position = physicsCaster.GetPosition() + physicsCaster.GetDirection() * physicsCaster.GetSize();
                        //debugLine.transform.localScale = Vector3.one * physicsCaster.radius * 2;
                        debugLine.material.color = grab ? new Color(0, 1, 0) : new Color(1, 1, 1);
                    }
                }
                else
                    Debug.LogError("Debugging GrabInfo requires a pool in pool manager called DebugLines");
            }
            public void ReturnDebugCast()
            {
                if (debugLine != null)
                    PoolManager.GetPool("DebugSpheres")?.Return(debugLine.transform);
            }

            public int CountInRange()
            {
                return inRange.Count;
            }
        }
    }
}