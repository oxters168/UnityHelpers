using UnityEngine;

namespace UnityHelpers
{
    public class GrabbableTransform : GrabbableBase
    {
        /// <summary>
        /// When true will use MovePosition and MoveRotation in Rigidbody. When false will set transform position and rotation.
        /// </summary>
        [Tooltip("When true will use MovePosition and MoveRotation in Rigidbody. When false will set transform position and rotation.")]
        public bool physicsBased;
        private Rigidbody AffectedBody { get { if (_affectedBody == null) _affectedBody = GetComponentInParent<Rigidbody>(); return _affectedBody; } }
        private Rigidbody _affectedBody;

        protected override void ApplyPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (!physicsBased)
            {
                transform.position = position;
                transform.rotation = rotation;
            }
            else if (AffectedBody != null)
            {
                AffectedBody.MovePosition(position);
                AffectedBody.MoveRotation(rotation);
            }
            else
                Debug.LogError("GrabbableTransform: Could not find rigidbody on " + gameObject.name + " or any of its parents");
        }
    }
}