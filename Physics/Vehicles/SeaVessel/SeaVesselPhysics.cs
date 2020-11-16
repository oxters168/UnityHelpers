using UnityEngine;

namespace UnityHelpers
{
    public class SeaVesselPhysics : ValuedObject
    {
        public Rigidbody vesselBody;

        [Space(10), Tooltip("The maximum speed the ship can reach (meters per second)")]
        public float maxSpeed = 10;
        [Tooltip("How fast to reach the max speed (meters per second squared)")]
        public float acceleration = 1f;
        [Tooltip("The maximum force that can be applied to the ship to achieve its strived speed")]
        public float maxForce = 140f;
        [Tooltip("How fast to reach the max rotation speed (degrees per second squared)")]
        public float rotAcceleration = 0.01f;
        [Tooltip("The maximum speed the ship can rotate (degrees per second)")]
        public float maxRotSpeed = 0.5f;
        [Tooltip("The maximum torque that can be applied to the ship to achieve its strived orientation")]
        public float maxTorque = 120f;
        private float currentRotSpeed = 0;

        private Vector2 dpad;
        private bool crossBtn;

        void Update()
        {
            //Retrieve input
            dpad = new Vector2(GetAxis("dpadHor"), GetAxis("dpadVer"));
            crossBtn = GetToggle("crossBtn");
        }
        void FixedUpdate()
        {
            #region Position stuff
            if (crossBtn)
            {
                Vector3 planarForward = vesselBody.transform.forward.Planar(Vector3.up);
                Vector3 currentVelocity = vesselBody.velocity.Planar(Vector3.up);
                float currentSpeed = currentVelocity.magnitude * Vector3.Dot(currentVelocity.normalized, planarForward);
                currentSpeed = Mathf.Clamp(currentSpeed + acceleration, -maxSpeed, maxSpeed);
                Vector3 pushForce = vesselBody.CalculateRequiredForceForSpeed(currentSpeed * planarForward, Time.fixedDeltaTime, false, maxForce);
                vesselBody.AddForce(pushForce, ForceMode.Force);
            }
            #endregion

            #region Rotation stuff
            if (Mathf.Abs(dpad.x) > float.Epsilon || Mathf.Abs(dpad.y) > float.Epsilon)
            {
                float requestedAngle = Vector2.SignedAngle(dpad.ToCircle(), Vector2.up);
                float currentAngle = Vector2.SignedAngle(vesselBody.transform.forward.Planar(Vector3.up).xz(), Vector2.up);
                Quaternion nextUpOrientation = Quaternion.AngleAxis(currentAngle, Vector3.up);
                nextUpOrientation *= Quaternion.AngleAxis(currentRotSpeed, Vector3.up);
                Quaternion requestedUpOrientation = Quaternion.AngleAxis(requestedAngle, Vector3.up);
                Quaternion orientationDiff = requestedUpOrientation * Quaternion.Inverse(nextUpOrientation);
                orientationDiff = orientationDiff.Shorten();
                float nextAngleDiff;
                Vector3 axis;
                orientationDiff.ToAngleAxis(out nextAngleDiff, out axis);
                float requestedRotDirection = Mathf.Sign(Vector3.Dot(Vector3.up, axis));

                //Given our current rotational speed, how far would we rotate if we started decelerating now?
                float decelerationTime = Mathf.Abs(currentRotSpeed) / rotAcceleration;
                float decelerationDistance = Mathf.Abs((currentRotSpeed + rotAcceleration * decelerationTime * Mathf.Sign(currentRotSpeed)) * decelerationTime);
                
                //If we're rotating towards our target and we're going to overshoot then start decelerating
                if (requestedRotDirection == Mathf.Sign(currentRotSpeed) && decelerationDistance > nextAngleDiff)
                {
                    if (Mathf.Abs(currentRotSpeed) < rotAcceleration)
                        currentRotSpeed = 0;
                    else
                        currentRotSpeed = Mathf.Clamp(currentRotSpeed - rotAcceleration * requestedRotDirection, -maxRotSpeed, maxRotSpeed);
                }
                else
                    currentRotSpeed = Mathf.Clamp(currentRotSpeed + rotAcceleration * requestedRotDirection, -maxRotSpeed, maxRotSpeed);
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
                Quaternion shipOrientation = vesselBody.rotation * Quaternion.AngleAxis(currentRotSpeed, Vector3.up);
                Vector3 torque = vesselBody.CalculateRequiredTorqueForRotation(shipOrientation, Time.fixedDeltaTime, maxTorque);
                vesselBody.AddTorque(torque, ForceMode.Force);
            }
            #endregion
        }
    }
}