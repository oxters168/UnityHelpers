using UnityEngine;

namespace UnityHelpers
{
    public class SeaVesselPhysics : ValuedObject
    {
        public Rigidbody vesselBody;

        [Space(10), Tooltip("The maximum speed the ship can reach (meters per second)")]
        public float maxSpeed = 10;
        [Tooltip("How fast to reach the max speed (meters per second squared)")]
        public float acceleration = 5f;
        private float currentSpeed;
        [Tooltip("The maximum force that can be applied to the ship to achieve its strived speed")]
        public float maxForce = 280000f;
        [Tooltip("How fast to reach the max rotation speed (degrees per second squared)")]
        public float rotAcceleration = 50f;
        [Tooltip("The maximum speed the ship can rotate (degrees per second)")]
        public float maxRotSpeed = 100f;
        [Tooltip("The maximum torque that can be applied to the ship to achieve its strived orientation")]
        public float maxTorque = 280000f;
        private float currentRotSpeed = 0;

        private Vector2 dpad;
        private bool crossBtn;

        void Update()
        {
            //Retrieve input
            dpad = new Vector2(GetAxis("horizontal"), GetAxis("vertical"));
            crossBtn = GetToggle("crossBtn");
        }
        void FixedUpdate()
        {
            #region Position stuff
            if (crossBtn)
            {
                currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.fixedDeltaTime, -maxSpeed, maxSpeed);
            }
            else if (currentSpeed > float.Epsilon)
            {
                if (currentSpeed > acceleration * Time.fixedDeltaTime)
                    currentSpeed -= acceleration * Time.fixedDeltaTime;
                else
                    currentSpeed = 0;
            }

            if (currentSpeed > float.Epsilon)
            {
                Vector3 planarForward = Vector3.ProjectOnPlane(vesselBody.transform.forward, Vector3.up);
                Vector3 pushForce = vesselBody.CalculateRequiredForceForSpeed(currentSpeed * planarForward, Time.fixedDeltaTime, false, maxForce);
                vesselBody.AddForce(pushForce, ForceMode.Force);
            }
            #endregion

            #region Rotation stuff
            float rotSpeedOffset = rotAcceleration * Time.fixedDeltaTime; //The amount of rotational speed change in one frame
            if (Mathf.Abs(dpad.x) > float.Epsilon || Mathf.Abs(dpad.y) > float.Epsilon)
            {
                float angleDiff = vesselBody.transform.forward.Planar(Vector3.up).xz().normalized.GetShortestSignedAngle(dpad.normalized);
                float requestedRotDirection = Mathf.Sign(angleDiff);

                //Given our current rotational speed, how far would we rotate if we started decelerating now?
                float decelerationTime = Mathf.Abs(currentRotSpeed) / rotAcceleration;
                float decelerationDistance = Mathf.Abs((currentRotSpeed + rotAcceleration * decelerationTime * Mathf.Sign(currentRotSpeed)) * decelerationTime);
                
                //If we're rotating towards our target and we're going to overshoot then start decelerating
                if (requestedRotDirection == Mathf.Sign(currentRotSpeed) && decelerationDistance > Mathf.Abs(angleDiff))
                {
                    if (Mathf.Abs(currentRotSpeed) < rotSpeedOffset)
                        currentRotSpeed = 0;
                    else
                        currentRotSpeed = Mathf.Clamp(currentRotSpeed - rotSpeedOffset * requestedRotDirection, -maxRotSpeed, maxRotSpeed);
                }
                else
                    currentRotSpeed = Mathf.Clamp(currentRotSpeed + rotSpeedOffset * requestedRotDirection, -maxRotSpeed, maxRotSpeed);
            }
            else if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                //If there is no input but there is still some rotational speed, then decelerate to zero speed
                float rotDirection = Mathf.Sign(currentRotSpeed);
                if (Mathf.Abs(currentRotSpeed) > rotSpeedOffset)
                    currentRotSpeed += rotSpeedOffset * -rotDirection;
                else
                    currentRotSpeed = 0;
            }

            //If the current rot speed is not zero then apply torque on the ship
            if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                Quaternion shipOrientation = vesselBody.rotation * Quaternion.AngleAxis(currentRotSpeed * Time.fixedDeltaTime, Vector3.up);
                Vector3 torque = vesselBody.CalculateRequiredTorqueForRotation(shipOrientation, Time.fixedDeltaTime, maxTorque);
                vesselBody.AddTorque(torque, ForceMode.Force);
            }
            #endregion
        }
    }
}