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
        private Rigidbody dud;

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

        public bool showDud = false;

        void Awake()
        {
            prevParentPos = psuedoParent.position;
            prevParRot = psuedoParent.rotation;
        }
        void OnDrawGizmos()
        {
            if (showDud && dud != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.matrix = dud.transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
        }
        void Start()
        {
            dud = GameObject.CreatePrimitive(PrimitiveType.Cube).AddComponent<Rigidbody>();
            dud.transform.position = transform.position;
            dud.transform.rotation = transform.rotation;
            Destroy(dud.GetComponent<MeshFilter>());
            Destroy(dud.GetComponent<Renderer>());
            Destroy(dud.GetComponent<Collider>());
            dud.name = "Dud";
            MimicOntoDud();
        }
        void Update()
        {
            //Not sure how to properly decrease velocity and angular velocity
            // remainingVelocity = ApplyDrag(remainingVelocity, PhysicsBody.drag, Time.deltaTime);
            // remainingAngularVelocity = ApplyDrag(remainingAngularVelocity, PhysicsBody.drag, Time.deltaTime);
        }
        void FixedUpdate()
        {
            MimicOntoDud();

            var translationForce = Vector3.zero;
            var rotationForce = Vector3.zero;
            // var forwardForce = GetForceInDir(psuedoParent.forward);

            // var id = Quat_d.identity;
            //Calculate change in rotation
            var deltaRot = psuedoParent.rotation * Quaternion.Inverse(prevParRot);
            // var finalRot = deltaRot * transform.rotation;
            var finalRot = psuedoParent.rotation;
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
                Debug.Log("ParentPos: " + prevParentPos + " + " + deltaPos + " = " + psuedoParent.position + "\n"
                + "ChildPos: (" + transform.position + " => " + gammaPos + ") + " + deltaPos + " = " + finalPos);
                translationForce += PhysicsHelpers.CalculateRequiredForceForPosition(finalPos, prevPos, dud.velocity, PhysicsBody.mass, Time.fixedDeltaTime).FixNaN();
            }
            if (!prevParRot.SameOrientationAs(psuedoParent.rotation) && !finalRot.SameOrientationAs(prevRot))
            {
                Debug.Log("Applying parent rotation");
                Debug.Log("ParentRot: " + prevParRot.eulerAngles + " + " + deltaRot.eulerAngles + " = " + psuedoParent.rotation.eulerAngles + "\n"
                + "ChildRot: " + transform.rotation.eulerAngles + " + " + deltaRot.eulerAngles + " = " + finalRot.eulerAngles);
                rotationForce += PhysicsHelpers.CalculateRequiredTorqueForRotation(finalRot, prevRot, dud.angularVelocity, PhysicsBody.inertiaTensor, PhysicsBody.inertiaTensorRotation, Time.fixedDeltaTime);
                // rotationForce += PhysicsHelpers.CalculateRequiredAngularAccelerationForRotation(finalRot, prevRot, remainingAngularVelocity, Time.fixedDeltaTime).FixNaN();
            }

            //Calculated up here in case they get affected by newly added forces
            Vector3 stoppingTransForce = dud.CalculateRequiredForceForSpeed(Vector3.zero, Time.fixedDeltaTime);
            Vector3 stoppingRotForce = dud.CalculateTorqueForAngularVelocity(Vector3.zero, Time.fixedDeltaTime);

            // PhysicsBody.AddForce(-forwardForce);

            //Applying accumulated forces to the rigidbody to achieve desired position and rotation
            if (!translationForce.EqualTo(Vector3.zero, tolerance))
            {
                PhysicsBody.AddForce(translationForce);
                dud.AddForce(translationForce);
            }
            if (!rotationForce.EqualTo(Vector3.zero, tolerance))
            {
                // PhysicsBody.AddTorque(rotationForce);
                PhysicsBody.AddTorque(rotationForce);
                // PhysicsBody.AddTorque(PhysicsHelpers.CalculateAngularAccelerationFromTorque(rotationForce, PhysicsBody.inertiaTensor, PhysicsBody.inertiaTensorRotation), ForceMode.Acceleration);
                dud.AddTorque(rotationForce);
            }

            //Cancelling out the previous frame's forces (this assumes that the forces applied were exactly enough to achieve the desired position and rotation)
            if (!stoppingTransForce.EqualTo(Vector3.zero, tolerance))
            {
                PhysicsBody.AddForce(stoppingTransForce);
                dud.AddForce(stoppingTransForce);
            }
            if (!stoppingRotForce.EqualTo(Vector3.zero, tolerance))
            {
                // var stepRotAcc = PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot);
                // PhysicsBody.AddTorque(PhysicsHelpers.CalculateTorqueFromAngularAcceleration(-stepRotAcc, transform.rotation, stepRotAcc * Time.fixedDeltaTime, prevInertiaTensor, prevInertiaTensorRot));
                // PhysicsBody.AddTorque(-PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot), ForceMode.Acceleration);
                // PhysicsBody.AddTorque(-prevRotForce);

                PhysicsBody.AddTorque(stoppingRotForce);
                dud.AddTorque(stoppingRotForce);
            }

            remainingVelocity = (translationForce / PhysicsBody.mass) * Time.fixedDeltaTime;
            remainingAngularVelocity = (rotationForce / PhysicsBody.mass) * Time.fixedDeltaTime;

            // var extraForces = GetForceInDir(psuedoParent.forward);
            // if (!extraForces.Approximately(Vector3.zero))
            // {
            //     Debug.Log("Forward force: " + extraForces.magnitude + " N");
            //     PhysicsBody.AddForce(-extraForces);
            // }


            prevVelocity = PhysicsBody.velocity;
            prevPos = transform.position;
            prevRot = transform.rotation;
            prevParentPos = psuedoParent.position;
            prevParRot = psuedoParent.rotation;
            prevInertiaTensor = PhysicsBody.inertiaTensor;
            prevInertiaTensorRot = PhysicsBody.inertiaTensorRotation;
            prevTransForce = translationForce;
            prevRotForce = rotationForce;

            // remainingAngularVelocity = PhysicsHelpers.CalculateAngularAccelerationFromTorque(prevRotForce, prevInertiaTensor, prevInertiaTensorRot) * Time.fixedDeltaTime;
        }
        void LateUpdate()
        {
            
        }

        private void MimicOntoDud()
        {
            dud.detectCollisions = false;
            dud.useGravity = false;
            dud.mass = PhysicsBody.mass;
            dud.drag = PhysicsBody.drag;
            dud.angularDrag = PhysicsBody.angularDrag;
            dud.centerOfMass = PhysicsBody.centerOfMass;
        }
        public Vector3 GetForceInDir(Vector3 dir)
        {
            // var acc = (PhysicsBody.velocity / Time.fixedDeltaTime);
            var acc = (PhysicsBody.velocity - remainingVelocity) / Time.fixedDeltaTime;
            // var acc = (PhysicsBody.velocity - remainingVelocity) - (prevVelocity - ((prevTransForce / PhysicsBody.mass) * Time.fixedDeltaTime)); 
            return dir * acc.magnitude * VectorHelpers.PercentDirection(acc, dir) * PhysicsBody.mass;
        }

        public Vector3 ApplyDrag(Vector3 velocity, float drag, float timestep = 0.02f)
        {
            return velocity * Mathf.Clamp01(1.0f - drag * timestep);
        }
    }
}