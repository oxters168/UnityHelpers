using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    [System.Serializable]
    public class GrabEvent : UnityEngine.Events.UnityEvent<IGrabbable, LocalInfo> {}
    [System.Serializable]
    public class ManipulateEvent : UnityEngine.Events.UnityEvent<IGrabbable, float> {}

    public struct LocalInfo
    {
        public Grabber.GrabInfo info;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public float maxForce;
    }

    public abstract class GrabbableBase : MonoBehaviour, IGrabbable
    {
        protected List<LocalInfo> grabbers = new List<LocalInfo>();

        /// <summary>
        /// The minimum amount of distance before squeeze or stretch is called
        /// </summary>
        [Tooltip("The minimum amount of distance before squeeze or stretch is called")]
        public float minSqueezeStretchDelta = 0.01f;
        protected float prevDistance = -1;

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

        protected virtual void Update()
        {            
            CalculateNewPositionAndRotation(ApplyPositionAndRotation);
            DistanceEvent();
        }

        protected virtual void CalculateNewPositionAndRotation(System.Action<Vector3, Quaternion> outputAction)
        {
            //Calculate position and rotation based on parents
            if (grabbers.Count > 0)
            {
                var positions = grabbers.Select(grabber => grabber.info.parent.TransformPoint(grabber.localPosition));
                var rotations = grabbers.Select(grabber => grabber.info.parent.TransformRotation(grabber.localRotation));
                var averagedPosition = positions.Average();
                var averagedRotation = rotations.Average();

                if (outputAction != null)
                    outputAction(averagedPosition, averagedRotation);
                else
                    Debug.LogError("Output action not set");
            }
        }
        protected abstract void ApplyPositionAndRotation(Vector3 position, Quaternion rotation);

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

        public virtual LocalInfo CreateLocalInfo(Grabber.GrabInfo grabInfo, float maxForce)
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
        public virtual bool GetLocalInfo(Grabber.GrabInfo grabInfo, out LocalInfo localInfo)
        {
            var matches = grabbers.Where(grabber => grabber.info == grabInfo);
            bool isCurrentlyGrabber = matches.Count() > 0;
            if (isCurrentlyGrabber)
                localInfo = matches.First();
            else
                localInfo = default;

            return isCurrentlyGrabber;
        }

        public virtual void Grab(Grabber.GrabInfo grabberInfo, float maxForce)
        {
            Grab(CreateLocalInfo(grabberInfo, maxForce));
        }
        public virtual void Grab(LocalInfo grabberInfo)
        {
            if (!grabbers.Exists(grabber => grabber.info == grabberInfo.info))
            {                
                //Only store values if this is the first time being grabbed
                if (grabbers.Count <= 0)
                    OnFirstGrab?.Invoke(this, grabberInfo);

                //Add current grabber as parent
                grabbers.Add(grabberInfo);

                OnGrabbed?.Invoke(this, grabberInfo);
            }
        }
        public virtual void Ungrab(LocalInfo grabberInfo)
        {
            //Only if the grabber was grabbing this object
            if (grabbers.Contains(grabberInfo))
            {
                grabbers.Remove(grabberInfo);

                //If there are no more objects grabbing, then restore the PhysicsTransform script and possibly destroy
                if (grabbers.Count <= 0)
                    OnCompletelyUngrabbed?.Invoke(this, grabberInfo);

                OnUngrabbed?.Invoke(this, grabberInfo);
            }
        }
        public virtual void Ungrab(Grabber.GrabInfo grabberInfo)
        {
            var matches = grabbers.Where(grabber => grabber.info == grabberInfo);
            bool isCurrentlyGrabber = matches.Count() > 0;
            if (isCurrentlyGrabber)
                Ungrab(matches.First());
        }
        public virtual void UngrabAll()
        {
            //Start from then end since they'll be removed as we go
            for (int i = grabbers.Count - 1; i >= 0; i--)
                Ungrab(grabbers[i]);
        }

        public virtual LocalInfo GetGrabber(int index)
        {
            if (index < 0 || index >= grabbers.Count)
                throw new System.IndexOutOfRangeException();

            return grabbers[index];
        }
        public virtual int GetGrabCount()
        {
            return grabbers.Count;
        }
    }
}