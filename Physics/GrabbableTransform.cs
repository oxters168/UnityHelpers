using UnityEngine;

namespace UnityHelpers
{
    public class GrabbableTransform : GrabbableBase
    {
        protected override void ApplyPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
    }
}