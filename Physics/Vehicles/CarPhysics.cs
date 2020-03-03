using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This script has been tested and works well with NWH's WheelController. Make sure to set the center of mass to be closer to the ground so the car doesn't keep flipping.
    /// </summary>
    public class CarPhysics : MonoBehaviour
    {
        public Rigidbody vehicleRigidbody;
        private Bounds vehicleBounds;

        public Transform wheelFL, wheelFR;
        public Transform wheelRL, wheelRR;
        [Tooltip("This value sets how far from the center of the rear wheels to check for the ground. If the ground is not being touched, the car won't accelerate.")]
        public float wheelGroundDistance = 1;

        [Space(10)]
        public CarStats vehicleStats;

        public float currentSpeed { get; private set; }
        private float strivedSpeed;
        private float prevCurrentSpeed, prevStrivedSpeed;

        [Space(10), Range(-1, 1)]
        public float gas;
        [Range(0, 1)]
        public float brake;
        [Range(-1, 1)]
        public float steer;

        //private Vector3 prevVelocity;

        private void Start()
        {
            vehicleBounds = transform.GetTotalBounds(false);
        }
        void FixedUpdate()
        {
            Quaternion wheelRotation = Quaternion.Euler(0, vehicleStats.maxWheelAngle * steer, 0);
            wheelFL.localRotation = wheelRotation;
            wheelFR.localRotation = wheelRotation;

            float forwardPercent = vehicleRigidbody.velocity.PercentDirection(transform.forward);
            currentSpeed = vehicleRigidbody.velocity.magnitude * forwardPercent;

            if (wheelRL.IsGrounded(-wheelRL.up, wheelGroundDistance) || wheelRR.IsGrounded(-wheelRR.up, wheelGroundDistance))
            {
                gas = Mathf.Clamp(gas, -1, 1);
                brake = Mathf.Clamp(brake, 0, 1);
                steer = Mathf.Clamp(steer, -1, 1);

                float gasAmount = gas * (vehicleStats.acceleration + (gas > 0 && currentSpeed < 0 || gas < 0 && currentSpeed > 0 ? vehicleStats.brakeleration : 0));
                float brakeAmount = brake * vehicleStats.brakeleration * (currentSpeed >= 0 ? -1 : 1);
                float totalAcceleration = gasAmount + brakeAmount;
                if (totalAcceleration > -float.Epsilon && totalAcceleration < float.Epsilon && !(strivedSpeed > -float.Epsilon && strivedSpeed < float.Epsilon))
                    totalAcceleration = vehicleStats.deceleration * (currentSpeed >= 0 ? -1 : 1);
                float deltaSpeed = totalAcceleration * Time.fixedDeltaTime;

                SetStrivedSpeed(strivedSpeed + deltaSpeed);

                float nextCurrentSpeed = currentSpeed + deltaSpeed;
                //If strived speed is changing differently from current speed then set strived speed to current speed
                if (Mathf.Sign(strivedSpeed - prevStrivedSpeed) != Mathf.Sign(nextCurrentSpeed - prevCurrentSpeed))
                    SetStrivedSpeed(nextCurrentSpeed);

                vehicleRigidbody.AddForce(PhysicsHelpers.CalculateRequiredForceForSpeed(vehicleRigidbody.mass, currentSpeed * transform.forward, strivedSpeed * transform.forward), ForceMode.Force);
            }

            prevCurrentSpeed = currentSpeed;
            prevStrivedSpeed = strivedSpeed;
            //prevVelocity = vehicleRigidbody.velocity;
        }

        private void SetStrivedSpeed(float value)
        {
            strivedSpeed = Mathf.Clamp(value, -vehicleStats.maxReverseSpeed, vehicleStats.maxForwardSpeed);
        }

        public void Match(CarPhysics other)
        {
            if (other != null)
            {
                gas = other.gas;
                brake = other.brake;
                steer = other.steer;

                Teleport(other.transform.position, other.transform.rotation, other.currentSpeed);
                vehicleRigidbody.angularVelocity = other.vehicleRigidbody.angularVelocity;
            }
        }
        public void Teleport(Vector3 position, Quaternion rotation, float speed = 0)
        {
            SetStrivedSpeed(speed);
            currentSpeed = strivedSpeed;

            transform.position = position;
            transform.rotation = rotation;
            vehicleRigidbody.velocity = transform.forward * currentSpeed;
            vehicleRigidbody.angularVelocity = Vector3.zero;
        }

        /// <summary>
        /// Gets a point in the bounds of the car. This assumes the car's pivot is low down near the ground.
        /// </summary>
        /// <param name="percentX">A value between -1 and 1 where 0 would mean at the center of the bounds.</param>
        /// <param name="percentY">A value between -1 and 1 where 0 would mean at the center of the bounds.</param>
        /// <param name="percentZ">A value between -1 and 1 where 0 would mean at the center of the bounds.</param>
        /// <returns>A point within the bounds.</returns>
        public Vector3 GetPointOnBoundsBorder(float percentX, float percentY, float percentZ)
        {
            percentX = Mathf.Clamp(percentX, -1, 1);
            percentY = Mathf.Clamp(percentY, -1, 1);
            percentZ = Mathf.Clamp(percentZ, -1, 1);
            Vector3 borderPercentOffset = transform.right * vehicleBounds.extents.x * percentX + transform.up * vehicleBounds.extents.y * percentY + transform.forward * vehicleBounds.extents.z * percentZ;
            return transform.position + borderPercentOffset + transform.up * vehicleBounds.extents.y;
        }
    }
}