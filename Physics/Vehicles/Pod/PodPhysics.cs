using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UnityHelpers
{
    public class PodPhysics : MonoBehaviour
    {
        private Rigidbody _podBody;
        /// <summary>
        /// The default values were adjusted for a vehicle with a mass of 1395
        /// </summary>
        private Rigidbody PodBody { get { if (_podBody == null) _podBody = GetComponentInChildren<Rigidbody>(); return _podBody; } }

        public Vector2 strafeStick;
        public float brake;
        public float rotate;
        public float fly;

        [Space(10), Tooltip("The speed of the speed (in m/s^2)")]
        public float acceleration = 0.4f;
        [Tooltip("The fastest speed the vehicle can achieve (in m/s)")]
        public float maxSpeed = 40;

        [Tooltip("The speed of the fly speed (in m/s^2)")]
        public float flyAcceleration = 2;
        [Tooltip("The fastest the vehicle can go up")]
        public float maxFlySpeed = 20;

        [Tooltip("The speed of the rotational speed (in deg/s^2)")]
        public float rotAcceleration = 8;
        [Tooltip("The fastest the vehicle can rotate (in deg/s)")]
        public float maxRotSpeed = 200;

        [Space(10), Tooltip("The max force applied to stop the vehicle (in newtons")]
        public float maxBrakeForce = 20000;

        [Space(10), Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
        public float minGroundDistance = 1;

        [Space(10), Tooltip("The constant applied to the proportional part of the floatation pd controller")]
        public float Kp = 20000;
        [Tooltip("The constant applied to the derivative part of the floatation pd controller")]
        public float Kd = 800000;
        [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
        public float antigravityFalloffDistance = 20;
        public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Space(10), Tooltip("The maximum torque the pod can apply to fix its orientation (in newton meters)")]
        public float maxCorrectionTorque = 60000;

        [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
        public LayerMask groundMask = ~0;

        [Space(10)]
        public bool showOrientationCasters;
        public bool showCalculatedUp;
        public bool showCalculatedForward;
        public bool showCalculatedRight;
        public bool showExpectedOrientation;
        public bool showStrafeInput;
        public bool showGroundedPosition;

        /// <summary>
        /// The bounds of the vehicle calculated every fixed frame
        /// </summary>
        private Bounds vehicleBounds;
        /// <summary>
        /// The up direction the pod should be
        /// </summary>
        private Vector3 up;
        /// <summary>
        /// The forward direction the pod should be
        /// </summary>
        private Vector3 forward;
        /// <summary>
        /// The right direction the pod should be
        /// </summary>
        private Vector3 right;
        /// <summary>
        /// The orientation the pod should be, calculated from the the up averaged from the surrounding face normals and the forward based on that up and the transform's forward
        /// </summary>
        private Quaternion castedOrientation;
        /// <summary>
        /// The position the pod would be if left motionless
        /// </summary>
        private Vector3 groundedPosition;
        /// <summary>
        /// Used in the pd controller of the floatation
        /// </summary>
        private Vector3 prevErr;

        void FixedUpdate()
        {
            vehicleBounds = transform.GetTotalBounds(Space.World);

            CalculateOrientationFromSurroundings();

            ApplyFloatation();
            ApplyOrientator();

            ApplyInputFly();
            ApplyInputStrafe();
            ApplyInputBrake();
            ApplyInputRotate();
        }
        void OnDrawGizmos()
        {
            if (showGroundedPosition)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(groundedPosition, 0.1f);
            }

            if (showExpectedOrientation && castedOrientation.IsValid())
            {
                float ocSize = 0.1f;
                Gizmos.matrix = Matrix4x4.TRS(vehicleBounds.center, castedOrientation, Vector3.one);
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(Vector3.forward * ocSize, Vector3.one * ocSize);
                Gizmos.color = Color.green;
                Gizmos.DrawCube(Vector3.up * ocSize, Vector3.one * ocSize);
                Gizmos.color = Color.red;
                Gizmos.DrawCube(Vector3.right * ocSize, Vector3.one * ocSize);
            }
        }

        private void CalculateOrientationFromSurroundings()
        {
            var rayResults = PhysicsHelpers.CastRays(vehicleBounds.center, showOrientationCasters, 15, 15, 10, groundMask);
            var rayHits = rayResults.Where(rayResult => rayResult.raycastHit);

            var nextUp = Vector3.up;
            if (rayHits.Count() > 0)
                nextUp = (rayHits.Select(rayInfo => rayInfo.hitData[0].normal).Aggregate((firstRay, secondRay) => firstRay + secondRay) / rayHits.Count()).normalized;
            up = Vector3.Lerp(up, nextUp, Time.fixedDeltaTime * 5);
            forward = Vector3.ProjectOnPlane(transform.forward, up).normalized;
            right = Quaternion.AngleAxis(90, up) * forward;
            castedOrientation = Quaternion.LookRotation(forward, up);

            if (showCalculatedUp)
                Debug.DrawRay(vehicleBounds.center, up * 5, Color.green);
            if (showCalculatedForward)
                Debug.DrawRay(vehicleBounds.center, forward * 5, Color.blue);
            if (showCalculatedRight)
                Debug.DrawRay(vehicleBounds.center, right * 5, Color.red);
        }

        private void ApplyInputFly()
        {
            if (Mathf.Abs(fly) > float.Epsilon)
            {
                float currentVelocity = Vector3.Dot(PodBody.velocity, up);
                float nextVelocity = Mathf.Clamp(currentVelocity + flyAcceleration * fly, -maxFlySpeed, maxFlySpeed);
                float flyForce = Vector3.Dot(PodBody.CalculateRequiredForceForSpeed(up * nextVelocity, 0.02f, true), up);
                PodBody.AddForce(up * flyForce, ForceMode.Force);
            }
        }
        private void ApplyInputStrafe()
        {
            if (!strafeStick.IsZero())
            {
                // var currentVelocity = new Vector3(PodBody.velocity.x, 0, PodBody.velocity.z);
                // var orthoForward = transform.forward.Planar(Vector3.up);
                var currentHorVelocity = Vector3.Dot(PodBody.velocity, right);
                var currentForVelocity = Vector3.Dot(PodBody.velocity, forward);
                var currentVelocity = currentHorVelocity * right + currentForVelocity * forward;
                var currentPushDirection = Quaternion.AngleAxis(Vector2.up.GetClockwiseAngle(strafeStick.normalized), up) * forward;
                float percentPower = strafeStick.magnitude;

                if (showStrafeInput)
                    Debug.DrawRay(vehicleBounds.center, currentPushDirection * percentPower * 5, Color.white);
                // float currentSpeed = currentVelocity.magnitude * currentVelocity.normalized.PercentDirection(currentPushDirection);
                float currentSpeed = Vector3.Dot(currentVelocity, currentPushDirection);
                float inputAcceleration = percentPower * acceleration;
                float desiredSpeed = Mathf.Clamp(currentSpeed + inputAcceleration, -maxSpeed, maxSpeed);
                Vector3 appliedForce = PhysicsHelpers.CalculateRequiredForceForSpeed(PodBody.mass, currentSpeed, desiredSpeed) * currentPushDirection;
                // appliedForce = new Vector3(appliedForce.x, 0, appliedForce.z);
                appliedForce -= up * Vector3.Dot(appliedForce, up);
                PodBody.AddForce(appliedForce, ForceMode.Force);
            }
        }
        private void ApplyInputBrake()
        {
            if (brake > float.Epsilon)
            {
                Vector3 brakeForce = PodBody.CalculateRequiredForceForSpeed(Vector3.zero, 0.02f, false, Mathf.Clamp01(brake) * maxBrakeForce);
                PodBody.AddForce(brakeForce, ForceMode.Force);
            }
        }
        private void ApplyInputRotate()
        {
            bool isAttemptingToRotate = Mathf.Abs(rotate) > float.Epsilon;
            if (isAttemptingToRotate)
            {
                float currentRotSpeed = (Vector3.Dot(PodBody.angularVelocity, up) * 180f) / Mathf.PI;
                if ((currentRotSpeed > -maxRotSpeed || rotate > 0) && (currentRotSpeed < maxRotSpeed || rotate < 0))
                    PodBody.AddTorque(up * PodBody.mass * rotAcceleration * Mathf.Clamp(rotate, -1, 1), ForceMode.Force);
            }
        }

        private void ApplyFloatation()
        {
            Vector3 expectedFloatingForce = CalculateFloatingForce();
                
            if (Mathf.Abs(fly) > float.Epsilon)
                expectedFloatingForce = Vector3.zero;

            PodBody.AddForce(expectedFloatingForce, ForceMode.Force);
        }
        private Vector3 CalculateFloatingForce()
        {
            float vehicleSizeOnUpAxis = Mathf.Abs(Vector3.Dot(vehicleBounds.extents, up));

            float groundCastDistance = vehicleSizeOnUpAxis + minGroundDistance * 5;
            RaycastHit hitInfo;
            float groundDistance = float.MaxValue;
            bool rayHitGround = Physics.Raycast(vehicleBounds.center, -up, out hitInfo, groundCastDistance, groundMask);
            if (rayHitGround)
                groundDistance = hitInfo.distance - vehicleSizeOnUpAxis;

            float groundOffset = minGroundDistance - groundDistance;

            float antigravityMultiplier = 1;
            if (groundOffset < -float.Epsilon)
                antigravityMultiplier = antigravityFalloffCurve.Evaluate(Mathf.Max(antigravityFalloffDistance - Mathf.Abs(groundOffset), 0) / antigravityFalloffDistance);
            Vector3 antigravityForce = PodBody.CalculateAntiGravityForce() * antigravityMultiplier;

            Vector3 floatingForce = Vector3.zero;
            if (groundDistance < float.MaxValue) //If the ground is within range
            {
                //Thanks to @bmabsout for a much better and more stable floatation method
                //based on pid but just the p and the d
                groundedPosition = hitInfo.point + up * minGroundDistance;
                Vector3 err = up * Vector3.Dot(up, groundedPosition - PodBody.position);
                Vector3 proportional = Kp * err;
                Vector3 derivative = Kd * (err - prevErr);
                floatingForce = proportional + derivative;
                prevErr = err;
            }

            return antigravityForce + floatingForce;
        }

        private void ApplyOrientator()
        {
            Vector3 correctionTorque = CalculateCorrectionTorque();
            PodBody.AddTorque(correctionTorque, ForceMode.Force);
        }
        private Vector3 CalculateCorrectionTorque()
        {
            var correctionTorque = PodBody.CalculateRequiredTorqueForRotation(castedOrientation, Time.fixedDeltaTime, maxCorrectionTorque);
            correctionTorque -= up * Vector3.Dot(correctionTorque, up);
            return correctionTorque;
        }
    }
}
