using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsTransform : MonoBehaviour
    {
        /// <summary>
        /// Only used for local position and local rotation calculations, does not actively anchor self unless anchorPosition/anchorRotation is greater than 0. Inspector values are in world coordinates, use scripting to access local position and local rotation.
        /// </summary>
        [Tooltip("Only used for local position and local rotation calculations, does not actively anchor self unless anchorPosition/anchorRotation is greater than 0. Inspector values are in world coordinates, use scripting to access local position and local rotation.")]
        public Transform parent;
        private PhysicsTransform parentPhysicsTransform;
        [Range(0, 1), Tooltip("0 means don't anchor at all, 1 means anchor completely")]
        public float anchorPositionPercent, anchorRotationPercent;
        /// <summary>
        /// This requires the 'parent' to have a PhysicsTransform attached
        /// </summary>
        public bool affectParent;
        public float maxDistanceFromParent = 0.01f;
        private Vector3 anchorPositionRelativeToSelf, anchorPositionRelativeToParent;
        //[Range(0, 1), Tooltip("0 means don't anchor at all, 1 means anchor completely")]
        //public float anchorRotaion;
        private Vector3 originalLocalPosition;
        private Quaternion originalLocalRotation;
        [Range(0, float.MaxValue), Tooltip("The max distance the object can be from original local position, or if there is no parent then world position")]
        public float localLinearLimit = float.MaxValue;

        public ConfigurableJoint joint; //Temporary will move later

        /// <summary>
        /// Sets striveForPosition and striveForOrientation simultaneously.
        /// </summary>
        public bool strive { set { striveForPosition = value; striveForOrientation = value; } }
        public bool striveForPosition = true;
        public bool striveForOrientation = true;
        /// <summary>
        /// Only counteracts gravity if rigidbody is affected by gravity and not kinematic
        /// </summary>
        [Tooltip("Only counteracts gravity if rigidbody is affected by gravity and not kinematic")]
        public bool counteractGravity = true; //Suzan told me about PID controllers and how they work, so maybe in the future I can add the I to positional strivingness to counteract gravity/friction automatically.
        
        [Space(10)]
        public Vector3 position;
        /// <summary>
        /// Sets position relative to parent.
        /// </summary>
        public Vector3 localPosition
        {
            set
            {
                if (parent != null)
                    position = parent.TransformPoint(value);
                else
                    position = value;
            }
        }
        public float strength = 1;
        [Tooltip("In kg * m/s^2")]
        public float maxForce = 500;
        //[Tooltip("Speed threshold before testing speed difference to stop")]
        //public float minSpeedTest = 20; //Highest recorded hand speed supposedly 67 m/s
        [Space(10)]
        public Quaternion rotation = Quaternion.identity;
        /// <summary>
        /// Sets rotation relative to parent.
        /// </summary>
        public Quaternion localRotation
        {
            set
            {
                if (parent != null)
                    rotation = parent.TransformRotation(value);
                else
                    rotation = value;
            }
        }
        [Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.")]
        public float frequency = 6;
        [Tooltip("damping = 1, the system is critically damped\ndamping is greater than 1 the system is over damped(sluggish)\ndamping is less than 1 the system is under damped(it will oscillate a little)")]
        public float damping = 1;

        public Rigidbody AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponent<Rigidbody>(); return _affectedBody; } }
        private Rigidbody _affectedBody;

        private List<RepellantForce> repellingForces = new List<RepellantForce>();
        private LineRenderer currentLine;

        private Vector3 affectionPosition;
        private Vector3 previousPosition;
        private float maxDistanceFromChild = -1;
        private float currentMaxAffectionDistance = float.MinValue;

        void Awake()
        {
            //AffectedBody = GetComponent<Rigidbody>();

            if (parent != null)
            {
                originalLocalPosition = parent.InverseTransformPoint(transform.position);
                originalLocalRotation = parent.InverseTransformRotation(transform.rotation);

                parentPhysicsTransform = parent.GetComponent<PhysicsTransform>();

                Vector3 anchorPosition = GetAnchorPoint();
                anchorPositionRelativeToSelf = anchorPosition - AffectedBody.position;
                anchorPositionRelativeToParent = anchorPosition - parentPhysicsTransform.AffectedBody.position;
                //anchorPositionRelativeToSelf = transform.position - anchorPosition;
                //anchorPositionRelativeToParent = parent.position - anchorPosition;
                //anchorPositionRelativeToSelf = transform.InverseTransformPoint(anchorPosition);
                //anchorPositionRelativeToParent = parent.InverseTransformPoint(anchorPosition);
            }
        }
        void FixedUpdate()
        {
            if (striveForPosition)
            {
                Vector3 local = parent != null ? parent.TransformPoint(originalLocalPosition) : position; //Get original local position or strive position if no parent
                Vector3 strivedPosition = Vector3.Lerp(position, local, anchorPositionPercent); //Get interpolated position
                Vector3 delta = strivedPosition - local; //Get difference in position
                if (delta.sqrMagnitude > localLinearLimit * localLinearLimit) //If greater than limit
                    strivedPosition = local + delta.normalized * localLinearLimit; //Set to limit

                //Affection interpolation
                //if (currentMaxAffectionDistance > maxDistanceFromChild)
                //{
                float striveAffectionDistance = Vector3.Distance(strivedPosition, affectionPosition);
                //float percentAffected = Mathf.Clamp01(currentMaxAffectionDistance / maxDistanceFromChild);
                float percentAffected = Mathf.Clamp01(striveAffectionDistance / maxDistanceFromChild);
                strivedPosition = Vector3.Lerp(strivedPosition, affectionPosition, percentAffected);
                    //affectionDistance = -1;
                    //currentMaxAffectionDistance = float.MinValue;
                //}

                Vector3 pushForceVector = AffectedBody.CalculateRequiredForceForPosition(strivedPosition, Time.fixedDeltaTime, strength, maxForce);
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
                AffectedBody.AddForce(pushForceVector * percentage, ForceMode.Force);
            }

            if (striveForOrientation)
            {
                Quaternion strivedOrientation = Quaternion.Lerp(rotation, parent != null ? parent.TransformRotation(originalLocalRotation) : rotation, anchorRotationPercent);
                AffectedBody.AddTorque(AffectedBody.CalculateRequiredTorque(strivedOrientation, frequency, damping));
            }

            if (counteractGravity && AffectedBody.useGravity && !AffectedBody.isKinematic)
                AffectedBody.AddForce(-Physics.gravity * AffectedBody.mass);

            #region Parent Affection
            bool returnLine = false;
            if (affectParent && parentPhysicsTransform != null)
            {
                //Vector3 parentAnchorWP = parent.TransformPoint(anchorPositionRelativeToParent);
                //Vector3 selfAnchorWP = transform.TransformPoint(anchorPositionRelativeToSelf);
                //Vector3 parentAnchorWP = parentPhysicsTransform.AffectedBody.position + anchorPositionRelativeToParent;
                //Vector3 selfAnchorWP = AffectedBody.position + anchorPositionRelativeToSelf;
                Matrix4x4 mat = Matrix4x4.TRS(AffectedBody.position, AffectedBody.rotation, transform.localScale);
                Matrix4x4 parentMat = Matrix4x4.TRS(parentPhysicsTransform.AffectedBody.position, parentPhysicsTransform.AffectedBody.rotation, parent.localScale);
                Vector3 parentAnchorWP = parentMat.MultiplyPoint(anchorPositionRelativeToParent);
                Vector3 selfAnchorWP = mat.MultiplyPoint(anchorPositionRelativeToSelf);

                Vector3 difference = parentAnchorWP - selfAnchorWP;
                float anchorDistance = difference.magnitude;

                if (currentLine == null)
                    currentLine = PoolManager.GetPool("Lines").Get<LineRenderer>();

                currentLine.SetPositions(new Vector3[] { parentAnchorWP, parentAnchorWP - difference });

                if (anchorDistance > parentPhysicsTransform.currentMaxAffectionDistance)
                {
                    //parentPhysicsTransform.affectionPosition = parent.position - (difference.normalized * (anchorDistance - maxDistanceFromParent));
                    parentPhysicsTransform.affectionPosition = parentPhysicsTransform.AffectedBody.position - (difference.normalized * (anchorDistance - maxDistanceFromParent));
                    parentPhysicsTransform.maxDistanceFromChild = maxDistanceFromParent;
                    parentPhysicsTransform.currentMaxAffectionDistance = anchorDistance;
                }
            }
            else
                returnLine = true;
            if (returnLine && currentLine != null)
            {
                PoolManager.GetPool("Lines").Return(currentLine.transform);
                currentLine = null;
            }
            #endregion

            previousPosition = AffectedBody.position;
            //previousVelocity = affectedBody.velocity;
        }
        private void OnTriggerStay(Collider other)
        {
            repellingForces.Add(
                new RepellantForce()
                {
                    mass = other.attachedRigidbody != null ? other.attachedRigidbody.mass : AffectedBody.mass,
                    direction = (other.transform.position - transform.position).normalized
                }
            );
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (parent != null)
                Gizmos.DrawWireSphere(GetAnchorPoint(), 0.005f);
            //Gizmos.DrawWireSphere(transform.position, localLinearLimit);
        }

        private Vector3 GetAnchorPoint()
        {
            float height = transform.GetBounds(false).extents.y;
            return transform.position - transform.up * height;
            //return VectorHelpers.Average(transform.position, parent.position);
        }
        public struct RepellantForce
        {
            public float mass;
            public Vector3 direction;
        }
    }
}