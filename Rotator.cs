using UnityEngine;

namespace UnityHelpers
{
    public class Rotator : MonoBehaviour
    {
        [Tooltip("In rotations/second")]
        public float rotSpeed;

        [Tooltip("The local axis to spin on")]
        public Vector3 spinAxis = Vector3.up;

        private void Update()
        {
            float degrees = rotSpeed * 360f * Time.deltaTime;
            Quaternion quatRot = Quaternion.AngleAxis(degrees, spinAxis.normalized);
            transform.rotation = transform.rotation * quatRot;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawLine(Vector3.zero, spinAxis.normalized);
        }
    }
}