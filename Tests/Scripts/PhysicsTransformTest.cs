using UnityEngine;

namespace UnityHelpers
{
    public class PhysicsTransformTest : MonoBehaviour
    {
        public PhysicsTransform physicsTransform;
        public Transform target;

        void Update()
        {
            physicsTransform.position = target.position;
            physicsTransform.rotation = target.rotation;
        }
    }
}