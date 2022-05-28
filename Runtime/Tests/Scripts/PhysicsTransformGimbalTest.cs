using UnityEngine;

namespace UnityHelpers.Tests
{
    public class PhysicsTransformGimbalTest : MonoBehaviour
    {
        public PhysicsTransform objectInQuestion;
        public Vector3 axis;
        public float degreesPerSecond = 60;

        void FixedUpdate()
        {
            axis = axis.normalized;
            objectInQuestion.rotation = Quaternion.AngleAxis(degreesPerSecond * Time.fixedDeltaTime, transform.TransformDirection(axis)) * objectInQuestion.rotation;
        }
    }
}