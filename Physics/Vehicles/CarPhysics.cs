using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This script has been tested and works well with NWH's WheelController. Make sure to set the center of mass to be closer to the ground so the car doesn't keep flipping.
    /// </summary>
    public class CarPhysics : MonoBehaviour
    {
        public Rigidbody vehicleRigidbody;

        public Transform wheelFL, wheelFR;
        public Transform wheelRL, wheelRR;
        [Tooltip("This value sets how far from the center of the rear wheels to check for the ground. If the ground is not being touched, the car won't accelerate.")]
        public float wheelGroundDistance = 1;

        [Space(10)]
        public CarStats vehicleStats;

        public float currentSpeed { get; private set; }
        private float strivedSpeed;

        [Space(10), Range(-1, 1)]
        public float gas;
        [Range(0, 1)]
        public float brake;
        [Range(-1, 1)]
        public float steer;

        void FixedUpdate()
        {
            Quaternion wheelRotation = Quaternion.Euler(0, vehicleStats.maxWheelAngle * steer, 0);
            wheelFL.localRotation = wheelRotation;
            wheelFR.localRotation = wheelRotation;

            float forwardPercent = -(Vector3.Angle(transform.forward, vehicleRigidbody.velocity.normalized) / 90 - 1);
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

                float differenceInSpeed = Mathf.Abs(Mathf.Abs(strivedSpeed) - Mathf.Abs(currentSpeed)); //This is used to
                float speedRatio = Mathf.Abs(currentSpeed) / Mathf.Abs(strivedSpeed);                   //reset the strivedSpeed
                if (differenceInSpeed > speedRatio * vehicleStats.maxReverseSpeed)                      //value when it's too
                    strivedSpeed = currentSpeed;                                                        //far from actual speed

                strivedSpeed = Mathf.Clamp(strivedSpeed + deltaSpeed, -vehicleStats.maxReverseSpeed, vehicleStats.maxForwardSpeed);

                Vector3 nonForwardVelocity = vehicleRigidbody.velocity - currentSpeed * transform.forward;
                vehicleRigidbody.AddForce(vehicleRigidbody.CalculateRequiredForceForSpeed(strivedSpeed * transform.forward + (currentSpeed > 0.01f ? nonForwardVelocity : Vector3.zero)), ForceMode.Force);
            }
        }
    }
}