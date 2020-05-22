using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UnityHelpers
{
    public class GrabbablePhysicsTransform : MonoBehaviour, IGrabbable
    {
        private List<LocalInfo> parents = new List<LocalInfo>();

        private PhysicsTransform PhysicsObject { get { if (_physicsObject == null) _physicsObject = GetComponent<PhysicsTransform>(); return _physicsObject; } }
        private PhysicsTransform _physicsObject;
        private bool createdSelf;

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
            if (parents.Count > 0 && PhysicsObject != null)
            {
                var positions = parents.Select(parentInfo => parentInfo.parent.TransformPoint(parentInfo.localPosition));
                var rotations = parents.Select(parentInfo => parentInfo.parent.TransformRotation(parentInfo.localRotation));
                var averagedPosition = positions.Average();
                var averagedRotation = rotations.Average();
                PhysicsObject.position = averagedPosition;
                PhysicsObject.rotation = averagedRotation;
            }
        }

        public void Grab(Transform grabber, float maxForce)
        {
            if (!parents.Exists(parentInfo => parentInfo.parent == grabber))
            {
                //Add current grabber as parent
                parents.Add(new LocalInfo()
                {
                    parent = grabber,
                    localPosition = grabber.InverseTransformPoint(transform.position),
                    localRotation = grabber.InverseTransformRotation(transform.rotation)
                });

                //Add physics transform if doesn't already exist and store it's values (important for when it does exist)
                if (PhysicsObject == null)
                {
                    _physicsObject = gameObject.AddComponent<PhysicsTransform>();
                    createdSelf = true;
                }
                StoreValues();

                //Set the values of the physics transform so that it is ready for grabbage
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
        public void Ungrab(Transform grabber)
        {
            //Remove grabber from parents list
            var matches = parents.Where(parentInfo => parentInfo.parent == grabber);
            if (matches.Count() > 0)
                parents.Remove(matches.First());

            //If there are no more objects grabbing, then restore the PhysicsTransform script and possibly destroy
            if (parents.Count <= 0)
            {
                RestoreValues();
                if (createdSelf)
                {
                    GameObject.Destroy(_physicsObject);
                    createdSelf = false;
                }
            }
        }

        private void StoreValues()
        {
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
            if (_physicsObject != null)
            {
                _physicsObject.enabled = previouslyEnabled;
                _physicsObject.parent = previousParent;
                _physicsObject.position = previousPos;
                _physicsObject.rotation = previousRot;
                _physicsObject.counteractGravity = previouslyCounteractingGravity;
                _physicsObject.anchorPositionPercent = previousPosAnchoring;
                _physicsObject.anchorRotationPercent = previousRotAnchoring;
                _physicsObject.maxForce = previousMaxForce;
            }
            else
                Debug.LogWarning(gameObject.name + ": Could not restore values to non existant physics transform");
        }
    }
    
    public struct LocalInfo
    {
        public Transform parent;
        public Vector3 localPosition;
        public Quaternion localRotation;
    }
}