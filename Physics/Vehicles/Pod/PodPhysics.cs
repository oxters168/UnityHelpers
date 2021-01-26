using UnityEngine;

namespace UnityHelpers
{
    public class PodPhysics : MonoBehaviour
    {
        private Rigidbody _podBody;
        private Rigidbody PodBody { get { if (_podBody == null) _podBody = GetComponentInChildren<Rigidbody>(); return _podBody; } }

        public Vector2 strafeStick;
        public float brake;
        public float rotate;
        public float fly;

        [Space(10), Tooltip("The speed of the speed (in m/s^2)")]
        public float acceleration = 0.2f;
        [Tooltip("The fastest speed the vehicle can achieve (in m/s)")]
        public float maxSpeed = 20;

        [Tooltip("The speed of the fly speed (in m/s^2)")]
        public float flyAcceleration = 1;
        [Tooltip("The fastest the vehicle can go up")]
        public float maxFlySpeed = 10;

        [Tooltip("The speed of the rotational speed (in deg/s^2)")]
        public float rotAcceleration = 4;
        [Tooltip("The fastest the vehicle can rotate (in deg/s)")]
        public float maxRotSpeed = 100;

        [Space(10), Tooltip("The max force applied to stop the vehicle (in newtons")]
        public float maxBrakeForce = 16000;

        [Space(10), Tooltip("The minimum distance the pod keeps itself floating above the ground (in meters)")]
        public float minGroundDistance = 1;

        [Space(10), Tooltip("How fast the force that floats the pod builds up (in newtons per fixed update)")]
        public float floatingForceSpeed = 8600;
        [Tooltip("How much distance beyond the minimum ground height before anti-gravity wears off")]
        public float antigravityFalloffDistance = 20;
        public AnimationCurve antigravityFalloffCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Space(10), Tooltip("The maximum torque the pod can apply to fix its orientation (in newton meters)")]
        public float maxCorrectionTorque = float.MaxValue;

        [Space(10), Tooltip("The layer(s) to be raycasted when looking for the ground")]
        public LayerMask groundMask = ~0;

        public float prevFloatingForce = 0;
        public float floatingForce, antigravityForce;
        public float currentRotSpeed;

        void FixedUpdate()
        {
            ApplyFloatation();
            ApplyOrientator();

            ApplyInputFly();
            ApplyInputStrafe();
            ApplyInputBrake();
            ApplyInputRotate();
        }

        private void ApplyInputFly()
        {
            float currentVelocity = PodBody.velocity.y;
            float nextVelocity = currentVelocity + flyAcceleration * fly;
            float flyForce = PodBody.CalculateRequiredForceForSpeed(Vector3.up * nextVelocity, 0.02f, true).y;
            PodBody.AddForce(Vector3.up * flyForce, ForceMode.Force);
        }
        private void ApplyInputStrafe()
        {
            if (!strafeStick.IsZero())
            {
                var currentVelocity = new Vector3(PodBody.velocity.x, 0, PodBody.velocity.z);
                var orthoForward = transform.forward.Planar(Vector3.up);
                var currentPushDirection = Quaternion.AngleAxis(Vector2.up.GetClockwiseAngle(strafeStick.normalized), Vector3.up) * orthoForward;
                float currentSpeed = currentVelocity.magnitude * currentVelocity.normalized.PercentDirection(currentPushDirection);
                float percentPower = strafeStick.magnitude;
                float inputAcceleration = percentPower * acceleration;
                float desiredSpeed = Mathf.Clamp(currentSpeed + inputAcceleration, -maxSpeed, maxSpeed);
                Vector3 appliedForce = PhysicsHelpers.CalculateRequiredForceForSpeed(PodBody.mass, currentSpeed, desiredSpeed) * currentPushDirection;
                appliedForce = new Vector3(appliedForce.x, 0, appliedForce.z);
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
                currentRotSpeed = (PodBody.angularVelocity.y * 180f) / Mathf.PI;
                currentRotSpeed = Mathf.Clamp(currentRotSpeed + rotAcceleration * Mathf.Clamp(rotate, -1, 1), -maxRotSpeed, maxRotSpeed);
            }
            else if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                //If there is no input but there is still some rotational speed, then decelerate to zero speed
                float rotDirection = Mathf.Sign(currentRotSpeed);
                if (Mathf.Abs(currentRotSpeed) > rotAcceleration)
                    currentRotSpeed += rotAcceleration * -rotDirection;
                else
                    currentRotSpeed = 0;
            }

            //If the current rot speed is not zero then apply torque on the ship
            if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                Quaternion resultRotation = PodBody.rotation * Quaternion.AngleAxis(currentRotSpeed * Time.fixedDeltaTime, Vector3.up);
                Vector3 torque = Vector3.up * PodBody.CalculateRequiredTorqueForRotation(resultRotation, Time.fixedDeltaTime).y;
                PodBody.AddTorque(torque, ForceMode.Force);
            }
        }

        private void ApplyFloatation()
        {
            float expectedFloatingForce = CalculateFloatingForce();
            float deltaFloatingForce = expectedFloatingForce - prevFloatingForce;
            float currentFloatingForce = prevFloatingForce;
            //If there is a measurable change in force
            if (Mathf.Abs(deltaFloatingForce) > float.Epsilon)
            {
                //If force is increasing in magnitude
                // if ((currentFloatingForce > float.Epsilon && Mathf.Sign(deltaFloatingForce) > 0 || currentFloatingForce < float.Epsilon && Mathf.Sign(deltaFloatingForce) < 0))
                    currentFloatingForce += Mathf.Sign(deltaFloatingForce) * Mathf.Min(Mathf.Abs(deltaFloatingForce), floatingForceSpeed);
                // else
                //     currentFloatingForce = expectedFloatingForce;
            }

            if (Mathf.Abs(fly) > float.Epsilon)
                currentFloatingForce = 0;

            PodBody.AddForce(Vector3.up * currentFloatingForce, ForceMode.Force);
            prevFloatingForce = currentFloatingForce;
        }
        private float CalculateFloatingForce()
        {
            var vehicleBounds = transform.GetTotalBounds(Space.World);
            float groundCastDistance = vehicleBounds.extents.y + minGroundDistance * 5;
            RaycastHit hitInfo;
            float groundDistance = float.MaxValue;
            bool rayHitGround = Physics.Raycast(vehicleBounds.center, Vector3.down, out hitInfo, groundCastDistance, groundMask);
            if (rayHitGround)
                groundDistance = hitInfo.distance - vehicleBounds.extents.y;
            Debug.DrawRay(vehicleBounds.center + (rayHitGround ? Vector3.down * vehicleBounds.extents.y : Vector3.zero), Vector3.down * (rayHitGround ? groundDistance : groundCastDistance), rayHitGround ? Color.green : Color.red);

            float groundOffset = minGroundDistance - groundDistance;

            float antigravityMultiplier = 1;
            if (groundOffset < -float.Epsilon)
                antigravityMultiplier = antigravityFalloffCurve.Evaluate(Mathf.Max(antigravityFalloffDistance - Mathf.Abs(groundOffset), 0) / antigravityFalloffDistance);
            antigravityForce = PodBody.CalculateAntiGravityForce().y * antigravityMultiplier;

            floatingForce = 0;
            if (groundDistance < float.MaxValue)
            {
                float currentVerticalForce = PodBody.mass * PodBody.velocity.y / Time.fixedDeltaTime;
                floatingForce = antigravityMultiplier * PodBody.CalculateRequiredForceForSpeed(new Vector3(0, groundOffset, 0), Time.fixedDeltaTime).y - currentVerticalForce;
            }

            return (antigravityForce + floatingForce) - prevFloatingForce;
        }

        private void ApplyOrientator()
        {
            Vector3 correctionTorque = CalculateCorrectionTorque();
            PodBody.AddTorque(correctionTorque, ForceMode.Force);
        }
        private Vector3 CalculateCorrectionTorque()
        {
            Vector3 correctionTorque;
            Quaternion correctedOrientation = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);
            correctionTorque = PodBody.CalculateRequiredTorqueForRotation(correctedOrientation, Time.fixedDeltaTime, maxCorrectionTorque);
            correctionTorque = new Vector3(correctionTorque.x, 0, correctionTorque.z);
            return correctionTorque;
        }
    }
}
