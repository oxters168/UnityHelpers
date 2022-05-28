using UnityEngine;

namespace UnityHelpers
{
    public class FirstPersonCameraController : BaseCameraController
    {
        public Transform target;
        public Vector3 offset;
        public Vector3 lookDirection;

        [Range(0.01f, 1)]
        public float shiftMinimum = 0.5f;

        public event ShiftHandler shiftRight, shiftLeft;
        public delegate void ShiftHandler();

        private bool asserted;

        protected override void ApplyInput()
        {
            if (!asserted)
            {
                Debug.Assert(target != null, "First Person Camera: Target not set!");
                asserted = true;
            }

            if (target != null)
            {
                transform.position = target.position + offset;
                //transform.forward = target.forward;
                transform.rotation = Quaternion.Euler(lookDirection);
                asserted = false;
            }
            else
                transform.position = Vector3.zero;

            if (strafe >= shiftMinimum || lookHorizontal >= shiftMinimum)
                shiftRight?.Invoke();
            else if (strafe <= -shiftMinimum || lookHorizontal <= -shiftMinimum)
                shiftLeft?.Invoke();

        }
    }
}