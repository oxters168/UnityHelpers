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
            return physicsBody.IsGrounded(Physics.gravity.normalized, groundDistance);
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
            return physicsBody.IsGrounded(Physics.gravity.normalized, layerMask, groundDistance);
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
            return physicsBody.transform.IsGrounded(groundDirection, groundDistance);
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
            return physicsBody.transform.IsGrounded(groundDirection, layerMask, groundDistance);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, float groundDistance = 0.1f)
        {
            return root.IsGrounded(Physics.gravity.normalized, groundDistance);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, int layerMask, float groundDistance = 0.1f)
        {
            return root.IsGrounded(Physics.gravity.normalized, layerMask, groundDistance);
        }
        /// <summary>
        /// Checks if the transform is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, Vector3 groundDirection, float groundDistance = 0.1f)
        {
            float heightExtent = BoundsHelpers.GetTotalBounds(root).extents.y;
            Vector3 checkPosition = root.position + groundDirection * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, groundDirection, groundDistance);
            Debug.DrawRay(checkPosition, groundDirection * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
        /// <summary>
        /// Checks if the transforn is grounded.
        /// </summary>
        /// <param name="root">The root transform to be checked</param>
        /// <param name="groundDirection">The direction of the ground</param>
        /// <param name="layerMask">The ground layer mask</param>
        /// <param name="groundDistance">The distance from the bottom of the bounds to the ground to be considered grounded</param>
        /// <returns>True if the rigidbody is grounded false otherwise</returns>
        public static bool IsGrounded(this Transform root, Vector3 groundDirection, int layerMask, float groundDistance = 0.1f)
        {
            float heightExtent = BoundsHelpers.GetTotalBounds(root).extents.y;
            Vector3 checkPosition = root.position + groundDirection * heightExtent + Vector3.up * groundDistance / 2;
            bool grounded = Physics.Raycast(checkPosition, groundDirection, groundDistance, layerMask);
            Debug.DrawRay(checkPosition, groundDirection * groundDistance, grounded ? Color.green : Color.red);
            return grounded;
        }
    }
}