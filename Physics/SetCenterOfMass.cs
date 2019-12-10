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

        [Range(0, 1)]
        public float directionCorrectionPercent;
        public float directionCorrectionFrequency = 1;
        public float directionCorrectionDamping = 0.1f;

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
            if (directionCorrectionPercent > 0 && centerOfMass.sqrMagnitude > 0 && body.velocity.sqrMagnitude > 0)
            {
                Vector3 comDirection = transform.TransformDirection(centerOfMass.normalized);
                Quaternion comDeltaRotation = Quaternion.FromToRotation(comDirection, transform.forward);
                body.AddTorque(directionCorrectionPercent * body.CalculateRequiredTorque(comDeltaRotation * Quaternion.LookRotation(body.velocity.normalized, Vector3.up), directionCorrectionFrequency, directionCorrectionDamping));
            }
            /*float comSqrMag = centerOfMass.sqrMagnitude;
            if (directionCorrectionPercent > 0 && comSqrMag > 0)
            {
                Vector3 velocityDirection = previousVelocity.normalized;
                Vector3 comDirection = transform.TransformDirection(centerOfMass.normalized);
                float dotAngle = Vector3.Dot(velocityDirection, comDirection);
                float percentCorrect = -(dotAngle - 3) / 4f * multiplier; //[2 - 4] => [0.5 - 1]
                Vector3 torqueDirection = Vector3.Cross(comDirection, velocityDirection).normalized;
                Debug.DrawRay(transform.position, torqueDirection, Color.red);
                body.AddTorque(torqueDirection * directionCorrectionPercent * comSqrMag * percentCorrect);
            }*/

            //if (body)
            //    previousVelocity = body.velocity;
        }

        private void OnDrawGizmos()
        {
            if (body != null && gizmoSize > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(body.worldCenterOfMass, gizmoSize);
            }
        }
    }
}