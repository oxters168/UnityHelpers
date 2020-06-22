using UnityEngine;

namespace UnityHelpers
{
	[ExecuteInEditMode]
    public class SetCenterOfMass : MonoBehaviour
    {
        private Rigidbody body { get { if (_body == null) { _body = GetComponent<Rigidbody>(); if (_body != null) InitCOM(); } return _body; } }
        private Rigidbody _body;

        [SerializeField, HideInInspector]
        private Vector3 centerOfMassOffset;
        private Vector3 originalCenterOfMass;
        public Vector3 centerOfMass;

        [Range(0, 100)]
        public float gizmoSize = 0.2f;

        [Space(10)]

        [Range(0, 1), Tooltip("The percent the COM will effect the torque of the object (this is not perfect, it can be very inaccurate)")]
        public float directionCorrectionPercent;
        [Tooltip("In case if your object has renderers that aren't 'physical', you can mask them out with this")]
        public LayerMask boundsMask = ~0;
        public float directionCorrectionFrequency = 1;
        public float directionCorrectionDamping = 0.333f;
        private bool skippedFirstFrame; //This seems to be necessary for arrows just shot out of a bow

        private void InitCOM()
        {
            if (body != null)
                body.centerOfMass = centerOfMass;
            else
                Debug.LogError("Could not get rigidbody");
        }
        void Update()
        {
            if (body != null)
                body.centerOfMass = centerOfMass;
        }
        void FixedUpdate()
        {
            if (body != null && directionCorrectionPercent > 0 && skippedFirstFrame)
            {
                float velocitySqrMag = body.velocity.sqrMagnitude;
                if (velocitySqrMag > 0.1f)
                {
                    Bounds objectBounds = transform.GetTotalBounds(Space.Self, boundsMask);
                    Vector3 comCenterOffset = body.worldCenterOfMass - transform.TransformPoint(objectBounds.center);

                    float comSqrMag = comCenterOffset.sqrMagnitude;
                    if (comSqrMag > 0.1f)
                    {
                        Vector3 comDirection = comCenterOffset.normalized;
                        Quaternion comDeltaRotation = Quaternion.FromToRotation(comDirection, transform.forward);

                        float maxExtentInDirection = objectBounds.extents.Multiply(comDirection).sqrMagnitude;
                        float comPercent = comSqrMag / maxExtentInDirection;
                        //Debug.DrawRay(objectBounds.center, comDirection * Mathf.Sqrt(maxExtentInDirection), Color.red);

                        Quaternion desiredRotation = Quaternion.LookRotation(body.velocity.normalized, Vector3.up) * comDeltaRotation;
                        Debug.DrawRay(transform.position, desiredRotation * Vector3.forward, Color.red);
                        body.AddTorque(directionCorrectionPercent * comPercent * body.CalculateRequiredTorque(desiredRotation, directionCorrectionFrequency, directionCorrectionDamping));
                    }
                }
            }

            if (!skippedFirstFrame)
                skippedFirstFrame = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (body != null && gizmoSize > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(body.centerOfMass, gizmoSize);
            }

            Gizmos.color = Color.green;
            Bounds objectBounds = transform.GetTotalBounds(Space.Self, boundsMask);
            Gizmos.DrawWireCube(objectBounds.center, objectBounds.size);
        }
    }
}