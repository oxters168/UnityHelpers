using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
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
                    transform.localPosition = nextPosition;
                else
                    transform.position = nextPosition;

                //Rotation stuff
                Quaternion mimickedRotation = mimicLocalRotation ? other.localRotation : other.rotation;
                if (lookAt != null)
                    mimickedRotation = Quaternion.LookRotation(lookAt.position - transform.position);
                Quaternion nextRotation = mimickedRotation * Quaternion.Euler(rotOffset);
                if (lerpRotation)
                    nextRotation = Quaternion.Lerp(mimicLocalRotation ? transform.localRotation : transform.rotation, nextRotation, Time.deltaTime * lerpRotationAmount);

                if (lookAt != null || (!mimicLocalRotation && mimicRotation))
                    transform.rotation = nextRotation;
                else if (mimicLocalRotation)
                    transform.localRotation = nextRotation;
            }
            else if (!errored)
            {
                Debug.LogError("MimicTransform(" + transform.name + "): Can't mimic nothing");
                errored = true;
            }
        }
    }
}