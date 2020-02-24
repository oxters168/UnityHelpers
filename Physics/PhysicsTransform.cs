using UnityEngine;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsTransform : MonoBehaviour
    {
        public Vector3 position;
        public Quaternion rotation;
        public float strength = 1;
        public bool strive;
        private Rigidbody affectedBody;

        void Awake()
        {
            affectedBody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (strive)
            {
                Vector3 direction = position - transform.position;
                Vector3 deltaVelocity = direction - affectedBody.velocity;
                if (affectedBody.useGravity && !affectedBody.isKinematic)
                    affectedBody.AddForce(-Physics.gravity * affectedBody.mass);
                    //deltaVelocity += Physics.gravity * Time.fixedDeltaTime;
                affectedBody.AddForce(deltaVelocity * strength, ForceMode.VelocityChange);
            }
        }
    }
}