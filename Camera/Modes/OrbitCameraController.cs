using UnityEngine;

namespace UnityHelpers
{
    public class OrbitCameraController : BaseCameraController
    {
        private Camera _attachedCamera;
        private Camera AttachedCamera { get { if (_attachedCamera == null) _attachedCamera = GetComponent<Camera>(); return _attachedCamera; } }
        public Transform target;
        [Tooltip("If set to true will use the target's local axes to offset")]
        public bool localOffset;
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
                Vector3 offsetPosition = target.position + offset;
                if (localOffset)
                    offsetPosition = target.TransformPoint(offset);
                transform.position = offsetPosition;
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
            float cameraPhysicalDistance = distance;
            if (AttachedCamera.orthographic)
            {
                AttachedCamera.orthographicSize = distance;
                cameraPhysicalDistance = AttachedCamera.PerspectiveDistanceFromHeight(distance) * 10f;
            }
            transform.position -= transform.forward * cameraPhysicalDistance;

            if (strafe >= shiftMinimum)
                shiftRight?.Invoke();
            else if (strafe <= -shiftMinimum)
                shiftLeft?.Invoke();
        }
    }
}