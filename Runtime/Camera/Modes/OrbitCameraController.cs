using UnityEngine;

namespace UnityHelpers
{
    public class OrbitCameraController : BaseCameraController
    {
        private Camera _attachedCamera;
        private Camera AttachedCamera { get { if (_attachedCamera == null) _attachedCamera = GetComponent<Camera>(); return _attachedCamera; } }

        [Space(10)]
        public Transform target;

        [Space(10), Tooltip("If set to true will use the target's local axes to offset")]
        public bool localOffset;
        public Vector3 offset;

        [Space(10)]
        public float distance = 10;
        public float minDistance = 0, maxDistance = 100;
        [Tooltip("The camera position lerp multiplier, the higher the number the faster the speed of the camera. Set to 0 to disable lerping.")]
        public float positionLerp;

        /// <summary>
        /// The angle on the world y axis
        /// </summary>
        [Space(10), Tooltip("The angle on the world y axis")]
        public float upAngle;
        /// <summary>
        /// The angle on the world x axis
        /// </summary>
        [Tooltip("The angle on the world x axis")]
        public float rightAngle;
        /// <summary>
        /// The angle on the local z axis
        /// </summary>
        [Tooltip("The angle on the local z axis")]
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

            Vector3 targetPosition = transform.position;

            if (target != null)
            {
                Vector3 offsetPosition = target.position + offset;
                if (localOffset)
                    offsetPosition = target.TransformPoint(offset);
                targetPosition = offsetPosition;
                asserted = false;
            }
            else
                targetPosition = Vector3.zero;

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
                float nextOrtho = AttachedCamera.orthographicSize;
                if (positionLerp > float.Epsilon)
                    nextOrtho = Mathf.Lerp(nextOrtho, distance, positionLerp * Time.deltaTime);
                else
                    nextOrtho = distance;

                AttachedCamera.orthographicSize = nextOrtho;
                cameraPhysicalDistance = AttachedCamera.PerspectiveDistanceFromHeight(distance) * 10f;
            }
            targetPosition -= transform.forward * cameraPhysicalDistance;
            if (positionLerp > float.Epsilon)
                targetPosition = Vector3.Lerp(transform.position, targetPosition, positionLerp * Time.deltaTime);
            transform.position = targetPosition;

            if (strafe >= shiftMinimum)
                shiftRight?.Invoke();
            else if (strafe <= -shiftMinimum)
                shiftLeft?.Invoke();
        }
    }
}