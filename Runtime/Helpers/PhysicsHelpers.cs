﻿using UnityEngine;

namespace UnityHelpers
{
    public static class PhysicsHelpers
    {
        /// <summary>
        /// Casts rays out from the transform's position in a spherical pattern
        /// </summary>
        /// <param name="transform">The object to cast rays from</param>
        /// <param name="debugRays">Show rays in editor</param>
        /// <param name="upAngle">The angle between each ray rotated around the up axis</param>
        /// <param name="rightAngle">The angle between each ray rotated around the right axis</param>
        /// <param name="distance">The distance to cast each ray</param>
        /// <param name="layerMask">The mask for each ray</param>
        /// <param name="space">Sets whether to cast along the world axes or the transform's axes</param>
        /// <param name="upCoverage">The total coverage of rays along the up axis</param>
        /// <param name="rightCoverage">The total coverage of rays along the right axis</param>
        /// <returns>The casted rays and their results</returns>
        public static RaycastInfo[] CastRays(this Transform transform, bool debugRays = false, float upAngle = 15, float rightAngle = 15, float distance = 10, int layerMask = ~0, Space space = Space.World, float upCoverage = 360, float rightCoverage = 360)
        {
            Vector3 up = Vector3.up, right = Vector3.right, forward = Vector3.forward;
            if (space == Space.Self)
            {
                up = transform.up;
                right = transform.right;
                forward = transform.forward;
            }

            return CastRays(transform.position, up, right, forward, debugRays, upAngle, rightAngle, distance, layerMask, upCoverage, rightCoverage);
        }
        /// <summary>
        /// Casts rays out from the given position in a spherical pattern using the world axes
        /// </summary>
        /// <param name="position">The position to cast rays from</param>
        /// <param name="debugRays">Show rays in editor</param>
        /// <param name="upAngle">The angle between each ray rotated around the up axis</param>
        /// <param name="rightAngle">The angle between each ray rotated around the right axis</param>
        /// <param name="distance">The distance to cast each ray</param>
        /// <param name="layerMask">The mask for each ray</param>
        /// <param name="upCoverage">The total coverage of rays along the up axis</param>
        /// <param name="rightCoverage">The total coverage of rays along the right axis</param>
        /// <returns>The casted rays and their results</returns>
        public static RaycastInfo[] CastRays(Vector3 position, bool debugRays = false, float upAngle = 15, float rightAngle = 15, float distance = 10, int layerMask = ~0, float upCoverage = 360, float rightCoverage = 360)
        {
            return CastRays(position, Vector3.up, Vector3.right, Vector3.forward, debugRays, upAngle, rightAngle, distance, layerMask, upCoverage, rightCoverage);
        }
        /// <summary>
        /// Casts rays out from the given position in a spherical pattern
        /// </summary>
        /// <param name="position">The position to cast rays from</param>
        /// <param name="up">An axis that the rays will rotate on</param>
        /// <param name="right">An axis that the rays will rotate on</param>
        /// <param name="forward">Used as a starting point for the rays</param>
        /// <param name="debugRays">Show rays in editor</param>
        /// <param name="upAngle">The angle between each ray rotated around the up axis</param>
        /// <param name="rightAngle">The angle between each ray rotated around the right axis</param>
        /// <param name="distance">The distance to cast each ray</param>
        /// <param name="layerMask">The mask for each ray</param>
        /// <param name="upCoverage">The total coverage of rays along the up axis</param>
        /// <param name="rightCoverage">The total coverage of rays along the right axis</param>
        /// <returns>The casted rays and their results</returns>
        public static RaycastInfo[] CastRays(Vector3 position, Vector3 up, Vector3 right, Vector3 forward, bool debugRays = false, float upAngle = 15, float rightAngle = 15, float distance = 10, int layerMask = ~0, float upCoverage = 360, float rightCoverage = 360)
        {
            int upHalfCount = (Mathf.CeilToInt(upCoverage / upAngle) - 1) / 2;
            int rightHalfCount = (Mathf.CeilToInt(rightCoverage / rightAngle) - 1) / 2;

            RaycastInfo[] rays = new RaycastInfo[upHalfCount * rightHalfCount * 4];
            for (int i = -upHalfCount; i < upHalfCount; i++)
            {
                for (int j = -rightHalfCount; j < rightHalfCount; j++)
                {
                    Vector3 currentDir = Quaternion.Euler(rightAngle * j, upAngle * i, 0) * forward;
                    
                    RaycastInfo currentRay = new RaycastInfo();
                    currentRay.position = position;
                    currentRay.direction = currentDir;
                    currentRay.distance = distance;
                    currentRay.castMask = layerMask;
                    currentRay.Cast();
                    rays[(i + upHalfCount) * rightHalfCount * 2 + (j + rightHalfCount)] = currentRay;

                    if (debugRays)
                        Debug.DrawRay(currentRay.position, currentRay.direction * distance, (currentRay.raycastHit ? Color.green : Color.red));
                }
            }

            return rays;
        }
        
        /// <summary>
        /// Gets the current angular velocity on the given axis in radians/s
        /// </summary>
        /// <param name="rigidbody">The rigidbody in question</param>
        /// <param name="axis">The axis to find the angular velocity on</param>
        /// <param name="normal">An axis that is perpendicular to the previous</param>
        /// <returns>The angular velocity in radians per second</returns>
        public static float GetAxisAngularVelocity(this Rigidbody rigidbody, Vector3 axis, Vector3 normal)
        {
            float rotDir = Mathf.Sign(Vector3.Dot(rigidbody.angularVelocity, axis));
            float currentAngVel = rotDir * Vector3.ProjectOnPlane(rigidbody.angularVelocity, normal).magnitude;
            return currentAngVel;
        }

        /// <summary>
        /// Rotates the rigidbody to a specified angle along an axis
        /// </summary>
        /// <param name="rigidbody">The object to rotate</param>
        /// <param name="axis">The axis the object will be rotating on</param>
        /// <param name="normal">An axis perpendicular to the rotation axis and the direction that represents the angle at 0</param>
        /// <param name="angle">The clockwise angle to rotate to in degrees (0 = forward, 90 = right, 180 = back, 270 = left)</param>
        /// <param name="acceleration">The angular acceleration the object is capable of (in radians/s^2)</param>
        /// <param name="maxSpeed">The max angular speed the object is capable of (in radians/s)</param>
        /// <param name="deceleration">The angular deceleration the object is capable of (in radians/s^2)</param>
        /// <param name="deltaTime">The time between calling this function</param>
        public static void RotateTo(this Rigidbody rigidbody, Vector3 axis, Vector3 normal, float angle, float acceleration, float maxSpeed, float deceleration, float deltaTime)
        {
            var currentAngVel = rigidbody.GetAxisAngularVelocity(axis, normal);
            var rotDir = Mathf.Sign(currentAngVel);
            var currentAngle = VectorHelpers.GetClockwiseAngle(normal, rigidbody.transform.forward, axis);
            if (Mathf.Abs(angle - currentAngle) > 180) //To avoid going the wrong way to reach an angle
                currentAngle += Mathf.Sign(angle - currentAngle) * 360;
            var angleTravelLeft = Mathf.Abs(angle - currentAngle);
            var dirToGo = Mathf.Sign(angle - currentAngle);

            //How much time would it take us to decelerate based on our current velocity
            float decelerationTime = Mathf.Abs(currentAngVel / deceleration);
            //How much time would it take us to reach our desired angle based on our current velocity
            float reachTime = Mathf.Abs((angleTravelLeft * Mathf.Deg2Rad) / currentAngVel);
            
            if (decelerationTime >= reachTime) //If the amount of time to decelerate is longer than the amount of time it would take our momentum to reach (if we're overshooting), then start decelerating
            {
                var expectedDec = (Mathf.Abs(currentAngVel) / deltaTime);
                var rotAccValue = -rotDir * Mathf.Min(deceleration, expectedDec);
                rigidbody.AddTorque(rotAccValue * axis, ForceMode.Acceleration);
            }
            else
            {
                var expectedAcc = (angleTravelLeft / (deltaTime * deltaTime)) * Mathf.Deg2Rad;
                var rotAccValue = (dirToGo * (Mathf.Sign(dirToGo) != rotDir ? deceleration : Mathf.Min(acceleration, expectedAcc))); //If we're currently rotating in the opposite direction of what we want, use the deceleration value instead of acceleration
                if (Mathf.Abs(currentAngVel + (rotAccValue * deltaTime)) >= maxSpeed)
                    rotAccValue = dirToGo * (maxSpeed - Mathf.Abs(currentAngVel));
                
                rigidbody.AddTorque(rotAccValue * axis, ForceMode.Acceleration);
            }
        }
        /// <summary>
        /// Rotates the rigidbody in a direction along an axis
        /// </summary>
        /// <param name="rigidbody">The object to rotate</param>
        /// <param name="axis">The axis to rotate along</param>
        /// <param name="normal">An axis perpendicular to the rotation axis</param>
        /// <param name="input">A value between -1 and 1 that represents whether to go left or right</param>
        /// <param name="acceleration">The speed with which the object can reach the max speed</param>
        /// <param name="maxSpeed">The max speed the object can rotate in</param>
        /// <param name="deceleration">The speed with which the object can stop rotating</param>
        public static void Rotate(this Rigidbody rigidbody, Vector3 axis, Vector3 normal, float input, float acceleration, float maxSpeed, float deceleration, float deltaTime)
        {
            var currentAngVel = rigidbody.GetAxisAngularVelocity(axis, normal);
            var rotDir = Mathf.Sign(currentAngVel);
            float rotAccValue = (input * (Mathf.Sign(input) != rotDir ? deceleration : acceleration)); //If we're currently rotating in the opposite direction of what we want, use the deceleration value instead of acceleration
            bool inputRot = Mathf.Abs(input) > float.Epsilon;
            if (inputRot && Mathf.Abs(currentAngVel + (rotAccValue * deltaTime)) >= maxSpeed)
                rotAccValue = Mathf.Sign(rotAccValue) * (maxSpeed - Mathf.Abs(currentAngVel));
            else if (!inputRot && Mathf.Abs(currentAngVel) >= (deceleration * deltaTime))
                rotAccValue = -rotDir * deceleration;
            else if (!inputRot && Mathf.Abs(currentAngVel) > Mathf.Epsilon)
                rotAccValue = -(currentAngVel / deltaTime);
            rigidbody.AddTorque(axis * rotAccValue, ForceMode.Acceleration);
        }
        /// <summary>
        /// <para>Source: https://digitalopus.ca/site/pd-controllers/ </para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Acceleration ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the torque will be applied to</param>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="frequency">Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.</param>
        /// <param name="damping"><para>damping = 1, the system is critically damped</para><para>damping is greater than 1 the system is over damped(sluggish)</para><para>damping is less than 1 the system is under damped(it will oscillate a little)</para></param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredTorque(this Rigidbody rigidbody, Quaternion desiredRotation, float frequency, float damping)
        {
            float kp = (6f * frequency) * (6f * frequency) * 0.25f;
            float kd = 4.5f * frequency * damping;
            float dt = Time.fixedDeltaTime;
            float g = 1 / (1 + kd * dt + kp * dt * dt);
            float ksg = kp * g;
            float kdg = (kd + kp * dt) * g;

            Vector3 x;
            float xMag;
            Quaternion q = desiredRotation * Quaternion.Inverse(rigidbody.transform.rotation);
            q = q.Shorten();
            q.ToAngleAxis(out xMag, out x);
            x.Normalize();

            x *= Mathf.Deg2Rad;
            Vector3 pidv = kp * x * xMag - kd * rigidbody.angularVelocity;
            Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * rigidbody.transform.rotation;
            pidv = Quaternion.Inverse(rotInertia2World) * pidv;
            pidv.Scale(rigidbody.inertiaTensor);
            pidv = rotInertia2World * pidv;
            return pidv;
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the angular acceleration required to achieve the desired rotation. Works with Acceleration ForceMode.</para>
        /// </summary>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="rotation">The rotation the rigidbody currently has</param>
        /// <param name="angularVelocity">The angular velocity the rigidbody currently has</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxAcceleration">The max acceleration the result can have.</param>
        /// <returns>The angular acceleration value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredAngularAccelerationForRotation(Quaternion desiredRotation, Quaternion rotation, Vector3 angularVelocity, float timestep = 0.02f, float maxAcceleration = float.MaxValue)
        {
            Vector3 axis;
            float angle;
            Quaternion rotDiff = desiredRotation * Quaternion.Inverse(rotation);
            rotDiff = rotDiff.Shorten();
            rotDiff.ToAngleAxis(out angle, out axis);
            axis.Normalize();

            angle *= Mathf.Deg2Rad;
            Vector3 desiredAngularAcceleration = (axis * angle) / (timestep * timestep);

            return (desiredAngularAcceleration - (angularVelocity / timestep)).MaxMag(maxAcceleration);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the torque will be applied to</param>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredTorqueForRotation(this Rigidbody rigidbody, Quaternion desiredRotation, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            return CalculateRequiredTorqueForRotation(desiredRotation, rigidbody.rotation, rigidbody.angularVelocity, rigidbody.inertiaTensor, rigidbody.inertiaTensorRotation, timestep, maxTorque);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="rotation">The rotation the rigidbody currently has</param>
        /// <param name="angularVelocity">The angular velocity the rigidbody currently has</param>
        /// <param name="inertiaTensor">The inertia tensor the rigidbody currently has</param>
        /// <param name="inertiaTensorRotation">The rotation inertia tensor the rigidbody currently has</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredTorqueForRotation(Quaternion desiredRotation, Quaternion rotation, Vector3 angularVelocity, Vector3 inertiaTensor, Quaternion inertiaTensorRotation, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            Vector3 axis;
            float angle;
            Quaternion rotDiff = desiredRotation * Quaternion.Inverse(rotation);
            rotDiff = rotDiff.Shorten();
            rotDiff.ToAngleAxis(out angle, out axis);
            axis.Normalize();

            angle *= Mathf.Deg2Rad;
            Vector3 desiredAngularVelocity = (axis * angle) / timestep;
            
            return CalculateTorqueForAngularVelocity(desiredAngularVelocity, rotation, angularVelocity, inertiaTensor, inertiaTensorRotation, timestep, maxTorque);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the angular velocity. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody the torque needs to be calculated for</param>
        /// <param name="desiredAngularVelocity">The angular velocity to be achieved</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateTorqueForAngularVelocity(this Rigidbody rigidbody, Vector3 desiredAngularVelocity, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            return CalculateTorqueForAngularVelocity(desiredAngularVelocity, rigidbody.rotation, rigidbody.angularVelocity, rigidbody.inertiaTensor, rigidbody.inertiaTensorRotation, timestep, maxTorque);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the angular velocity. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="desiredAngularVelocity">The angular velocity to be achieved</param>
        /// <param name="rotation">The rotation the rigidbody currently has</param>
        /// <param name="angularVelocity">The angular velocity the rigidbody currently has</param>
        /// <param name="inertiaTensor">The inertia tensor the rigidbody currently has</param>
        /// <param name="inertiaTensorRotation">The rotation inertia tensor the rigidbody currently has</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateTorqueForAngularVelocity(Vector3 desiredAngularVelocity, Quaternion rotation, Vector3 angularVelocity, Vector3 inertiaTensor, Quaternion inertiaTensorRotation, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            Quaternion q = rotation * inertiaTensorRotation;
            Vector3 T = q * Vector3.Scale(inertiaTensor, Quaternion.Inverse(q) * (desiredAngularVelocity / timestep));
            Vector3 prevT = q * Vector3.Scale(inertiaTensor, (Quaternion.Inverse(q) * (angularVelocity / timestep)));

            var deltaT = T - prevT;

            return deltaT.MaxMag(maxTorque);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/821833/calculating-the-rotational-acceleration-from-torqu.html</para>
        /// <para>Calculates the torque that would be as a result of the given acceleration</para>
        /// </summary>
        /// <param name="relativeTorque">The torque to be converted</param>
        /// <param name="inertiaTensor">The inertia tensor of the rigidbody</param>
        /// <param name="inertiaTensorRotation">The rotation inertia tensor of the rigidbody</param>
        /// <returns>The angular acceleration equivalent of the given torque</returns>
        public static Vector3 CalculateAngularAccelerationForTorque(Vector3 relativeTorque, Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
        {
            inertiaTensor = inertiaTensorRotation * inertiaTensor;

            float x = (relativeTorque.x / inertiaTensor.x);
            float y = (relativeTorque.y / inertiaTensor.y);
            float z = (relativeTorque.z / inertiaTensor.z);

            return new Vector3(x, y, z);
        }
        /// <summary>
        /// <para>Source: https://answers.unity.com/questions/48836/determining-the-torque-needed-to-rotate-an-object.html</para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Force ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the torque will be applied to</param>
        /// <param name="desiredAngle">The rotation that you'd like the rigidbody to have in degrees (0, 360] counter-clockwise</param>
        /// <param name="proportionalTerm">A coefficient applied to the desired force. Giving a value higher than 0.01 can cause trouble.</param>
        /// <param name="derivativeTerm">A coefficient applied to the current force. Giving a value higher than 0.01 can cause trouble.</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="maxTorque">The max torque the result can have.</param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static float CalculateRequiredTorqueForRotation(this Rigidbody2D rigidbody, float desiredAngle, float proportionalTerm = 0.01f, float derivativeTerm = 0.01f, float timestep = 0.02f, float maxTorque = float.MaxValue)
        {
            float currentAngle = rigidbody.rotation % 360;
            float expectedAngle = Mathf.Abs(desiredAngle) % 360;
            if (currentAngle < 0)
                currentAngle += 360;

            //Set up the rot diff to always be below 180 so that it is always the shortest route
            float rotDiff = expectedAngle - currentAngle;
            if (Mathf.Abs(rotDiff) > 180 && currentAngle > expectedAngle)
                rotDiff = expectedAngle - (currentAngle - 360);
            else if (Mathf.Abs(rotDiff) > 180 && expectedAngle > currentAngle)
                rotDiff = (expectedAngle - 360) - currentAngle;

            float desiredAngularAcceleration = rotDiff / (timestep * timestep);
            
            float proportionalTorque = proportionalTerm * rigidbody.inertia * desiredAngularAcceleration;
            float derivativeTorque = -derivativeTerm * rigidbody.inertia * (rigidbody.angularVelocity / timestep);

            var torque = proportionalTorque + derivativeTorque;
            if (Mathf.Abs(torque) > maxTorque)
                torque = Mathf.Sign(torque) * maxTorque;

            return torque;
        }
        /// <summary>
        /// Calculates the force that needs to be applied to an object with the given mass
        /// to counteract gravity's effect on it.
        /// </summary>
        /// <param name="mass">The mass of the object</param>
        /// <returns>The force value to be applied</returns>
        public static Vector3 CalculateAntiGravityForce(float mass)
        {
                return -Physics.gravity * mass;
        }
        /// <summary>
        /// Calculates the force that needs to be applied to a rigidbody to counteract
        /// gravity's effect on it.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to</param>
        /// <returns>The force value to be applied</returns>
        public static Vector3 CalculateAntiGravityForce(this Rigidbody rigidbody)
        {
                return CalculateAntiGravityForce(rigidbody.mass);
        }
        /// <summary>
        /// Calculates the force that needs to be applied to a rigidbody to counteract
        /// gravity's effect on it.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to</param>
        /// <returns>The force value to be applied</returns>
        public static Vector2 CalculateAntiGravityForce(this Rigidbody2D rigidbody)
        {
                return CalculateAntiGravityForce(rigidbody.mass);
        }
        /// <summary>
        /// Calculates the velocity vector required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the VelocityChange ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the velocity will be applied to.</param>
        /// <param name="desiredPosition">The position that you'd like the rigidbody to have.</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxSpeed">The max speed the result velocity can have.</param>
        /// <returns>The velocity value to  be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredVelocityForPosition(this Rigidbody rigidbody, Vector3 desiredPosition, float timestep = 0.02f, bool accountForGravity = false, float maxSpeed = float.MaxValue)
        {
            Vector3 nakedVelocity = desiredPosition - rigidbody.position;

            Vector3 gravityVelocity = Vector3.zero;
            if (accountForGravity)
                gravityVelocity = Physics.gravity * timestep * timestep; //Needs testing

            Vector3 deltaVelocity = nakedVelocity - gravityVelocity;

            return deltaVelocity.MaxMag(maxSpeed);
        }
        //Needs more work
        private static Vector3 CalculateRequiredVelocityForRotation(this Rigidbody rigidbody, Quaternion desiredRotation, float timestep = 0.02f, float maxSpeed = float.MaxValue)
        {
            Vector3 axis;
            float angle;
            Quaternion rotDiff = desiredRotation * Quaternion.Inverse(rigidbody.transform.rotation);
            rotDiff = rotDiff.Shorten();
            rotDiff.ToAngleAxis(out angle, out axis);
            axis.Normalize();

            angle *= Mathf.Deg2Rad;
            return (axis * angle) / (1);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the Force ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to.</param>
        /// <param name="desiredPosition">The position that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredForceForPosition(this Rigidbody rigidbody, Vector3 desiredPosition, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            return CalculateRequiredForceForPosition(desiredPosition, rigidbody.position, rigidbody.velocity, rigidbody.mass, timestep, accountForGravity, maxForce);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the Force ForceMode.
        /// </summary>
        /// <param name="desiredPosition">The position that you'd like the rigidbody to have.</param>
        /// <param name="currentPosition">The current position of the rigidbody that the force will be applied to.</param>
        /// <param name="currentVelocity">The current velocity of the rigidbody that the force will be applied to.</param>
        /// <param name="mass">The mass of the rigidbody that the force will be applied to.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredForceForPosition(Vector3 desiredPosition, Vector3 currentPosition, Vector3 currentVelocity, float mass, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            Vector3 nakedForce = (desiredPosition - currentPosition) / (timestep * timestep);
            nakedForce *= mass;

            Vector3 gravityForce = Vector3.zero;
            if (accountForGravity)
                gravityForce = CalculateAntiGravityForce(mass);

            Vector3 deltaForce = nakedForce - (((currentVelocity / timestep) * mass) + gravityForce);

            return deltaForce.MaxMag(maxForce);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the Force ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to.</param>
        /// <param name="desiredPosition">The position that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector2 CalculateRequiredForceForPosition(this Rigidbody2D rigidbody, Vector2 desiredPosition, float timestep = 0.02f, float maxForce = float.MaxValue)
        {
            Vector2 nakedForce = (desiredPosition - rigidbody.position) / (timestep * timestep);
            nakedForce *= rigidbody.mass;

            Vector2 deltaForce = nakedForce - ((rigidbody.velocity / timestep) * rigidbody.mass);

            return deltaForce.MaxMag(maxForce);
        }

        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired speed. Works with the Force ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to.</param>
        /// <param name="desiredVelocity">The velocity that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredForceForSpeed(this Rigidbody rigidbody, Vector3 desiredVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            return CalculateRequiredForceForSpeed(rigidbody.mass, rigidbody.velocity, desiredVelocity, timestep, accountForGravity, maxForce);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired speed. Works with the Force ForceMode.
        /// </summary>
        /// <param name="mass">The mass of the rigidbody.</param>
        /// <param name="velocity">The velocity of the rigidbody.</param>
        /// <param name="desiredVelocity">The velocity that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredForceForSpeed(float mass, Vector3 velocity, Vector3 desiredVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            Vector3 nakedForce = desiredVelocity / timestep;
            nakedForce *= mass;

            Vector3 currentForce = (velocity / timestep) * mass;

            Vector3 gravityForce = Vector3.zero;
            if (accountForGravity)
                gravityForce = CalculateAntiGravityForce(mass);

            Vector3 deltaForce = nakedForce - (currentForce + gravityForce);

            return deltaForce.MaxMag(maxForce);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired speed. Works with the Force ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to.</param>
        /// <param name="desiredVelocity">The velocity that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector2 CalculateRequiredForceForSpeed(this Rigidbody2D rigidbody, Vector2 desiredVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            return CalculateRequiredForceForSpeed(rigidbody.mass, rigidbody.velocity, desiredVelocity, timestep, accountForGravity, maxForce);
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired speed. Works with the Force ForceMode.
        /// </summary>
        /// <param name="mass">The mass of the rigidbody.</param>
        /// <param name="velocity">The velocity of the rigidbody.</param>
        /// <param name="desiredVelocity">The velocity that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector2 CalculateRequiredForceForSpeed(float mass, Vector2 velocity, Vector2 desiredVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            Vector2 nakedForce = desiredVelocity / timestep;
            nakedForce *= mass;

            Vector2 currentForce = (velocity / timestep) * mass;

            Vector2 gravityForce = Vector3.zero;
            if (accountForGravity)
                gravityForce = Physics.gravity * mass;

            Vector2 deltaForce = nakedForce - (currentForce + gravityForce);

            return deltaForce.MaxMag(maxForce);
        }

        /// <summary>
        /// Calculates the force value required to be applied to a rigidbody through AddForce to achieve the desired speed. Works with the Force ForceMode.
        /// </summary>
        /// <param name="mass">The mass of the rigidbody.</param>
        /// <param name="velocity">The velocity of the rigidbody.</param>
        /// <param name="desiredVelocity">The velocity that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static float CalculateRequiredForceForSpeed(float mass, float velocity, float desiredVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxForce = float.MaxValue)
        {
            float nakedForce = desiredVelocity / timestep;
            nakedForce *= mass;

            float currentForce = (velocity / timestep) * mass;

            float gravityForce = 0;
            if (accountForGravity)
                gravityForce = -Physics.gravity.magnitude * mass;

            float deltaForce = nakedForce - (currentForce + gravityForce);

            if (deltaForce > maxForce)
                deltaForce = maxForce;

            return deltaForce;
        }
        /// <summary>
        /// Calculates the velocity value required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the VelocityChange ForceMode.
        /// </summary>
        /// <param name="changeInPosition">The amount of distance to traverse within the given time</param>
        /// <param name="timestep">Time to achieve change in position.</param>
        /// <param name="accountForGravity">Oppose gravity force?</param>
        /// <param name="maxSpeed">The max speed the result velocity can have.</param>
        /// <returns>The velocity value to  be applied to the rigidbody.</returns>
        public static float CalculateRequiredVelocityForPosition(float changeInPosition, float currentVelocity, float timestep = 0.02f, bool accountForGravity = false, float maxSpeed = float.MaxValue)
        {
            float nakedVelocity = changeInPosition / timestep;

            float gravityVelocity = 0;
            if (accountForGravity)
                gravityVelocity = -Physics.gravity.magnitude * timestep;

            float deltaVelocity = nakedVelocity - (currentVelocity + gravityVelocity);

            if (deltaVelocity > maxSpeed)
                deltaVelocity = maxSpeed;

            return deltaVelocity;
        }

        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, float groundDistance = 0.1f, bool useColliders = false)
        {
            return physicsBody.IsGrounded(Physics.gravity.normalized, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, int layerMask, float groundDistance = 0.1f, bool useColliders = false)
        {
            return physicsBody.IsGrounded(Physics.gravity.normalized, layerMask, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, Vector3 groundDirection, float groundDistance = 0.1f, bool useColliders = false)
        {
            return physicsBody.transform.IsGrounded(groundDirection, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, Vector3 groundDirection, int layerMask, float groundDistance = 0.1f, bool useColliders = false)
        {
            return physicsBody.transform.IsGrounded(groundDirection, layerMask, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, float groundDistance = 0.1f, bool useColliders = false)
        {
            return root.IsGrounded(Physics.gravity.normalized, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, int layerMask, float groundDistance = 0.1f, bool useColliders = false)
        {
            return root.IsGrounded(Physics.gravity.normalized, layerMask, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, Vector3 groundDirection, float groundDistance = 0.1f, bool useColliders = false)
        {
            return root.IsGrounded(groundDirection, ~0, groundDistance, useColliders);
        }
        /// <summary>
        /// Checks if the transforn is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <param name="useColliders">If set to true, will use the bounds of the colliders rather than renderers to check groundedness</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, Vector3 groundDirection, int layerMask, float groundDistance = 0.1f, bool useColliders = false)
        {
            return root.RaycastFromBounds(groundDirection, layerMask, Vector3.down * 0.9f, true, groundDistance, useColliders);
        }
        /// <summary>
        /// Raycasts from the center of the object's bounds plus bounds extents based on percentExtents
        /// </summary>
        /// <param name="root">The object's transform</param>
        /// <param name="rayDirection">The direction the ray will be cast in</param>
        /// <param name="percentExtents">How far from the center to the extents to start ray casting from (axes in root's local space)</param>
        /// <param name="totalBounds">If set to true, will use childrens' bounds as well rather than just the current object's bounds</param>
        /// <param name="maxDistance">The max distance the ray cast should be</param>
        /// <param name="useColliders">Whether the bounds should be calculated from the colliders or from the renderers</param>
        /// <returns>True if an object was hit, false otherwise</returns>
        public static bool RaycastFromBounds(this Transform root, Vector3 rayDirection, Vector3 percentExtents, bool totalBounds = true, float maxDistance = 0.1f, bool useColliders = false)
        {
            RaycastHit hitInfo;
            return root.RaycastFromBounds(rayDirection, ~0, out hitInfo, percentExtents, totalBounds, maxDistance, useColliders);
        }
        /// <summary>
        /// Raycasts from the center of the object's bounds plus bounds extents based on percentExtents
        /// </summary>
        /// <param name="root">The object's transform</param>
        /// <param name="rayDirection">The direction the ray will be cast in</param>
        /// <param name="hitInfo">The returned info of the ray cast</param>
        /// <param name="percentExtents">How far from the center to the extents to start ray casting from (axes in root's local space)</param>
        /// <param name="totalBounds">If set to true, will use childrens' bounds as well rather than just the current object's bounds</param>
        /// <param name="maxDistance">The max distance the ray cast should be</param>
        /// <param name="useColliders">Whether the bounds should be calculated from the colliders or from the renderers</param>
        /// <returns>True if an object was hit, false otherwise</returns>
        public static bool RaycastFromBounds(this Transform root, Vector3 rayDirection, out RaycastHit hitInfo, Vector3 percentExtents, bool totalBounds = true, float maxDistance = 0.1f, bool useColliders = false)
        {
            return root.RaycastFromBounds(rayDirection, ~0, out hitInfo, percentExtents, totalBounds, maxDistance, useColliders);
        }
        /// <summary>
        /// Raycasts from the center of the object's bounds plus bounds extents based on percentExtents
        /// </summary>
        /// <param name="root">The object's transform</param>
        /// <param name="rayDirection">The direction the ray will be cast in</param>
        /// <param name="layerMask">Which layers to whitelist in the ray cast</param>
        /// <param name="percentExtents">How far from the center to the extents to start ray casting from (axes in root's local space)</param>
        /// <param name="totalBounds">If set to true, will use childrens' bounds as well rather than just the current object's bounds</param>
        /// <param name="maxDistance">The max distance the ray cast should be</param>
        /// <param name="useColliders">Whether the bounds should be calculated from the colliders or from the renderers</param>
        /// <returns>True if an object was hit, false otherwise</returns>
        public static bool RaycastFromBounds(this Transform root, Vector3 rayDirection, int layerMask, Vector3 percentExtents, bool totalBounds = true, float maxDistance = 0.1f, bool useColliders = false)
        {
            RaycastHit hitInfo;
            return root.RaycastFromBounds(rayDirection, layerMask, out hitInfo, percentExtents, totalBounds, maxDistance, useColliders);
        }
        /// <summary>
        /// Raycasts from the center of the object's bounds plus bounds extents based on percentExtents
        /// </summary>
        /// <param name="root">The object's transform</param>
        /// <param name="rayDirection">The direction the ray will be cast in</param>
        /// <param name="layerMask">Which layers to whitelist in the ray cast</param>
        /// <param name="hitInfo">The returned info of the ray cast</param>
        /// <param name="percentExtents">How far from the center to the extents to start ray casting from (axes in root's local space)</param>
        /// <param name="totalBounds">If set to true, will use childrens' bounds as well rather than just the current object's bounds</param>
        /// <param name="maxDistance">The max distance the ray cast should be</param>
        /// <param name="useColliders">Whether the bounds should be calculated from the colliders or from the renderers</param>
        /// <returns>True if an object was hit, false otherwise</returns>
        public static bool RaycastFromBounds(this Transform root, Vector3 rayDirection, int layerMask, out RaycastHit hitInfo, Vector3 percentExtents, bool totalBounds = true, float maxDistance = 0.1f, bool useColliders = false)
        {
            Vector3 checkPosition = root.GetPointInBounds(percentExtents, totalBounds, useColliders);
            bool castHit = Physics.Raycast(checkPosition, rayDirection, out hitInfo, maxDistance, layerMask);
            //Debug.DrawLine(root.TransformPoint(bounds.center), checkPosition, Color.blue);
            //Debug.DrawRay(checkPosition, rayDirection * Mathf.Min(maxDistance, 100f), castHit ? Color.green : Color.red);
            return castHit;
        }
        /// <summary>
        /// Calculates a world space point in the bounds of the given transform
        /// </summary>
        /// <param name="root">The object's transform</param>
        /// <param name="percentExtents">How far the point should be from the center to the extents of the bounds (axes in root's local space)</param>
        /// <param name="totalBounds">If set to true, will use childrens' bounds as well rather than just the current object's bounds</param>
        /// <param name="useColliders">Whether the bounds should be calculated from the colliders or from the renderers</param>
        /// <returns>A point within the bounds of the object</returns>
        public static Vector3 GetPointInBounds(this Transform root, Vector3 percentExtents, bool totalBounds = true, bool useColliders = false)
        {
            Bounds bounds;
            if (totalBounds)
                bounds = root.GetTotalBounds(Space.Self, useColliders);
            else
                bounds = root.GetBounds(Space.Self, useColliders);
                
            Vector3 extentsOffset = Vector3.right * bounds.extents.x * percentExtents.x + Vector3.up * bounds.extents.y * percentExtents.y + Vector3.forward * bounds.extents.z * percentExtents.z;
            Vector3 boundsPoint = root.TransformPoint(bounds.center + extentsOffset);
            return boundsPoint;
        }

        /// <summary>
        /// Predicts where the rigidbody will be in the next physics timestep
        /// </summary>
        /// <param name="rigidbody">The rigidbody whose position is to be predicted</param>
        /// <returns>The predicted position of the rigidbody</returns>
        public static Vector3 PredictPosition(this Rigidbody rigidbody)
        {
            return PredictPosition(rigidbody, Time.fixedDeltaTime);
        }
        /// <summary>
        /// Predicts where the rigidbody will be after some time
        /// </summary>
        /// <param name="rigidbody">The rigidbody whose position is to be predicted</param>
        /// <param name="time">How far in the future to predict</param>
        /// <returns>The predicted position of the rigidbody</returns>
        public static Vector3 PredictPosition(this Rigidbody rigidbody, float time)
        {
            return PredictPosition(rigidbody.position, rigidbody.velocity, rigidbody.drag, time, rigidbody.useGravity);
        }
        /// <summary>
        /// Predicts where an object will be after some time
        /// </summary>
        /// <param name="position">The current position of the object</param>
        /// <param name="velocity">The current velocity of the object</param>
        /// <param name="drag">The drag on the object</param>
        /// <param name="time">How far in the future to predict</param>
        /// <param name="useGravity">Should gravity be taken into consideration</param>
        /// <returns>The predicted position of the object</returns>
        public static Vector3 PredictPosition(Vector3 position, Vector3 velocity, float drag, float time, bool useGravity = true)
        {
            float appliedDrag = Mathf.Clamp01(1f - drag * time);
            Vector3 gravityEffect = Vector3.zero;
            if (useGravity)
                gravityEffect = Physics.gravity * time * 0.5f;
            Vector3 nextVelocity = (velocity + gravityEffect) * appliedDrag;
            return position + nextVelocity * time;
        }
    }
}