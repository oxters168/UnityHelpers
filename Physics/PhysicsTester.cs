using UnityEngine;

namespace UnityHelpers
{
    public class PhysicsTester : MonoBehaviour
    {
        private Rigidbody body;

        public bool setVelocity;
        [DraggablePoint(true)]
        public Vector3 velocity;

        void Start()
        {
            body = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (body != null)
            {
                if (setVelocity)
                    body.velocity = velocity;
            }
        }
    }
}