using UnityEngine;

namespace UnityHelpers
{
    public class MimicTransform : MonoBehaviour
    {
        public Transform other;
        private bool errored;

        void FixedUpdate()
        {
            if (other != null)
            {
                errored = false;
                transform.position = other.position;
                transform.rotation = other.rotation;
            }
            else if (!errored)
            {
                Debug.LogError("Can't mimic nothing");
                errored = true;
            }
        }
    }
}