using UnityEngine;

namespace UnityHelpers
{
    public static class JointHelpers
    {
        /// <summary>
        /// Sets the target rotation of the configurable joint to be the given rotation relative to the original rotation
        /// </summary>
        /// <param name="joint">The joint whose target rotation is to be set</param>
        /// <param name="currentRotation">The orientation you would like the joint to be in</param>
        /// <param name="originalRotation">The original orientation of the joint</param>
        public static void SetTargetRotation(this ConfigurableJoint joint, Quaternion currentRotation, Quaternion originalRotation)
        {
            joint.targetRotation = Quaternion.identity * (originalRotation * Quaternion.Inverse(currentRotation));
        }
    }
}