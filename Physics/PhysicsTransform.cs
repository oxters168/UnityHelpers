using UnityEngine;

namespace UnityHelpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsTransform : MonoBehaviour
    {
        [System.Flags]
        public enum LockAxes
        {
            none = 0x0,

            up = 0x1,
            down = 0x2,
            left = 0x4,
            right = 0x8,
            forward = 0x10,
            back = 0x20,
        }

        /// <summary>
        /// Only used for local position and local rotation calculations, does not actively anchor self unless anchorPosition/anchorRotation is greater than 0. Inspector values are in world coordinates, use scripting to access local position and local rotation.
        /// </summary>
        [Tooltip("Only used for local position and local rotation calculations, does not actively anchor self unless anchorPosition/anchorRotation is greater than 0. Inspector values are in world coordinates, use scripting to access local position and local rotation.")]
        public Transform parent;
        [Range(0, 1), Tooltip("0 means don't anchor at all and 1 means anchor completely to the anchor position/rotation (by default is the starting position/rotation and local if there is parent)")]
        public float anchorPositionPercent, anchorRotationPercent;
        private Vector3 worldAnchorPosition;
        private Quaternion worldAnchorRotation;
        private Vector3 localAnchorPosition;
        private Quaternion localAnchorRotation;
        [Range(0, float.MaxValue), Tooltip("The max distance the object can be from original local position, or if there is no parent then world position")]
        public float localLinearLimit = float.MaxValue;

        /// <summary>
        /// Sets striveForPosition and striveForOrientation simultaneously.
        /// </summary>
        public bool strive { set { striveForPosition = value; striveForOrientation = value; } }
        /// <summary>
        /// Only counteracts gravity if rigidbody is affected by gravity and not kinematic
        /// </summary>
        [Tooltip("Only counteracts gravity if rigidbody is affected by gravity and is not kinematic")]
        public bool counteractGravity = true; //Suzan told me about PID controllers and how they work, so maybe in the future I can add the I to positional strivingness to counteract gravity/friction automatically.

        [Space(10)]
        public bool striveForPosition = true;
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
            get
            {
                if (parent != null)
                    return parent.InverseTransformPoint(position);
                else
                    return position;
            }
        }
        [Tooltip("The final multiplier or coefficient of the calculated force")]
        public float strength = 1;
        [Tooltip("In kg * m/s^2 (newtons)")]
        public float maxForce = 500;
        [Tooltip("If set to true, will dynamically set the strength value to be based on distance from given position")]
        public bool calculateStrengthByDistance = false;
        [Tooltip("This will be what the current distance is divided by to make a strength percentage (measured in meters)")]
        public float distanceDivisor = 5;
        public AnimationCurve strengthGraph = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [Tooltip("If set to true, will clamp the strength value to between 0 and 1")]
        public bool clampStrength = true;
        [Tooltip("World space directions where to counteract any forces acted on this Rigidbody (does not work well, better to use freeze position in rigidbody)")]
        public LockAxes lockedWorldDirections = LockAxes.none;
        [Tooltip("Local space directions where to counteract any forces acted on this Rigidbody (does not work well, better to use freeze position in rigidbody)")]
        public LockAxes lockedLocalDirections = LockAxes.none;
        public float minimumSpeedToBlock = 0.1f;

        [Space(10)]
        public bool striveForOrientation = true;
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
            get
            {
                if (parent != null)
                    return parent.InverseTransformRotation(rotation);
                else
                    return rotation;
            }
        }
        [Tooltip("Frequency is the speed of convergence. If damping is 1, frequency is the 1/time taken to reach ~95% of the target value. i.e. a frequency of 6 will bring you very close to your target within 1/6 seconds.")]
        public float frequency = 6;
        [Tooltip("damping = 1, the system is critically damped\ndamping is greater than 1 the system is over damped(sluggish)\ndamping is less than 1 the system is under damped(it will oscillate a little)")]
        public float damping = 1;
        [Tooltip("Stops the Rigidbody from rotating or having any angular velocity (does not work well, better to use freeze rotation in rigidbody)")]
        public bool lockRotation;

        [Space(10)]
        public bool striveForVelocity = false;
        public Vector3 velocity;
        public Vector3 localVelocity
        {
            set
            {
                if (parent != null)
                    velocity = parent.TransformDirection(value);
                else
                    velocity = value;
            }
            get
            {
                if (parent != null)
                    return parent.InverseTransformDirection(velocity);
                else
                    return velocity;
            }
        }
        public float velStrength = 1;
        [Tooltip("In kg * m/s^2 (newtons)")]
        public float velMaxForce = 500;

        public Rigidbody AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponent<Rigidbody>(); return _affectedBody; } }
        private Rigidbody _affectedBody;

        //private Vector3 strivedPosition;

        void Awake()
        {
            SetAnchorPosition(transform.position, Space.World);
            SetAnchorRotation(transform.rotation, Space.World);
        }
        void FixedUpdate()
        {
            if (striveForPosition)
            {
                Vector3 pushForceVector = CalculatePushForceVector();
                AffectedBody.AddForce(pushForceVector, ForceMode.Force);
            }

            if (striveForVelocity)
            {
                Vector3 boneForce = AffectedBody.CalculateRequiredForceForSpeed(velocity, Time.deltaTime, velMaxForce) * velStrength;
                AffectedBody.AddForce(boneForce, ForceMode.Force);
            }

            if (striveForOrientation)
            {
                Quaternion strivedOrientation = Quaternion.Lerp(rotation, parent != null ? parent.TransformRotation(localAnchorRotation) : rotation, anchorRotationPercent);
                Vector3 rotationTorque = AffectedBody.CalculateRequiredTorque(strivedOrientation, frequency, damping);
                AffectedBody.AddTorque(rotationTorque);
            }

            if (counteractGravity && AffectedBody.useGravity && !AffectedBody.isKinematic)
                AffectedBody.AddForce(-Physics.gravity * AffectedBody.mass);

            ApplyPositionalLock(lockedWorldDirections, lockedLocalDirections);
            if (lockRotation)
                ApplyRotationalLock();
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, localLinearLimit);
        }

        public Vector3 GetAnchorPositionInWorldCoords()
        {
            return parent != null ? parent.TransformPoint(localAnchorPosition) : worldAnchorPosition;
        }
        public Vector3 GetAnchorPositionInLocalCoords()
        {
            return parent != null ? localAnchorPosition : worldAnchorPosition;
        }
        public Quaternion GetAnchorRotationInWorldCoords()
        {
            return parent != null ? parent.TransformRotation(localAnchorRotation) : worldAnchorRotation;
        }
        public Quaternion GetAnchorRotationInLocalCoords()
        {
            return parent != null ? localAnchorRotation : worldAnchorRotation;
        }
        public void SetAnchorPosition(Vector3 anchorPosition, Space space)
        {
            if (space == Space.Self && parent != null)
            {
                localAnchorPosition = anchorPosition;
                worldAnchorPosition = parent.TransformPoint(anchorPosition);
            }
            else
            {
                worldAnchorPosition = anchorPosition;
                if (parent != null)
                    localAnchorPosition = parent.InverseTransformPoint(anchorPosition);
            }
        }
        public void SetAnchorRotation(Quaternion anchorRotation, Space space)
        {
            if (space == Space.Self && parent != null)
            {
                localAnchorRotation = anchorRotation;
                worldAnchorRotation = parent.TransformRotation(anchorRotation);
            }
            else
            {
                worldAnchorRotation = anchorRotation;
                if (parent != null)
                    localAnchorRotation = parent.InverseTransformRotation(anchorRotation);
            }
        }
        public Vector3 GetStrivedPosition()
        {
            Vector3 local = GetAnchorPositionInWorldCoords(); //Get original local position or strive position if no parent
            return Vector3.Lerp(position, local, anchorPositionPercent); //Get interpolated position
        }
        public Vector3 CalculatePushForceVector()
        {
            Vector3 strivedPosition = GetStrivedPosition();

            Vector3 delta = strivedPosition - GetAnchorPositionInWorldCoords(); //Get difference in position
            if (delta.sqrMagnitude > localLinearLimit * localLinearLimit) //If greater than limit
                strivedPosition = GetAnchorPositionInWorldCoords() + delta.normalized * localLinearLimit; //Set to limit

            if (calculateStrengthByDistance)
            {
                float currentDistance = (strivedPosition - AffectedBody.position).magnitude;
                strength = currentDistance;
                if (distanceDivisor != 0)
                    strength /= distanceDivisor;

                strength = strengthGraph.Evaluate(strength);
            }

            if (clampStrength)
                strength = Mathf.Clamp01(strength);

            return AffectedBody.CalculateRequiredForceForPosition(strivedPosition, Time.fixedDeltaTime, maxForce) * strength;
        }
        public void ApplyPositionalLock(LockAxes lockedWorldDirections, LockAxes lockedLocalDirections)
        {
            Vector3 originalLocalPosition = GetAnchorPositionInLocalCoords();
            IterateDirections((lockedDirection) =>
            {
                Vector3 positionDifference = transform.localPosition - originalLocalPosition;
                //float totalPosDifference = positionDifference.magnitude;

                Vector3 lockInverse = Vector3.one - lockedDirection.Abs();
                Vector3 positionalOffsetInDirection = positionDifference.Multiply(lockedDirection);
                float distance = positionalOffsetInDirection.magnitude;
                float percentInDirection = positionDifference.PercentDirection(lockedDirection);
                //float percentInDirection = totalPosDifference * positionDifference.PercentDirection(lockedDirection);
                if (percentInDirection > 0 && distance >= localLinearLimit)
                {
                    transform.localPosition = transform.localPosition.Multiply(lockInverse) + originalLocalPosition.Multiply(lockedDirection);

                    /*Vector3 localVelocity = AffectedBody.velocity.normalized;
                    if (parent != null)
                        localVelocity = parent.InverseTransformDirection(localVelocity);
                    //float totalSpeed = velocity.magnitude;
                    float speedInDirection = localVelocity.Multiply(lockedDirection).magnitude;
                    if (speedInDirection >= minimumSpeedToBlock)
                    {
                        Vector3 adjustedVelocity = localVelocity.Multiply(lockInverse);
                        float percentMagnitude = adjustedVelocity.magnitude;
                        if (parent != null)
                            adjustedVelocity = parent.TransformDirection(adjustedVelocity.normalized);
                        AffectedBody.velocity = adjustedVelocity * AffectedBody.velocity.magnitude * percentMagnitude;
                        //Vector3 velocityInDirection = lockedDirection * speedInDirection;
                        //AffectedBody.velocity = velocity - velocityInDirection;
                    }*/
                }
            }, lockedLocalDirections);
            //}, lockedWorldDirections, lockedLocalDirections, parent);

            //I don't like that I have the same code here twice, but what can I do
            Vector3 originalWorldPosition = GetAnchorPositionInWorldCoords();
            IterateDirections((lockedDirection) =>
            {
                Vector3 positionDifference = transform.position - originalWorldPosition;

                Vector3 lockInverse = Vector3.one - lockedDirection.Abs();
                Vector3 positionalOffsetInDirection = positionDifference.Multiply(lockedDirection);
                float distance = positionalOffsetInDirection.magnitude;
                float percentInDirection = positionDifference.PercentDirection(lockedDirection);
                if (percentInDirection > 0 && distance >= localLinearLimit)
                    transform.position = transform.position.Multiply(lockInverse) + originalWorldPosition.Multiply(lockedDirection);
            }, lockedWorldDirections);
        }
        public void ApplyRotationalLock()
        {
            AffectedBody.angularVelocity = Vector3.zero;
            transform.rotation = GetAnchorRotationInWorldCoords();
        }

        public static void IterateDirections(System.Action<Vector3> directionAction, LockAxes worldDirections, LockAxes localDirections = LockAxes.none, Transform transform = null)
        {
            LockAxes[] allDirections = (LockAxes[])System.Enum.GetValues(typeof(LockAxes));
            for (int i = 0; i < allDirections.Length; i++)
            {
                var currentDirection = allDirections[i];
                if ((worldDirections & currentDirection) != 0)
                    directionAction.Invoke(GetSingleDirection(currentDirection));
                if ((localDirections & currentDirection) != 0)
                    directionAction.Invoke(GetSingleDirection(currentDirection, transform));
            }
        }
        public static Vector3 GetSingleDirection(LockAxes direction, Transform transform = null)
        {
            Vector3 returnedDirection;

            if ((direction & LockAxes.up) != 0)
                returnedDirection = transform != null ? transform.up : Vector3.up;
            else if ((direction & LockAxes.down) != 0)
                returnedDirection = transform != null ? -transform.up : Vector3.down;
            else if ((direction & LockAxes.left) != 0)
                returnedDirection = transform != null ? -transform.right : Vector3.left;
            else if ((direction & LockAxes.right) != 0)
                returnedDirection = transform != null ? transform.right : Vector3.right;
            else if ((direction & LockAxes.forward) != 0)
                returnedDirection = transform != null ? transform.forward : Vector3.forward;
            else if ((direction & LockAxes.back) != 0)
                returnedDirection = transform != null ? -transform.forward : Vector3.back;
            else
                returnedDirection = Vector3.zero;

            return returnedDirection;
        }
    }
}