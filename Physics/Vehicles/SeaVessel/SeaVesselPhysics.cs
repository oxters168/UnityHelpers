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

        // [Space(10)]
        // public ValuesVault values;

        void Update()
        {
            //Retrieve input
            dpad = new Vector2(GetAxis("dpadHor"), GetAxis("dpadVer"));
            crossBtn = GetToggle("crossBtn");
        }
        void FixedUpdate()
        {
            if (crossBtn)
            {
                Vector3 planarForward = vesselBody.transform.forward.Planar(Vector3.up);
                Vector3 currentVelocity = vesselBody.velocity.Planar(Vector3.up);
                float currentSpeed = currentVelocity.magnitude * Vector3.Dot(currentVelocity.normalized, planarForward);
                currentSpeed = Mathf.Clamp(currentSpeed + acceleration, -maxSpeed, maxSpeed);
                Vector3 pushForce = vesselBody.CalculateRequiredForceForSpeed(currentSpeed * planarForward, Time.fixedDeltaTime, false, maxForce);
                Debug.DrawRay(vesselBody.position + Vector3.up * 20, planarForward * 10, Color.blue);
                Debug.DrawRay(vesselBody.position + Vector3.up * 20, currentVelocity, Color.green);
                Debug.Log(pushForce.magnitude + " => " + currentSpeed);
                vesselBody.AddForce(pushForce, ForceMode.Force);
            }


            Vector3 torque = Vector3.zero;
            if (Mathf.Abs(dpad.x) > float.Epsilon || Mathf.Abs(dpad.y) > float.Epsilon)
            {
                float requestedAngle = Vector2.SignedAngle(dpad.ToCircle(), Vector2.up);

                float currentAngle = Vector2.SignedAngle(vesselBody.transform.forward.Planar(Vector3.up).xz(), Vector2.up);
                Quaternion currentUpOrientation = Quaternion.AngleAxis(currentAngle, Vector3.up);
                Quaternion requestedUpOrientation = Quaternion.AngleAxis(requestedAngle, Vector3.up);
                Quaternion orientationDiff = requestedUpOrientation * Quaternion.Inverse(currentUpOrientation);
                orientationDiff = orientationDiff.Shorten();
                float angleDiff;
                Vector3 axis;
                orientationDiff.ToAngleAxis(out angleDiff, out axis);

                // float expectedAcc = angleDiff / (Time.fixedDeltaTime * Time.fixedDeltaTime);
                float rotDirection = Mathf.Sign(Vector3.Dot(Vector3.up, axis));
                if (Mathf.Abs(currentRotSpeed) > angleDiff)
                    currentRotSpeed = angleDiff;
                // if (rotAcceleration > expectedAcc)
                //     currentRotSpeed = Mathf.Clamp(currentRotSpeed - rotAcceleration * rotDirection, -maxRotSpeed, maxRotSpeed);
                else
                    currentRotSpeed = Mathf.Clamp(currentRotSpeed + rotAcceleration * rotDirection, -maxRotSpeed, maxRotSpeed);
            }
            else if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                float rotDirection = Mathf.Sign(currentRotSpeed);
                if (Mathf.Abs(currentRotSpeed) > rotAcceleration)
                    currentRotSpeed += rotAcceleration * -rotDirection;
                else
                    currentRotSpeed = 0;
            }

            if (Mathf.Abs(currentRotSpeed) > float.Epsilon)
            {
                Quaternion shipOrientation = vesselBody.rotation * Quaternion.AngleAxis(currentRotSpeed, Vector3.up);
                torque = vesselBody.CalculateRequiredTorqueForRotation(shipOrientation, Time.fixedDeltaTime, maxTorque);
                // Debug.Log(torque.magnitude);
                vesselBody.AddTorque(torque, ForceMode.Force);
            }
        }
    }
}