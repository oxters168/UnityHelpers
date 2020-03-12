﻿using MIConvexHull;
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

        public bool castRays;
        private RaycastHitInfo[] forwardRayResults, leftRayResults, rightRayResults, rearRayResults;
        //private Vector3 prevVelocity;

        private void Awake()
        {
            vehicleBounds = vehicleRigidbody.transform.GetTotalBounds(false, false, true);
        }
        private void Update()
        {
            if (castRays)
                DetectCollision();
        }
        void FixedUpdate()
        {
            Quaternion wheelRotation = Quaternion.Euler(0, vehicleStats.maxWheelAngle * steer, 0);
            wheelFL.localRotation = wheelRotation;
            wheelFR.localRotation = wheelRotation;

            float forwardPercent = vehicleRigidbody.velocity.PercentDirection(vehicleRigidbody.transform.forward);
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

                vehicleRigidbody.AddForce(PhysicsHelpers.CalculateRequiredForceForSpeed(vehicleRigidbody.mass, currentSpeed * vehicleRigidbody.transform.forward, strivedSpeed * vehicleRigidbody.transform.forward), ForceMode.Force);
            }

            prevCurrentSpeed = currentSpeed;
            prevStrivedSpeed = strivedSpeed;
            //prevVelocity = vehicleRigidbody.velocity;
        }

        #region Collsion Detection
        private void DetectCollision()
        {
            int forwardRayCount = MathHelpers.GetOddNumber((int)vehicleStats.forwardRays);
            if (forwardRayResults == null || forwardRayResults.Length != forwardRayCount)
                forwardRayResults = new RaycastHitInfo[forwardRayCount];

            int leftRayCount = MathHelpers.GetOddNumber((int)vehicleStats.leftRays);
            if (leftRayResults == null || leftRayResults.Length != leftRayCount)
                leftRayResults = new RaycastHitInfo[leftRayCount];

            int rightRayCount = MathHelpers.GetOddNumber((int)vehicleStats.rightRays);
            if (rightRayResults == null || rightRayResults.Length != rightRayCount)
                rightRayResults = new RaycastHitInfo[rightRayCount];

            int rearRayCount = MathHelpers.GetOddNumber((int)vehicleStats.rearRays);
            if (rearRayResults == null || rearRayResults.Length != rearRayCount)
                rearRayResults = new RaycastHitInfo[rearRayCount];

            CastRays(forwardRayResults, vehicleStats.forwardDistanceObstacleCheck, vehicleRigidbody.transform.forward, 0, 1);
            CastRays(leftRayResults, vehicleStats.leftDistanceObstacleCheck, -vehicleRigidbody.transform.right, -1, 0);
            CastRays(rightRayResults, vehicleStats.rightDistanceObstacleCheck, vehicleRigidbody.transform.right, 1, 0);
            CastRays(rearRayResults, vehicleStats.rearDistanceObstacleCheck, -vehicleRigidbody.transform.forward, 0, -1);
        }
        /// <summary>
        /// Casts rays in a direction and outputs the results to the given array.
        /// </summary>
        /// <param name="rayResults">The results of the casts</param>
        /// <param name="distanceObstacleCheck">How far to send out the rays</param>
        /// <param name="rayDirection">The direction the rays shoot</param>
        /// <param name="xBorder">Where on the vehicle border in the x direction to shoot rays from (-1 .. 1)</param>
        /// <param name="zBorder">Where on the vehicle border in the z direction to shoot rays from (-1 .. 1)</param>
        private void CastRays(RaycastHitInfo[] rayResults, float distanceObstacleCheck, Vector3 rayDirection, float xBorder, float zBorder)
        {
            float extentPercent = 0.9f;
            xBorder = Mathf.Clamp(xBorder, -extentPercent, extentPercent);
            zBorder = Mathf.Clamp(zBorder, -extentPercent, extentPercent);

            Vector3 vehicleRayStart;
            int rayCount = rayResults.Length;
            int extents = rayCount / 2;
            float step = 1f / rayCount;
            RaycastHit rayhitInfo;
            for (int i = 0; i < rayCount; i++)
            {
                int offsetIndex = i - extents;
                float currentOffset = extents != 0 ? (step * offsetIndex) / (step * extents) : 0;
                vehicleRayStart = GetPointOnBoundsBorder((Mathf.Abs(zBorder) > Mathf.Epsilon ? extentPercent : 0) * currentOffset + xBorder, -0.5f, (Mathf.Abs(xBorder) > Mathf.Epsilon ? extentPercent : 0) * currentOffset + zBorder);

                bool rayhit = Physics.Raycast(vehicleRayStart, rayDirection, out rayhitInfo, distanceObstacleCheck);
                rayResults[i] = new RaycastHitInfo() { hit = rayhit, info = rayhitInfo, rayStart = vehicleRayStart, rayStartDirection = rayDirection, rayMaxDistance = distanceObstacleCheck };

                Debug.DrawRay(vehicleRayStart, rayDirection * (rayhit ? rayhitInfo.distance : distanceObstacleCheck), rayhit ? Color.green : Color.red);
            }
        }

        private static RaycastHitInfo GetClosestHitInfo(RaycastHitInfo[] directionRayResults)
        {
            RaycastHitInfo bestRay = default;
            if (directionRayResults != null)
            {
                float closestRay = float.MaxValue;
                for (int i = 0; i < directionRayResults.Length; i++)
                {
                    var currentRay = directionRayResults[i];
                    if (currentRay.hit && currentRay.info.distance < closestRay)
                        bestRay = currentRay;
                }
            }
            return bestRay;
        }
        /// <summary>
        /// Gets the closest raycast hit info that was hit. If no rays were hit, then returns the default value.
        /// </summary>
        /// <returns>Raycast hit info.</returns>
        public RaycastHitInfo GetForwardHitInfo()
        {
            return GetClosestHitInfo(forwardRayResults);
        }
        /// <summary>
        /// Gets the closest raycast hit info that was hit. If no rays were hit, then returns the default value.
        /// </summary>
        /// <returns>Raycast hit info.</returns>
        public RaycastHitInfo GetLeftHitInfo()
        {
            return GetClosestHitInfo(leftRayResults);
        }
        /// <summary>
        /// Gets the closest raycast hit info that was hit. If no rays were hit, then returns the default value.
        /// </summary>
        /// <returns>Raycast hit info.</returns>
        public RaycastHitInfo GetRightHitInfo()
        {
            return GetClosestHitInfo(rightRayResults);
        }
        /// <summary>
        /// Gets the closest raycast hit info that was hit. If no rays were hit, then returns the default value.
        /// </summary>
        /// <returns>Raycast hit info.</returns>
        public RaycastHitInfo GetRearHitInfo()
        {
            return GetClosestHitInfo(rearRayResults);
        }
        /// <summary>
        /// Gives the angle between the raycast direction and the direction of the hit on the car's up axis.
        /// </summary>
        /// <param name="raycastInfo">The info of the raycast.</param>
        /// <returns>The signed angle between the two directions.</returns>
        public float GetHitAngle(RaycastHitInfo raycastInfo)
        {
            return vehicleRigidbody.position.SignedAngle(raycastInfo.info.point, raycastInfo.rayStartDirection, vehicleRigidbody.transform.up);
        }
        #endregion

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

                Teleport(other.vehicleRigidbody.position, other.vehicleRigidbody.rotation, other.currentSpeed);
                vehicleRigidbody.angularVelocity = other.vehicleRigidbody.angularVelocity;
            }
        }
        public void SetVisible(bool onOff)
        {
            vehicleRigidbody.gameObject.SetActive(onOff);
        }
        public void Teleport(Vector3 position, Quaternion rotation, float speed = 0)
        {
            SetStrivedSpeed(speed);
            currentSpeed = strivedSpeed;

            vehicleRigidbody.transform.position = position;
            vehicleRigidbody.transform.rotation = rotation;
            vehicleRigidbody.velocity = vehicleRigidbody.transform.forward * currentSpeed;
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
            Vector3 borderPercentOffset = vehicleRigidbody.transform.right * vehicleBounds.extents.x * percentX + vehicleRigidbody.transform.up * vehicleBounds.extents.y * percentY + vehicleRigidbody.transform.forward * vehicleBounds.extents.z * percentZ;
            return vehicleRigidbody.position + borderPercentOffset + vehicleRigidbody.transform.up * vehicleBounds.extents.y;
        }
    }

    public struct RaycastHitInfo
    {
        public bool hit;
        public Vector3 rayStart, rayStartDirection;
        public float rayMaxDistance;
        public RaycastHit info;
    }
}