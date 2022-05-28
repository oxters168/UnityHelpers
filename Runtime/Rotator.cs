using UnityEngine;

namespace UnityHelpers
{
    public class Rotator : MonoBehaviour
    {
        [Tooltip("The angle to be set on the specified axis")]
        public float angle;
        [Tooltip("The speed of change of the angle in revolutions/second")]
        public float rotSpeed;

        [Tooltip("The local axis to spin on")]
        public Vector3 spinAxis = Vector3.up;
        [Tooltip("A transform that will have it's forward set to spinAxis")]
        public Transform applyAxisOnTransform;

        private void Update()
        {
            spinAxis = spinAxis.normalized;

            if (applyAxisOnTransform != null)
                applyAxisOnTransform.forward = spinAxis;

            angle += rotSpeed * 360f * Time.deltaTime;
            Quaternion quatRot = Quaternion.AngleAxis(angle, spinAxis);
            transform.rotation = quatRot;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, spinAxis.normalized);
        }
    }
}