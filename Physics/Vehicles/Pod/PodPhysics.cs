using UnityEngine;

namespace UnityHelpers
{
    public class PodPhysics : MonoBehaviour
    {
        private Rigidbody _podBody;
        private Rigidbody PodBody { get { if (_podBody == null) _podBody = GetComponentInChildren<Rigidbody>(); return _podBody; } }

        [Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
        public float minGroundDistance = 1;

        [Space(10), Tooltip("The maximum force the pod can apply to keep itself floating above the ground (in newtons)")]
        public float maxFloatingForce = float.MaxValue;
        [Tooltip("The maximum torque the pod can apply to fix its orientation (in newton meters)")]
        public float maxCorrectionTorque = float.MaxValue;

        [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
        public LayerMask groundMask = ~0;

        void FixedUpdate()
        {
            //Todo: add speed for planar movement and flight movement and apply force based on input
            Vector3 floatingForce = CalculateFloatingForce();
            PodBody.AddForce(floatingForce, ForceMode.Force);

            //Todo: break the correcting torque down to the x-axis and z-axis and take input for the y-axis
            Vector3 torque = PodBody.CalculateRequiredTorqueForRotation(Quaternion.identity, Time.fixedDeltaTime, maxCorrectionTorque);
            PodBody.AddTorque(torque, ForceMode.Force);
        }

        private Vector3 CalculateFloatingForce()
        {
            var vehicleBounds = transform.GetTotalBounds(Space.World);
            float groundCastDistance = vehicleBounds.extents.y + minGroundDistance * 5;
            RaycastHit hitInfo;
            float groundDistance = float.MaxValue;
            bool rayHitGround = Physics.Raycast(vehicleBounds.center, Vector3.down, out hitInfo, groundCastDistance, groundMask);
            if (rayHitGround)
                groundDistance = hitInfo.distance - vehicleBounds.extents.y;
            Debug.DrawRay(vehicleBounds.center + (rayHitGround ? Vector3.down * vehicleBounds.extents.y : Vector3.zero), Vector3.down * (rayHitGround ? groundDistance : groundCastDistance), rayHitGround ? Color.green : Color.red);

            float upSpeed = 0;
            Vector3 floatingForce = Vector3.zero;
            if (groundDistance < minGroundDistance)
            {
                upSpeed = (minGroundDistance - groundDistance);
                floatingForce = Vector3.up * PodBody.CalculateRequiredForceForSpeed(Vector3.up * upSpeed, Time.fixedDeltaTime, true, maxFloatingForce).y;
            }
            return floatingForce;
        }
    }
}
