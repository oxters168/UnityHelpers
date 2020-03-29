using UnityEngine;

namespace UnityHelpers
{
    public class OrbitCameraController : BaseCameraController
    {
        public Transform target;
        public Vector3 offset;
        public float distance = 10;
        public float minDistance = 0, maxDistance = 100;
        /// <summary>
        /// The angle on the world y axis
        /// </summary>
        public float upAngle;
        /// <summary>
        /// The angle on the world x axis
        /// </summary>
        public float rightAngle;
        /// <summary>
        /// The angle on the local z axis
        /// </summary>
        public float forwardAngle;

        public float moveSensitivity = 1, lookSensitivity = 1;
        [Range(0.01f, 1)]
        public float shiftMinimum = 0.5f;

        public event ShiftHandler shiftRight, shiftLeft;
        public delegate void ShiftHandler();

        private bool asserted;

        protected override void ApplyInput()
        {
            if (!asserted)
            {
                Debug.Assert(target != null, "Orbit Camera: Target not set!");
                asserted = true;
            }

            if (target != null)
            {
                transform.position = target.position + offset;
                asserted = false;
            }
            else
                transform.position = Vector3.zero;

            upAngle += lookHorizontal * lookSensitivity;
            Quaternion horizontalRot = Quaternion.AngleAxis(upAngle, Vector3.up);
            rightAngle += lookVertical * lookSensitivity;
            Quaternion verticalRot = Quaternion.AngleAxis(rightAngle, Vector3.right);
            Quaternion extraRot = Quaternion.AngleAxis(forwardAngle, Vector3.forward);
            transform.rotation = (horizontalRot * (verticalRot * Quaternion.identity)) * extraRot;

            distance -= push * moveSensitivity;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            transform.position -= transform.forward * distance;

            if (strafe >= shiftMinimum)
                shiftRight?.Invoke();
            else if (strafe <= -shiftMinimum)
                shiftLeft?.Invoke();
        }
    }
}