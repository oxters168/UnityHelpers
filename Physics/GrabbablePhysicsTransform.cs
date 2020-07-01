using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
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
        }

        public void Grab(Grabber.GrabInfo grabberInfo, float maxForce)
        {
            if (!grabbers.Exists(grabber => grabber.info == grabberInfo))
            {
                //Add current grabber as parent
                grabbers.Add(new LocalInfo()
                {
                    info = grabberInfo,
                    localPosition = grabberInfo.parent.InverseTransformPoint(transform.position),
                    localRotation = grabberInfo.parent.InverseTransformRotation(transform.rotation)
                });

                //Add physics transform if doesn't already exist and store it's values (important for when it does exist)
                if (PhysicsObject == null)
                {
                    _physicsObject = gameObject.AddComponent<PhysicsTransform>();
                    createdSelf = true;
                }
                StoreValues();

                //Set the values of the physics transform so that it is ready for grabbage
                PhysicsObject.follow = null;
                PhysicsObject.parent = null;
                PhysicsObject.anchorPositionPercent = 0;
                PhysicsObject.anchorRotationPercent = 0;
                PhysicsObject.counteractGravity = true;
                PhysicsObject.maxForce = maxForce;
                PhysicsObject.position = transform.position;
                PhysicsObject.rotation = transform.rotation;
                PhysicsObject.enabled = true;
            }
        }
        public void Ungrab(Grabber.GrabInfo grabberInfo)
        {
            //Remove grabber from parents list
            var matches = grabbers.Where(grabber => grabber.info == grabberInfo);
            bool isCurrentlyGrabber = matches.Count() > 0;
            //Only if the grabber was grabbing this object
            if (isCurrentlyGrabber)
            {
                grabbers.Remove(matches.First());

                //If there are no more objects grabbing, then restore the PhysicsTransform script and possibly destroy
                if (grabbers.Count <= 0)
                {
                    RestoreValues();
                    if (createdSelf)
                    {
                        GameObject.Destroy(_physicsObject);
                        createdSelf = false;
                    }
                }
            }
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
    }
}