using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// A ball that can be rolled around using torque
    /// </summary>
    public class BallPhysics : ValuedObject
    {
        public Rigidbody ballBody;

        /// <summary>
        /// How fast the ball speeds up in rotations/second^2
        /// </summary>
        [Tooltip("How fast the ball speeds up in rotations/second^2")]
        public float rotAcceleration = 6;
        /// <summary>
        /// How fast the ball can go in rotations/second
        /// </summary>
        [Tooltip("How fast the ball can go in rotations/second")]
        public float maxRotSpeed = 3;

        /// <summary>
        /// The world space forward of the ball's drive
        /// </summary>
        [Tooltip("The world space forward of the ball's drive")]
        public Vector3 forward = Vector3.forward;
        /// <summary>
        /// The world space up of the ball (used to calculate the torque direction)
        /// </summary>
        [Tooltip("The world space up of the ball (used to calculate the torque direction)")]
        public Vector3 up = Vector3.up;

        void FixedUpdate()
        {
            AdjustInput();
            AccelerateBall();
        }

        private void AdjustInput()
        {
            SetAxis("Horizontal", Mathf.Clamp(GetAxis("Horizontal"), -1, 1));
            SetAxis("Vertical", Mathf.Clamp(GetAxis("Vertical"), -1, 1));
        }
        private void AccelerateBall()
        {        
            float horizontal = GetAxis("Horizontal");
            float vertical = GetAxis("Vertical");
            //Turns the given 2D square input into a circular input
            Vector2 circularInput = new Vector2(horizontal, vertical).ToCircle();
            //Turns the input into a Vector3 direction
            Vector3 worldDirection = new Vector3(circularInput.x, 0, circularInput.y);

            //Get the angle offset of the input direction from world forward
            float upAngle = Vector3.SignedAngle(Vector3.forward, worldDirection, Vector3.up);
            //Apply the same angle offset but to the given ball forward and make sure the input magnitude is the same
            Vector3 adjustedWorldDirection = Quaternion.AngleAxis(upAngle, up) * forward * worldDirection.magnitude;

            //Cross the forward axis with the up axis to get the right axis which is the axis torque will be applied on
            Vector3 torqueDirection = Vector3.Cross(up, adjustedWorldDirection);
            //Calculate the angular acceleration which is radians per second squared
            Vector3 angularAcceleration = torqueDirection * (rotAcceleration * 2 * Mathf.PI);

            //Convert maxRotSpeed (rotations per second) to maxAngularSpeed (radians per second)
            float maxAngularSpeed = maxRotSpeed * 2 * Mathf.PI;
            //Calculate the expected angular velocity in the next frame
            Vector3 nextAngularVelocity = ballBody.angularVelocity + angularAcceleration * Time.fixedDeltaTime;
            //If the next expected angular speed due to input acceleration is greater than max angular speed
            if (nextAngularVelocity.magnitude > maxAngularSpeed)
            {
                //Calculate max angular velocity based on direction and speed
                Vector3 maxAngularVelocity = nextAngularVelocity.normalized * maxAngularSpeed;
                //Get difference between max angular velocity and current angular velocity
                Vector3 deltaAngularVelocity = maxAngularVelocity - ballBody.angularVelocity;
                //Set current angular acceleration to fit difference between max and current angular velocity
                angularAcceleration = deltaAngularVelocity / Time.fixedDeltaTime;
            }

            ballBody.AddTorque(angularAcceleration, ForceMode.Acceleration);
        }
    }
}