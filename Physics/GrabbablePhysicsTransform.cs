using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    [System.Serializable]
    public class GrabEvent : UnityEngine.Events.UnityEvent<IGrabbable, LocalInfo> {}
    [System.Serializable]
    public class ManipulateEvent : UnityEngine.Events.UnityEvent<IGrabbable, float> {}

    public class GrabbablePhysicsTransform : MonoBehaviour, IGrabbable
    {
        private List<LocalInfo> grabbers = new List<LocalInfo>();

        private PhysicsTransform PhysicsObject { get { if (_physicsObject == null) _physicsObject = GetComponent<PhysicsTransform>(); return _physicsObject; } }
        private PhysicsTransform _physicsObject;
        private bool createdSelf;

        private Transform previousFollow;
        private Transform previousParent;
        private bool previouslyEnabled;
        private bool previouslyCounteractingGravity;
        private float previousPosAnchoring;
        private float previousRotAnchoring;
        private Vector3 previousPos;
        private Quaternion previousRot;
        private float previousMaxForce;

        /// <summary>
        /// The minimum amount of distance before squeeze or stretch is called
        /// </summary>
        [Tooltip("The minimum amount of distance before squeeze or stretch is called")]
        public float minSqueezeStretchDelta = 0.01f;
        private float prevDistance = -1;

        /// <summary>
        /// Called when this object is grabbed when no other object is grabbing it
        /// </summary>
        [Space(10), Tooltip("Called when this object is grabbed when no other object is grabbing it")]
        public GrabEvent OnFirstGrab;
        /// <summary>
        /// Called when an object grabs this object
        /// </summary>
        [Tooltip("Called when an object grabs this object")]
        public GrabEvent OnGrabbed;
        /// <summary>
        /// Called when an objects stops grabbing this object
        /// </summary>
        [Tooltip("Called when an objects stops grabbing this object")]
        public GrabEvent OnUngrabbed;
        /// <summary>
        /// Called when there are no more objects grabbing this object
        /// </summary>
        [Tooltip("Called when there are no more objects grabbing this object")]
        public GrabEvent OnCompletelyUngrabbed;

        /// <summary>
        /// Called when there are two or more grabbers and the summed distance between them increased
        /// </summary>
        [Space(10), Tooltip("Called when there are two or more grabbers and the summed distance between them increased")]
        public ManipulateEvent OnStretch;
        /// <summary>
        /// Called when there are two or more grabbers and the summed distanced between them decreased
        /// </summary>
        [Tooltip("Called when there are two or more grabbers and the summed distanced between them decreased")]
        public ManipulateEvent OnSqueeze;

        void Update()
        {            
            //Calculate position and rotation based on parents
            if (grabbers.Count > 0 && PhysicsObject != null)
            {
                var positions = grabbers.Select(grabber => grabber.info.parent.TransformPoint(grabber.localPosition));
                var rotations = grabbers.Select(grabber => grabber.info.parent.TransformRotation(grabber.localRotation));
                var averagedPosition = positions.Average();
                var averagedRotation = rotations.Average();
                PhysicsObject.position = averagedPosition;
                PhysicsObject.rotation = averagedRotation;
            }

            DistanceEvent();
        }

        private void DistanceEvent()
        {
            if (grabbers.Count > 1)
            {
                float totalDistance = 0;
                //Calculate the 'circumference' of the grabbers
                for (int i = 1; i < grabbers.Count; i++)
                    totalDistance = (grabbers[i].info.parent.position - grabbers[i - 1].info.parent.position).sqrMagnitude;

                if (prevDistance >= 0)
                {
                    float minSquared = minSqueezeStretchDelta * minSqueezeStretchDelta;
                    float delta = totalDistance - prevDistance;
                    if (delta > minSquared)
                        OnStretch?.Invoke(this, delta);
                    else if (delta < -minSquared)
                        OnSqueeze?.Invoke(this, delta);
                }
                prevDistance = totalDistance;
            }
            else
                prevDistance = -1;
        }

        public LocalInfo CreateLocalInfo(Grabber.GrabInfo grabInfo, float maxForce)
        {
            var localInfo = new LocalInfo()
            {
                info = grabInfo,
                localPosition = grabInfo.parent.InverseTransformPoint(transform.position),
                localRotation = grabInfo.parent.InverseTransformRotation(transform.rotation),
                maxForce = maxForce
            };
            return localInfo;
        }
        public bool GetLocalInfo(Grabber.GrabInfo grabInfo, out LocalInfo localInfo)
        {
            var matches = grabbers.Where(grabber => grabber.info == grabInfo);
            bool isCurrentlyGrabber = matches.Count() > 0;
            if (isCurrentlyGrabber)
                localInfo = matches.First();
            else
                localInfo = default;

            return isCurrentlyGrabber;
        }

        public void Grab(Grabber.GrabInfo grabberInfo, float maxForce)
        {
            Grab(CreateLocalInfo(grabberInfo, maxForce));
        }
        public void Grab(LocalInfo grabberInfo)
        {
            if (!grabbers.Exists(grabber => grabber.info == grabberInfo.info))
            {                
                //Add physics transform if doesn't already exist and store it's values (important for when it does exist)
                if (PhysicsObject == null)
                {
                    _physicsObject = gameObject.AddComponent<PhysicsTransform>();
                    createdSelf = true;
                }

                //Only store values if this is the first time being grabbed
                if (grabbers.Count <= 0)
                {
                    StoreValues();
                    OnFirstGrab?.Invoke(this, grabberInfo);
                }

                //Add current grabber as parent
                grabbers.Add(grabberInfo);

                //Set the values of the physics transform so that it is ready for grabbage
                PhysicsObject.follow = null;
                PhysicsObject.parent = null;
                PhysicsObject.anchorPositionPercent = 0;
                PhysicsObject.anchorRotationPercent = 0;
                PhysicsObject.counteractGravity = true;
                PhysicsObject.maxForce = grabberInfo.maxForce;
                PhysicsObject.position = transform.position;
                PhysicsObject.rotation = transform.rotation;
                PhysicsObject.enabled = true;

                OnGrabbed?.Invoke(this, grabberInfo);
            }
        }
        public void Ungrab(LocalInfo grabberInfo)
        {
            //Only if the grabber was grabbing this object
            if (grabbers.Contains(grabberInfo))
            {
                grabbers.Remove(grabberInfo);

                //If there are no more objects grabbing, then restore the PhysicsTransform script and possibly destroy
                if (grabbers.Count <= 0)
                {
                    RestoreValues();
                    if (createdSelf)
                    {
                        GameObject.Destroy(_physicsObject);
                        createdSelf = false;
                    }

                    OnCompletelyUngrabbed?.Invoke(this, grabberInfo);
                }

                OnUngrabbed?.Invoke(this, grabberInfo);
            }
        }
        public void Ungrab(Grabber.GrabInfo grabberInfo)
        {
            var matches = grabbers.Where(grabber => grabber.info == grabberInfo);
            bool isCurrentlyGrabber = matches.Count() > 0;
            if (isCurrentlyGrabber)
                Ungrab(matches.First());
        }
        public void UngrabAll()
        {
            //Start from then end since they'll be removed as we go
            for (int i = grabbers.Count - 1; i >= 0; i--)
                Ungrab(grabbers[i]);
        }

        public LocalInfo GetGrabber(int index)
        {
            if (index < 0 || index >= grabbers.Count)
                throw new System.IndexOutOfRangeException();

            return grabbers[index];
        }
        public int GetGrabCount()
        {
            return grabbers.Count;
        }
        public float GetMaxForce()
        {
            return PhysicsObject.maxForce;
        }

        private void StoreValues()
        {
            previousFollow = PhysicsObject.follow;
            previousParent = PhysicsObject.parent;
            previouslyEnabled = PhysicsObject.enabled;
            previouslyCounteractingGravity = PhysicsObject.counteractGravity;
            previousPos = PhysicsObject.position;
            previousRot = PhysicsObject.rotation;
            previousPosAnchoring = PhysicsObject.anchorPositionPercent;
            previousRotAnchoring = PhysicsObject.anchorRotationPercent;
            previousMaxForce = PhysicsObject.maxForce;
        }
        private void RestoreValues()
        {
            if (PhysicsObject != null)
            {
                PhysicsObject.enabled = previouslyEnabled;
                PhysicsObject.follow = previousFollow;
                PhysicsObject.parent = previousParent;
                PhysicsObject.position = previousPos;
                PhysicsObject.rotation = previousRot;
                PhysicsObject.counteractGravity = previouslyCounteractingGravity;
                PhysicsObject.anchorPositionPercent = previousPosAnchoring;
                PhysicsObject.anchorRotationPercent = previousRotAnchoring;
                PhysicsObject.maxForce = previousMaxForce;
            }
            else
                Debug.LogWarning(gameObject.name + ": Could not restore values to non existant physics transform");
        }
    }
    
    public struct LocalInfo
    {
        public Grabber.GrabInfo info;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public float maxForce;
    }
}