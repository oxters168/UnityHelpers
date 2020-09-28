using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class GrabbablePhysicsTransform : GrabbableBase
    {
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

        protected override void ApplyPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            PhysicsObject.position = position;
            PhysicsObject.rotation = rotation;
        }

        public override void Grab(LocalInfo grabberInfo)
        {
            base.Grab(grabberInfo);

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
                }

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
            }
        }
        public override void Ungrab(LocalInfo grabberInfo)
        {
            base.Ungrab(grabberInfo);

            //Only if the grabber was grabbing this object
            if (grabbers.Contains(grabberInfo))
            {
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
}