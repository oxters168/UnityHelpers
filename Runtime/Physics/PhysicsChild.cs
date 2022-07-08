using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// Under construction, not fully fleshed out yet
    /// </summary>
    public class PhysicsChild : MonoBehaviour
    {
        private const float tolerance = 0.000001f;
        public Rigidbody PhysicsBody { get { if (_physicsBody == null) _physicsBody = GetComponent<Rigidbody>(); return _physicsBody; } }
        private Rigidbody _physicsBody;

        public Transform psuedoParent;

        // [Space(10)]
        // public bool freezePosX;
        // public bool freezePosY;
        // public bool freezePosZ;
        // [Space(10)]
        // public bool freezeRotX;
        // public bool freezeRotY;
        // public bool freezeRotZ;

        private Vector3 prevPos;
        private Quaternion prevRot;
        private Vector3 prevVelocity;

        private Vector3 prevParentPos;
        private Quaternion prevParRot;

        private Vector3 prevTransForce;
        private Vector3 prevRotForce;
        private Vector3 prevInertiaTensor;
        private Quaternion prevInertiaTensorRot;

        private Vector3 remainingVelocity = Vector3.zero;
        private Vector3 remainingAngularVelocity = Vector3.zero;

        void Awake()
        {
            prevParentPos = psuedoParent.position;
            prevParRot = psuedoParent.rotation;
        }
        // void Update()
        // {
        //     //Not sure how to properly decrease velocity and angular velocity
        // }
        void FixedUpdate()
        {
            var translationForce = Vector3.zero;
            var rotationForce = Vector3.zero;
            // var forwardForce = GetForceInDir(psuedoParent.forward);

            //Calculate change in rotation
            var deltaRot = psuedoParent.rotation * Quaternion.Inverse(prevParRot);
            var finalRot = deltaRot * transform.rotation;
            //Calculate effect of rotation on position
            var relativePos = transform.position - psuedoParent.position;
            var gammaPos = deltaRot * relativePos + psuedoParent.position;
            //Calculate total change in position
            var deltaPos = psuedoParent.position - prevParentPos;
            var finalPos = gammaPos + deltaPos;

            //I don't like the if statments, but I want to be sure that adhering to the parent will only happen when the parent moves or rotates 
            if ((!prevParentPos.EqualTo(psuedoParent.position, tolerance) || (!prevParRot.SameOrientationAs(psuedoParent.rotation) && !transform.position.EqualTo(psuedoParent.position, tolerance))) && !finalPos.EqualTo(prevPos, tolerance))
            {
                Debug.Log("Applying parent translation");
                //By taking in the actual velocity and angular velocity of the rigidbody, we are essentially cancelling out any other forces
                //When changing instead to Vector3.zero, does not work when there is fast movement to the transform
                //Need to find a fix
                translationForce += PhysicsHelpers.CalculateRequiredForceForPosition(finalPos, prevPos, PhysicsBody.velocity, PhysicsBody.mass, Time.fixedDeltaTime);
            }
            if (!prevParRot.SameOrientationAs(psuedoParent.rotation) && !finalRot.SameOrientationAs(prevRot))
            {
                Debug.Log("Applying parent rotation");
                // rotationForce += PhysicsHelpers.CalculateRequiredTorqueForRotation(finalRot, prevRot, PhysicsBody.angularVelocity, PhysicsBody.inertiaTensor, PhysicsBody.inertiaTensorRotation, Time.fixedDeltaTime);
                rotationForce += PhysicsHelpers.CalculateRequiredAngularAccelerationForRotation(finalRot, prevRot, PhysicsBody.angularVelocity, Time.fixedDeltaTime);
            }

            // PhysicsBody.AddForce(-forwardForce);

            //Applying accumulated forces to the rigidbody to achieve desired position and rotation
            if (!translationForce.EqualTo(Vector3.zero, tolerance))
                PhysicsBody.AddForce(translationForce);
            if (!rotationForce.EqualTo(Vector3.zero, tolerance))
                // PhysicsBody.AddTorque(rotationForce);
                PhysicsBody.AddTorque(rotationForce, ForceMode.Acceleration);
                // PhysicsBody.AddTorque(PhysicsHelpers.CalculateAngularAccelerationFromTorque(rotationForce, PhysicsBody.inertiaTensor, PhysicsBody.inertiaTensorRotation), ForceMode.Acceleration);

            //Cancelling out the previous frame's forces (this assumes that the forces applied were exactly enough to achieve the desired position and rotation)
            if (!prevTransForce.EqualTo(Vector3.zero, tolerance))
                PhysicsBody.AddForce(-prevTransForce);
            if (!prevRotForce.EqualTo(Vector3.zero, tolerance))
            {
                // var stepRotAcc = PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot);
                // PhysicsBody.AddTorque(PhysicsHelpers.CalculateTorqueFromAngularAcceleration(-stepRotAcc, transform.rotation, stepRotAcc * Time.fixedDeltaTime, prevInertiaTensor, prevInertiaTensorRot));
                // PhysicsBody.AddTorque(-PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot), ForceMode.Acceleration);
                // PhysicsBody.AddTorque(-prevRotForce);
                PhysicsBody.AddTorque(-prevRotForce, ForceMode.Acceleration);
            }

            prevVelocity = PhysicsBody.velocity;
            prevPos = transform.position;
            prevRot = transform.rotation;
            prevParentPos = psuedoParent.position;
            prevInertiaTensor = PhysicsBody.inertiaTensor;
            prevInertiaTensorRot = PhysicsBody.inertiaTensorRotation;
            prevParRot = psuedoParent.rotation;
            prevTransForce = translationForce;
            prevRotForce = rotationForce;
        }
        // void LateUpdate()
        // {
        //     remainingVelocity += prevTransForce * Time.fixedDeltaTime;
        //     remainingAngularVelocity += PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot) * Time.fixedDeltaTime;
        // }

        public Vector3 GetForceInDir(Vector3 dir)
        {
            var acc = PhysicsBody.velocity - prevVelocity;
            return dir * acc.magnitude * Vector3.Dot(acc, dir) * PhysicsBody.mass;
        }
    }
}