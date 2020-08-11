using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class Grabber : MonoBehaviour
    {
        private Dictionary<string, GrabInfo> grabSpots = new Dictionary<string, GrabInfo>();
        private List<IGrabbable> grabbedObjects = new List<IGrabbable>();

        /// <summary>
        /// Will grab any object within any grab spot when set to true
        /// </summary>
        [Tooltip("Will grab any object within any grab spot when set to true")]
        public bool grab;
        /// <summary>
        /// If an object is grabbed and it goes out of the range of the grabber, should it be ungrabbed?
        /// </summary>
        [Tooltip("If an object is grabbed and it goes out of the range of the grabber, should it be ungrabbed?")]
        public bool ungrabWhenOutOfRange = true;
        /// <summary>
        /// Defines how many items this grabber can hold at once
        /// </summary>
        [Tooltip("Defines how many items this grabber can hold at once")]
        public int maxCapacity = int.MaxValue;
        /// <summary>
        /// The max force the grabber can move the object with
        /// </summary>
        [Tooltip("The max force the grabber can move the object with")]
        public float maxForce = float.MaxValue;

        /// <summary>
        /// If set to true, will grab an object based on the size comparison between the grab spot and the object within
        /// </summary>
        [Space(10), Tooltip("If set to true, will grab an object based on the size comparison between the grab spot and the object within")]
        public bool grabBySpotSize;
        /// <summary>
        /// The item size multiplier to be compared to the grab size when grabbing (Larger value means more easily grabbed)
        /// </summary>
        [Tooltip("The item size multiplier to be compared to the grab size when grabbing (Larger value means more easily grabbed)")]
        public float itemGrabPercentSize = 1.25f;
        public bool debug;

        [Space(10)]
        public SpherecastInfo[] sphereGrabSpots;
        public RaycastInfo[] rayGrabSpots;

        void Start()
        {
            int currentIndex = 0;
            if (sphereGrabSpots != null)
                foreach (ICastable grabSpot in sphereGrabSpots)
                {
                    AddGrabSpot(currentIndex.ToString(), grabSpot.GetParent(), grabSpot);
                    currentIndex++;
                }
            if (rayGrabSpots != null)
                foreach (ICastable grabSpot in rayGrabSpots)
                {
                    AddGrabSpot(currentIndex.ToString(), grabSpot.GetParent(), grabSpot);
                    currentIndex++;
                }
        }
        void Update()
        {
            //string debugOutput = "";
            foreach (var grabSpot in grabSpots)
            {
                //debugOutput += "\n" + grabSpot.Key + "\nRadius: " + grabSpot.Value.spherecastInfo.radius;
                
                // grabSpot.Value.grab = grab;
                // bool attemptingToGrab = false;
                // if (grabSpot.Value.grab && !grabSpot.Value.grabbed)
                // {
                //     attemptingToGrab = true;
                //     grabSpot.Value.grabbed = true;
                // }
                // else if (!grabSpot.Value.grab)
                // {
                //     grabSpot.Value.grabbed = false;
                // }

                IList<Transform> enteredRange;
                IList<IGrabbable> leftRange;
                grabSpot.Value.RefreshInRange(out enteredRange, out leftRange);
                var leftRangeWhileGrabbing = grabbedObjects.Except(enteredRange.Select(objectInRange => objectInRange.GetComponentInParent<IGrabbable>()));
                //grabSpot.Value.grab = false;
                //grabSpot.Value.RefreshInRange((currentGrabbableItem) =>
                foreach (var currentGrabbableItem in enteredRange)
                {
                    bool itemLargerThanSpot = false;

                    if (grabBySpotSize)
                    {
                        var currentItemBounds = currentGrabbableItem.GetTotalBounds(Space.World);
                        var itemBoundsInDirection = currentItemBounds.size.Multiply(grabSpot.Value.physicsCaster.GetDirection());
                        //float itemBoundsExtents = (itemBoundsInDirection.x + itemBoundsInDirection.y + itemBoundsInDirection.z) / 3;
                        float itemBoundsSize = itemBoundsInDirection.magnitude * itemGrabPercentSize;
                        itemLargerThanSpot = itemBoundsSize >= grabSpot.Value.physicsCaster.GetSize();
                    }

                    grabSpot.Value.grab = grab || itemLargerThanSpot;
                    //debugOutput += "\n" + currentGrabbableItem.name + "\nExtents: " + itemBoundsExtents;

                    var grabbable = currentGrabbableItem.GetComponent<IGrabbable>();
                    if (grabSpot.Value.grab)
                    {
                        if (!grabbedObjects.Contains(grabbable) && grabbedObjects.Count < maxCapacity)
                        {
                            //var grabbable = currentGrabbableItem.GetComponent<IGrabbable>();
                            grabbable.Grab(grabbable.CreateLocalInfo(grabSpot.Value, maxForce));
                            grabbedObjects.Add(grabbable);
                        }
                    }
                    else if (!grabSpot.Value.grab)
                    {
                        //var grabbable = currentGrabbableItem.GetComponent<IGrabbable>();
                        LocalInfo localInfo;
                        if (grabbable.GetLocalInfo(grabSpot.Value, out localInfo))
                            grabbable.Ungrab(localInfo);

                        if (grabbedObjects.Contains(grabbable))
                            grabbedObjects.Remove(grabbable);
                    }
                };
                    //(lostItem) =>
                foreach (var lostItem in leftRange.Concat(leftRangeWhileGrabbing))
                {
                    //Stop grabbing any item that went out of range
                    if (ungrabWhenOutOfRange || !grabSpot.Value.grab)
                    {
                        LocalInfo localInfo;
                        if (lostItem.GetLocalInfo(grabSpot.Value, out localInfo))
                            lostItem.Ungrab(localInfo);
                            
                        if (grabbedObjects.Contains(lostItem))
                            grabbedObjects.Remove(lostItem);
                    }
                };
                //);

                if (debug)
                {
                    grabSpot.Value.DrawDebugCast();
                }
            }

            //DebugPanel.Log(gameObject.name, debugOutput);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = new Color(0, 1, 0, 0.5f);
            if (sphereGrabSpots != null)
            {
                foreach (SpherecastInfo sphere in sphereGrabSpots)
                {
                    Gizmos.DrawSphere(sphere.GetPosition(), sphere.GetSize());
                }
            }
            if (rayGrabSpots != null)
            {
                foreach (RaycastInfo ray in rayGrabSpots)
                {
                    Gizmos.DrawLine(ray.GetPosition(), ray.GetPosition() + ray.GetDirection() * ray.GetSize());
                }
            }
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

            public void RefreshInRange(out IList<Transform> enteredRange, out IList<IGrabbable> leftRange)
            {
                var oldInRange = inRange;
                inRange = new List<IGrabbable>();
                enteredRange = new List<Transform>();
                leftRange = new List<IGrabbable>();
                if (physicsCaster != null)
                {
                    var inCast = physicsCaster.CastAll();
                    foreach (var itemInCast in inCast)
                    {
                        var grabbableItem = itemInCast.transform.GetComponent<IGrabbable>();
                        if (grabbableItem != null)
                        {
                            inRange.Add(grabbableItem);
                            enteredRange.Add(itemInCast.transform);
                            //enteredRange?.Invoke(itemInCast.transform);
                        }
                    }
                }

                foreach (var lostItem in oldInRange.Except(inRange))
                {
                    leftRange.Add(lostItem);
                    //leftRange?.Invoke(lostItem);
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