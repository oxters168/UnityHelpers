using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
        public Transform lookAt;
        public bool mimicX = true, mimicY = true, mimicZ = true;
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

                Vector3 mimickedPosition = new Vector3(mimicX ? other.position.x : transform.position.x, mimicY ? other.position.y : transform.position.y, mimicZ ? other.position.z : transform.position.z);
                Vector3 nextPosition = mimickedPosition + other.right * offset.x + other.up * offset.y + other.forward * offset.z;
                if (lerpPosition)
                    nextPosition = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * lerpPositionAmount);
                transform.position = nextPosition;

                Quaternion mimickedRotation = other.rotation;
                if (lookAt != null)
                    mimickedRotation = Quaternion.LookRotation(lookAt.position - transform.position);
                Quaternion nextRotation = mimickedRotation * Quaternion.Euler(rotOffset);
                if (lerpRotation)
                    nextRotation = Quaternion.Lerp(transform.rotation, nextRotation, Time.deltaTime * lerpRotationAmount);
                transform.rotation = nextRotation;
            }
            else if (!errored)
            {
                Debug.LogError("MimicTransform(" + transform.name + "): Can't mimic nothing");
                errored = true;
            }
        }
    }
}