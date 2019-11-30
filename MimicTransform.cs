using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
        public bool lerp;
        public float lerpAmount = 5;
        private bool errored;

        void Update()
        {
            if (other != null)
            {
                errored = false;

                Vector3 nextPosition = other.position;
                Quaternion nextRotation = other.rotation;

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
                Debug.LogError("Can't mimic nothing");
                errored = true;
            }
        }
    }
}