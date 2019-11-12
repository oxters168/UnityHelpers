using UnityEngine;

namespace UnityHelpers
{
    public static class PhysicsHelpers
    {
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, float groundDistance = 0.1f)
        {
            Vector3 physicsDown = Physics.gravity.normalized;

            float heightExtent = BoundsHelpers.GetTotalBounds(physicsBody.transform).extents.y;
            Vector3 checkPosition = physicsBody.position + physicsDown * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, physicsDown, groundDistance);
            Debug.DrawRay(checkPosition, physicsDown * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, int layerMask, float groundDistance = 0.1f)
        {
            Vector3 physicsDown = Physics.gravity.normalized;

            float heightExtent = BoundsHelpers.GetTotalBounds(physicsBody.transform).extents.y;
            Vector3 checkPosition = physicsBody.position + physicsDown * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, physicsDown, groundDistance, layerMask);
            Debug.DrawRay(checkPosition, physicsDown * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, Vector3 groundDirection, float groundDistance = 0.1f)
        {
            float heightExtent = BoundsHelpers.GetTotalBounds(physicsBody.transform).extents.y;
            Vector3 checkPosition = physicsBody.position + groundDirection * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, groundDirection, groundDistance);
            Debug.DrawRay(checkPosition, groundDirection * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
        /// <summary>
        /// Checks if the rigidbody is grounded.
        /// </summary>
        /// <param name="physicsBody">The rigidbody to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Rigidbody physicsBody, Vector3 groundDirection, int layerMask, float groundDistance = 0.1f)
        {
            float heightExtent = BoundsHelpers.GetTotalBounds(physicsBody.transform).extents.y;
            Vector3 checkPosition = physicsBody.position + groundDirection * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, groundDirection, groundDistance, layerMask);
            Debug.DrawRay(checkPosition, groundDirection * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
    }
}