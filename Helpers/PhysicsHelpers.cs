using UnityEngine;

namespace UnityHelpers
{
    public static class PhysicsHelpers
    {
        /// <summary>
        /// <para>Source: https://digitalopus.ca/site/pd-controllers/ </para>
        /// <para>Calculates the torque required to be applied to a rigidbody to achieve the desired rotation. Works with Acceleration ForceMode.</para>
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the torque will be applied to</param>
        /// <param name="desiredRotation">The rotation that you'd like the rigidbody to have</param>
        /// <param name="frequency">Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.</param>
        /// <param name="damping"><para>damping = 1, the system is critically damped</para><para>damping is greater than 1 the system is over damped(sluggish)</para><para>damping is less than 1 the system is under damped(it will oscillate a little)</para></param>
        /// <returns>The torque value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredTorque(this Rigidbody rigidbody, Quaternion desiredRotation, float frequency = 6, float damping = 1)
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
            q.ToAngleAxis(out xMag, out x);
            x.Normalize();

            float expectedAngle = Quaternion.Angle(desiredRotation, rigidbody.transform.rotation); //Calculate the actual angle between the quaternions
            if (Mathf.Abs(xMag - expectedAngle) > 1f) //If the angle from ToAngleAxis is larger than the actual angle
            {
                xMag = expectedAngle; //Set the angle to the actual angle
                x = (-x).normalized; //Invert the axis
            }

            x *= Mathf.Deg2Rad;
            Vector3 pidv = kp * x * xMag - kd * rigidbody.angularVelocity;
            Quaternion rotInertia2World = rigidbody.inertiaTensorRotation * rigidbody.transform.rotation;
            pidv = Quaternion.Inverse(rotInertia2World) * pidv;
            pidv.Scale(rigidbody.inertiaTensor);
            pidv = rotInertia2World * pidv;
            return pidv;
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
            Vector3 nakedVelocity = (desiredPosition - rigidbody.position) / timestep;
            if (nakedVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                nakedVelocity = nakedVelocity.normalized * maxSpeed;

            Vector3 gravityVelocity = Vector3.zero;
            if (accountForGravity)
                gravityVelocity = Physics.gravity * timestep;

            Vector3 deltaVelocity = nakedVelocity - (rigidbody.velocity + gravityVelocity);

            return deltaVelocity;
        }
        /// <summary>
        /// Calculates the force vector required to be applied to a rigidbody through AddForce to achieve the desired position. Works with the Force ForceMode.
        /// </summary>
        /// <param name="rigidbody">The rigidbody that the force will be applied to.</param>
        /// <param name="desiredPosition">The position that you'd like the rigidbody to have.</param>
        /// <param name="timestep">The delta time between frames.</param>
        /// <param name="maxForce">The max force the result can have.</param>
        /// <returns>The force value to be applied to the rigidbody.</returns>
        public static Vector3 CalculateRequiredForceForPosition(this Rigidbody rigidbody, Vector3 desiredPosition, float timestep = 0.02f, float maxForce = float.MaxValue)
        {
            Vector3 nakedForce = (desiredPosition - rigidbody.position) / (timestep * timestep);
            nakedForce *= rigidbody.mass;
            if (nakedForce.sqrMagnitude > maxForce * maxForce)
                nakedForce = nakedForce.normalized * maxForce;

            Vector3 deltaForce = nakedForce - (rigidbody.velocity / timestep * rigidbody.mass);
            return deltaForce;
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
            if (nakedForce.sqrMagnitude > maxForce * maxForce)
                nakedForce = nakedForce.normalized * maxForce;

            Vector3 currentForce = (velocity / timestep * mass);

            Vector3 gravityForce = Vector3.zero;
            if (accountForGravity)
                gravityForce = Physics.gravity * mass;

            Vector3 deltaForce = nakedForce - (currentForce + gravityForce);
            return deltaForce;
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
            if (nakedForce > maxForce)
                nakedForce = maxForce;

            float currentForce = (velocity / timestep * mass);

            float gravityForce = 0;
            if (accountForGravity)
                gravityForce = -Physics.gravity.magnitude * mass;

            float deltaForce = nakedForce - (currentForce + gravityForce);
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
            if (nakedVelocity > maxSpeed)
                nakedVelocity = maxSpeed;

            float gravityVelocity = 0;
            if (accountForGravity)
                gravityVelocity = -Physics.gravity.magnitude * timestep;

            float deltaVelocity = nakedVelocity - (currentVelocity + gravityVelocity);

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