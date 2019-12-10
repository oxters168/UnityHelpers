using UnityEngine;

namespace UnityHelpers
{
    public class SetCenterOfMass : MonoBehaviour
    {
        private Rigidbody body;

        [SerializeField, HideInInspector]
        private Vector3 centerOfMassOffset;
        private Vector3 originalCenterOfMass;
        [DraggablePoint(true)]
        public Vector3 centerOfMass;

        [Range(0, float.MaxValue)]
        public float gizmoSize;

        [Space(10)]

        [Range(0, 1), Tooltip("The percent the COM will effect the torque of the object (this is not perfect, it can be very inaccurate)")]
        public float directionCorrectionPercent;
        [Tooltip("In case if your object has renderers that aren't 'physical', you can mask them out with this")]
        public LayerMask boundsMask = ~0;
        public float directionCorrectionFrequency = 1;
        public float directionCorrectionDamping = 0.333f;

        private void Start()
        {
            body = GetComponent<Rigidbody>();
            if (body != null)
            {
                originalCenterOfMass = body.centerOfMass;
                centerOfMass = originalCenterOfMass + centerOfMassOffset;
                body.centerOfMass = centerOfMass;
            }
            else
                Debug.LogError("Could not get rigidbody");
        }
        void Update()
        {
            if (body != null)
            {
                Vector3 newOffset = centerOfMass - originalCenterOfMass;
                if (newOffset != centerOfMassOffset)
                {
                    centerOfMassOffset = newOffset;
                    body.centerOfMass = originalCenterOfMass + centerOfMassOffset;
                }
            }
        }
        void FixedUpdate()
        {
            if (body != null && directionCorrectionPercent > 0 && body.velocity.sqrMagnitude > 0)
            {
                Bounds objectBounds = transform.GetTotalBounds(boundsMask, true, true);
                Vector3 comCenterOffset = body.worldCenterOfMass - objectBounds.center;

                float comSqrMag = comCenterOffset.sqrMagnitude;

                if (comSqrMag > 0)
                {
                    Vector3 comDirection = comCenterOffset.normalized;
                    Quaternion comDeltaRotation = Quaternion.FromToRotation(comDirection, transform.forward);

                    float maxExtentInDirection = objectBounds.extents.Multiply(comDirection).sqrMagnitude;
                    float comPercent = comSqrMag / maxExtentInDirection;
                    //Debug.DrawRay(objectBounds.center, comDirection * Mathf.Sqrt(maxExtentInDirection), Color.red);

                    body.AddTorque(directionCorrectionPercent * comPercent * body.CalculateRequiredTorque(comDeltaRotation * Quaternion.LookRotation(body.velocity.normalized, Vector3.up), directionCorrectionFrequency, directionCorrectionDamping));
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (body != null && gizmoSize > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(body.worldCenterOfMass, gizmoSize);
            }

            Gizmos.color = Color.green;
            Bounds objectBounds = transform.GetTotalBounds(boundsMask, true, true);
            Gizmos.DrawWireCube(objectBounds.center, objectBounds.size);
        }
    }
}