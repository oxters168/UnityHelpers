using UnityEngine;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsTransform : MonoBehaviour
    {
        public bool striveForPosition = true;
        public bool striveForOrientation = true;
        public bool counteractGravity = true; //Suzan told me about PID controllers and how they work, so maybe in the future I can add the I to positional strivingness to counteract gravity/friction automatically.
        
        [Space(10)]
        public Vector3 position;
        public float strength = 10;
        public float maxSpeed = float.MaxValue;
        [Space(10)]
        public Quaternion rotation = Quaternion.identity;
        [Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.")]
        public float frequency = 6;
        [Tooltip("damping = 1, the system is critically damped\ndamping is greater than 1 the system is over damped(sluggish)\ndamping is less than 1 the system is under damped(it will oscillate a little)")]
        public float damping = 1;

        private Rigidbody affectedBody;

        void Awake()
        {
            affectedBody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if (striveForPosition)
                affectedBody.AddForce(affectedBody.CalculateRequiredVelocity(position, strength, maxSpeed), ForceMode.VelocityChange);

            if (striveForOrientation)
                affectedBody.AddTorque(affectedBody.CalculateRequiredTorque(rotation, frequency, damping));

            if (counteractGravity && affectedBody.useGravity && !affectedBody.isKinematic)
                affectedBody.AddForce(-Physics.gravity * affectedBody.mass);
        }
    }
}