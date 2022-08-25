using UnityEngine;
#if UNITY_EDITOR
using UnityHelpers.Editor;
#endif

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class TankPhysics : MonoBehaviour
    {
        public Rigidbody TankBody { get { if (_tankBody == null) _tankBody = GetComponentInChildren<Rigidbody>(); return _tankBody; } }
        private Rigidbody _tankBody;
        private Rigidbody turretBody;

        public Transform turret;

        public float gas { get { return _gas; } set { _gas = Mathf.Clamp(value, -1, 1); } }
        private float _gas;
        public float steer { get { return _steer; } set { _steer = Mathf.Clamp(value, -1, 1); } }
        private float _steer;
        public float look { get { return _look; } set { _look = Mathf.Clamp(value, -1, 1); } }
        private float _look;
        [Tooltip("If set to true will change the behaviour of steer from turning left/right to turning to a specific direction")]
        public bool directionalSteer = true;
        [Tooltip("Only used if directionalSteer is set to true. Represents the look direction when steer is at 0.")]
        public Vector3 forwardSteerDir = Vector3.forward;
        [Tooltip("If set to true will change the behaviour of look from turning left/right to turning to a specific direction")]
        public bool directionalLook = true;
        [Tooltip("Only used when directionalLook is set to true. Represents the look direction when look is at 0.")]
        public Vector3 forwardLookDir = Vector3.forward;


        [Space(10), Tooltip("In m/s^2")]
        public float acc = 10;
        [Tooltip("In m/s^2")]
        public float dec = 20;
        [Tooltip("In m/s")]
        public float maxSpeed = 20;
        [Tooltip("Friction force in m/s^2")]
        public float fricDec = 20;

        [Space(10), Tooltip("In rev/s^2")]
        public float rotAcc = 0.4f;
        [Tooltip("In rev/s^2")]
        public float rotDec = 0.8f;
        [Tooltip("In rev/s")]
        public float maxRotSpeed = 0.8f;

        [Space(10)]
        public LayerMask groundMask = ~0;
        public int grndRays = 8;
        public float grndDist = 0.2f;
        public Vector3 treadRayShift = new Vector3(-0.5f, 0.1f, -0.6f);

        #if UNITY_EDITOR
        [Debug]
        #endif
        public string output;

        private Vector3 startPos;
        private Quaternion startRot;

        void Start()
        {
            if (turret != null)
                turretBody = SetupTurret(turret, TankBody);
            else
                Debug.LogWarning("Could not set up turret, transform was not provided");

            startPos = transform.position;
            startRot = transform.rotation;
        }

        void FixedUpdate()
        {
            output = "gas: " + gas + "\nsteer: " + steer + "\nlook: " + look;
            output += "\n\n" + ApplyAntiG(TankBody, grndRays, grndDist, treadRayShift, groundMask);
            output += "\n\n" + ApplyFriction(TankBody, fricDec);
            output += "\n\n" + Drive(TankBody, gas, acc, maxSpeed, dec, maxRotSpeed * 360 * Mathf.Deg2Rad);

            if (directionalSteer && Mathf.Abs(gas) > float.Epsilon)
            {
                TankBody.RotateTo(TankBody.transform.up, forwardSteerDir, MathHelpers.ThreeSixtyFi(steer * 360), rotAcc * 360 * Mathf.Deg2Rad, maxRotSpeed * 360 * Mathf.Deg2Rad, rotDec * 360 * Mathf.Deg2Rad);
            }
            else
                TankBody.Rotate(TankBody.transform.up, TankBody.transform.right, steer, rotAcc * 360 * Mathf.Deg2Rad, maxRotSpeed * 360 * Mathf.Deg2Rad, rotDec * 360 * Mathf.Deg2Rad);
            
            if (turretBody != null)
            {
                if (directionalLook)
                {
                    turretBody.RotateTo(turretBody.transform.up, forwardLookDir, MathHelpers.ThreeSixtyFi(look * 360), rotAcc * 360 * Mathf.Deg2Rad, maxRotSpeed * 360 * Mathf.Deg2Rad, rotDec * 360 * Mathf.Deg2Rad);
                }
                else
                    turretBody.Rotate(turretBody.transform.up, turretBody.transform.right, look, rotAcc * 360 * Mathf.Deg2Rad, maxRotSpeed * 360 * Mathf.Deg2Rad, rotDec * 360 * Mathf.Deg2Rad);
            }
        }

        private static Rigidbody SetupTurret(Transform turret, Rigidbody connectedBody)
        {
            // turretBody = turret.GetComponent<Rigidbody>();
            var turretBody = turret.gameObject.AddComponent<Rigidbody>();
            turretBody.drag = 0.05f;
            turretBody.useGravity = false;
            // turretBody.centerOfMass = Vector3.zero;
            // turretBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezePositionZ;
            // turretPosOffset = turret.localPosition;
            // turretRotOffset = turret.localRotation;
            // var turretJoint = gameObject.AddComponent<ConfigurableJoint>();
            // turretJoint.connectedBody = turretBody;
            var turretJoint = turret.gameObject.AddComponent<ConfigurableJoint>();
            turretJoint.connectedBody = connectedBody;
            turretJoint.xMotion = ConfigurableJointMotion.Locked;
            turretJoint.yMotion = ConfigurableJointMotion.Locked;
            turretJoint.zMotion = ConfigurableJointMotion.Locked;
            turretJoint.angularXMotion = ConfigurableJointMotion.Locked;
            // turretJoint.angularYMotion = ConfigurableJointMotion.Locked;
            turretJoint.angularZMotion = ConfigurableJointMotion.Locked;
            turretJoint.connectedMassScale = 0.00001f;

            return turretBody;
        }
        public void ResetLoc(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
            // turretBody.transform.localPosition = turretPosOffset; //Should not be neccessary since is child
            // turretBody.transform.localRotation = turretRotOffset;
            TankBody.velocity = Vector3.zero;
            TankBody.angularVelocity = Vector3.zero;
            turretBody.velocity = Vector3.zero;
            turretBody.angularVelocity = Vector3.zero;
        }

        private static string Drive(Rigidbody rigidbody, float input, float acc, float maxSpeed, float dec, float maxRotSpeed)
        {
            string output = string.Empty;

            var currentAngVel = rigidbody.GetAxisAngularVelocity(rigidbody.transform.up, rigidbody.transform.right);

            float percentMaxRot = 1 - Mathf.Clamp01(Mathf.Abs(currentAngVel / maxRotSpeed));
            input *= percentMaxRot;
            // if (Mathf.Abs(input) > float.Epsilon)
            //     input = Mathf.Sign(input) * Mathf.Clamp01(Mathf.Abs(input) - Mathf.Clamp01((percentMaxRot + Mathf.Abs(input)) - 1));
            var velProj = Vector3.ProjectOnPlane(rigidbody.velocity, rigidbody.transform.up);
            float currentVel = velProj.PercentDirection(rigidbody.transform.forward) * velProj.magnitude;
            float velDir = Mathf.Sign(currentVel);
            float accValue = input * (Mathf.Sign(input) != velDir ? acc + acc : acc);
            output += "Vel=" + currentVel; //debug
            bool inputVel = Mathf.Abs(input) > float.Epsilon;
            if (inputVel && Mathf.Abs(currentVel + (accValue * Time.fixedDeltaTime)) >= maxSpeed)
                accValue = Mathf.Sign(accValue) * (maxSpeed - Mathf.Abs(currentVel));
            else if (!inputVel && Mathf.Abs(currentVel) >= (dec * Time.fixedDeltaTime))
                accValue = -velDir * dec;
            else if (!inputVel && Mathf.Abs(currentVel) > Mathf.Epsilon)
                accValue = -(currentVel / Time.fixedDeltaTime);
            output += " + " + accValue; //debug
            rigidbody.AddForce((rigidbody.transform.forward * accValue), ForceMode.Acceleration);
            // turretBody.AddForce((transform.forward * accValue), ForceMode.Acceleration);
            return output;
        }
        private static string ApplyFriction(Rigidbody rigidbody, float fricDec)
        {
            string output = string.Empty;

            var velProj = Vector3.ProjectOnPlane(rigidbody.velocity, rigidbody.transform.up);
            float currentVel = velProj.PercentDirection(rigidbody.transform.forward) * velProj.magnitude;

            var leftoverVel = velProj - rigidbody.transform.forward * currentVel;
            var leftoverSpeed = leftoverVel.magnitude;
            output += "Fric=" + (Mathf.Sign(Vector3.Dot(leftoverVel, rigidbody.transform.right)) * leftoverSpeed); //debug
            var friction = Vector3.zero;
            if (leftoverSpeed >= fricDec)
                friction = -(leftoverVel).normalized * fricDec;
            else if (leftoverSpeed >= float.Epsilon)
                friction = -(leftoverVel).normalized * (leftoverSpeed / Time.fixedDeltaTime);

            rigidbody.AddForce(friction, ForceMode.Acceleration);

            return output;
        }
        private static string ApplyAntiG(Rigidbody rigidbody, int grndRays, float grndDist, Vector3 treadRayShift, LayerMask groundMask)
        {
            string output = string.Empty;

            var bounds = rigidbody.transform.GetBounds(Space.World, true);
            var center = bounds.center;
            bounds = rigidbody.transform.GetBounds(Space.Self, true);
            Vector3 botCenter = center + -rigidbody.transform.up * (bounds.extents.y + treadRayShift.y) + rigidbody.transform.forward * (bounds.extents.z + treadRayShift.z);
            int indLines = grndRays / 2;
            for (int i = 0; i < grndRays; i++)
            {
                var front = botCenter + ((i < indLines ? -1 : 1) * rigidbody.transform.right * (bounds.extents.x + treadRayShift.x));
                var currentPoint = front - (rigidbody.transform.forward * (bounds.size.z / indLines) * (i % indLines));
                RaycastHit hitInfo;
                bool isGrounded = Physics.Raycast(currentPoint, -rigidbody.transform.up, out hitInfo, grndDist, groundMask);
                var percentGrounded = isGrounded ? 1 - (hitInfo.distance / grndDist) : 0;
                output += "\n" + i + ": " + percentGrounded;
                Debug.DrawRay(currentPoint, -rigidbody.transform.up * grndDist, Color.Lerp(Color.red, Color.green, percentGrounded));
                // TankBody.AddForceAtPosition((-Physics.gravity / grndRays) * percentGrounded, currentPoint, ForceMode.Acceleration);
                if (isGrounded)
                    rigidbody.AddForceAtPosition((-Physics.gravity / grndRays), currentPoint, ForceMode.Acceleration);
            }
            // var botLeft = botCenter + -transform.right * bounds.extents.x;
            // var botRight = botCenter + transform.right * bounds.extents.x;
            // Vector3 antiG = -Physics.gravity;

            
            // turretBody.AddForce(friction, ForceMode.Acceleration);
            return output;
        }
    }
}