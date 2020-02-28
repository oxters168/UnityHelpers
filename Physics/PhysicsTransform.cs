using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsTransform : MonoBehaviour
    {
        [Tooltip("Only used for local position and local rotation calculations, does not actively anchor self. Inspector values are absolute, use scripting to access local position and local rotation.")]
        public Transform anchor;

        /// <summary>
        /// Sets striveForPosition, striveForOrientation, and counteractGravity simultaneously.
        /// </summary>
        public bool strive { set { striveForPosition = value; striveForOrientation = value; counteractGravity = value; } }
        public bool striveForPosition = true;
        public bool striveForOrientation = true;
        public bool counteractGravity = true; //Suzan told me about PID controllers and how they work, so maybe in the future I can add the I to positional strivingness to counteract gravity/friction automatically.
        
        [Space(10)]
        public Vector3 position;
        /// <summary>
        /// Sets position as if it were a child of anchor.
        /// </summary>
        public Vector3 localPosition
        {
            set
            {
                if (anchor != null)
                    position = anchor.TransformPoint(value);
                else
                    position = value;
            }
        }
        public float strength = 1;
        [Tooltip("In m/s")]
        public float maxSpeed = 20;
        //[Tooltip("Speed threshold before testing speed difference to stop")]
        //public float minSpeedTest = 20; //Highest recorded hand speed supposedly 67 m/s
        [Space(10)]
        public Quaternion rotation = Quaternion.identity;
        /// <summary>
        /// Sets rotation as if it were a child of anchor.
        /// </summary>
        public Quaternion localRotation
        {
            set
            {
                if (anchor != null)
                    rotation = anchor.TransformRotation(value);
                else
                    rotation = value;
            }
        }
        [Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.")]
        public float frequency = 6;
        [Tooltip("damping = 1, the system is critically damped\ndamping is greater than 1 the system is over damped(sluggish)\ndamping is less than 1 the system is under damped(it will oscillate a little)")]
        public float damping = 1;

        private Rigidbody affectedBody;
        private List<RepellantForce> repellingForces = new List<RepellantForce>();

        void Awake()
        {
            affectedBody = GetComponent<Rigidbody>();
        }
        void FixedUpdate()
        {
            if (striveForPosition)
            {
                Vector3 pushVelocity = affectedBody.CalculateRequiredVelocity(position, Time.fixedDeltaTime, strength, maxSpeed);
                float percentage = 1;
                //Vector3 actualPreviousForceVector = affectedBody.mass * (affectedBody.velocity / Time.fixedDeltaTime);
                //float previousPushForceSqr = previousPushForceVector.sqrMagnitude;
                //percentage = previousPushForceSqr > 0 ? Mathf.Clamp01(actualPreviousForceVector.sqrMagnitude / previousPushForceSqr) : 1;

                //float pushForce = pushForceVector.magnitude;
                //float pushAcceleration = pushForce / affectedBody.mass;
                //float pushSpeed = pushAcceleration * Time.fixedDeltaTime;
                //if (pushSpeed > 10)
                //    percentage = Mathf.Clamp(pushSpeed > 0 ? (previousVelocity.magnitude / pushSpeed) : 0, 0, 1f); //If the previous velocity is not meeting the expected velocity then that probably means something is trying to stop us like an obstacle or joint, so slow down
                
                //float acceleration = pushForce.magnitude / affectedBody.mass;
                /*Vector3 pushForceDirection = pushForceVector.normalized;
                //float totalRepellingForce = 0;
                for (int i = repellingForces.Count - 1; i >= 0; i--)
                {
                    var repellingForce = repellingForces[i];
                    if (Vector3.Dot(pushForceDirection, repellingForce.direction) <= 0) //If object is in front of the force being applied
                    {
                        //totalRepellingForce += repellingForce.mass * pushAcceleration;
                        pushForce -= pushForceDirection * repellingForce.mass * pushAcceleration; //Subtract the equal amount of force the object would apply based on mass
                        if (Vector3.Dot(pushForce.normalized, pushForceDirection) <= 0) //If we have reversed in direction, zero and break
                        {
                            pushForce = Vector3.zero;
                            repellingForces.Clear();
                            break;
                        }
                    }
                    repellingForces.RemoveAt(i);
                }*/
                //pushForceVector = pushForceDirection * Mathf.Max(Mathf.Lerp(totalRepellingForce, pushForce, percentage), maxForce);
                affectedBody.AddForce(pushVelocity * percentage, ForceMode.VelocityChange);
            }

            if (striveForOrientation)
                affectedBody.AddTorque(affectedBody.CalculateRequiredTorque(rotation, frequency, damping));

            if (counteractGravity && affectedBody.useGravity && !affectedBody.isKinematic)
                affectedBody.AddForce(-Physics.gravity * affectedBody.mass);

            //previousVelocity = affectedBody.velocity;
        }
        private void OnTriggerStay(Collider other)
        {
            repellingForces.Add(
                new RepellantForce()
                {
                    mass = other.attachedRigidbody != null ? other.attachedRigidbody.mass : affectedBody.mass,
                    direction = (other.transform.position - transform.position).normalized
                }
            );
        }

        public struct RepellantForce
        {
            public float mass;
            public Vector3 direction;
        }
    }
}