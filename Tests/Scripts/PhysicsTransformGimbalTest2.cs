using UnityEngine;

namespace UnityHelpers.Tests
{
    public class PhysicsTransformGimbalTest2 : MonoBehaviour
    {
        public Transform toBeMimicked;
        public float lerpAmount = 5;

        void Update()
        {
            Quaternion mimickedRotation = toBeMimicked.parent.TransformRotation(toBeMimicked.rotation);
            //Debug.Log(Quaternion.Angle(mimickedRotation, transform.rotation));
            transform.rotation = Quaternion.Lerp(transform.rotation, mimickedRotation, Time.deltaTime * lerpAmount);
        }
    }
}