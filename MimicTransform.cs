﻿using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
        /// <summary>
        /// When true will use MovePosition and MoveRotation in Rigidbody. When false will set transform position and rotation.
        /// </summary>
        [Tooltip("When true will use MovePosition and MoveRotation in Rigidbody. When false will set transform position and rotation.")]
        public bool physicsBased;
        private Rigidbody AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponentInParent<Rigidbody>(); return _affectedBody; } }
        private Rigidbody _affectedBody;

        public bool mimicLocalPosition;
        public bool mimicX = true, mimicY = true, mimicZ = true;
        public bool mimicRotation;
        public bool mimicLocalRotation;

        [Space(10)]
        public Transform lookAt;

        [Space(10)]
        public Vector3 offset;
        public Vector3 rotOffset;
        [Space(10)]
        public bool lerpPosition;
        public float lerpPositionAmount = 5;
        public bool lerpRotation;
        public float lerpRotationAmount = 5;
        private bool errored;

        void Update()
        {
            if (other != null)
            {
                errored = false;

                if (mimicLocalPosition || mimicX || mimicY || mimicZ)
                {
                    Vector3 mimickedPosition = mimicLocalPosition ? transform.localPosition : transform.position;
                    Vector3 otherPosition = mimicLocalPosition ? other.localPosition : other.position;
                    if (mimicX)
                        mimickedPosition.x = otherPosition.x;
                    if (mimicY)
                        mimickedPosition.y = otherPosition.y;
                    if (mimicZ)
                        mimickedPosition.z = otherPosition.z;

                    Vector3 nextPosition = mimickedPosition + Vector3.right * offset.x + Vector3.up * offset.y + Vector3.forward * offset.z;
                    if (lerpPosition)
                        nextPosition = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * lerpPositionAmount);

                    if (mimicLocalPosition)
                        nextPosition = LocalPositionToWorld(nextPosition);

                    if (!physicsBased)
                        transform.position = nextPosition;
                    else if (AffectedBody != null)
                        AffectedBody.MovePosition(nextPosition);
                }

                if (lookAt != null || mimicRotation || mimicLocalRotation)
                {
                    //Rotation stuff
                    Quaternion mimickedRotation = mimicLocalRotation ? other.localRotation : other.rotation;
                    if (lookAt != null)
                        mimickedRotation = Quaternion.LookRotation(lookAt.position - transform.position);
                    Quaternion nextRotation = mimickedRotation * Quaternion.Euler(rotOffset);
                    if (lerpRotation)
                        nextRotation = Quaternion.Lerp(mimicLocalRotation ? transform.localRotation : transform.rotation, nextRotation, Time.deltaTime * lerpRotationAmount);

                    if (lookAt == null && mimicLocalRotation)
                        nextRotation = LocalRotationToWorld(nextRotation);

                    if (!physicsBased)
                        transform.rotation = nextRotation;
                    else
                        AffectedBody.MoveRotation(nextRotation);
                }
            }
            else if (!errored)
            {
                Debug.LogError("MimicTransform(" + transform.name + "): Can't mimic nothing");
                errored = true;
            }
        }

        private Vector3 LocalPositionToWorld(Vector3 localPosition)
        {
            Vector3 worldPosition = localPosition;
            if (transform.parent != null)
                worldPosition = transform.parent.TransformPoint(worldPosition);
            return worldPosition;
        }
        private Quaternion LocalRotationToWorld(Quaternion localRotation)
        {
            Quaternion worldRotation = localRotation;
            if (transform.parent != null)
                worldRotation = transform.parent.TransformRotation(worldRotation);
            return worldRotation;
        }
    }
}