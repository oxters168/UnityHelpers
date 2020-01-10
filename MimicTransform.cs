using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
        [Space(10)]
        public Vector3 offset;
        public Vector3 rotOffset;
        [Space(10)]
        public bool lerp;
        public float lerpAmount = 5;
        private bool errored;

        void Update()
        {
            if (other != null)
            {
                errored = false;

                Vector3 nextPosition = other.position + other.right * offset.x + other.up * offset.y + other.forward * offset.z;
                Quaternion nextRotation = Quaternion.Euler(rotOffset) * other.rotation;

                if (lerp)
                {
                    nextPosition = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * lerpAmount);
                    nextRotation = Quaternion.Lerp(transform.rotation, nextRotation, Time.deltaTime * lerpAmount);
                }

                transform.position = nextPosition;
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