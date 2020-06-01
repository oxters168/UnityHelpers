using UnityEngine;

namespace UnityHelpers
{
    [System.Serializable]
    public class SpherecastInfo : ICastable
    {
        public LayerMask castMask = ~0;
        public Vector3 position;
        public Vector3 direction;
        public float distance;
        public float radius;
        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        public bool Cast(out RaycastHit hitInfo)
        {
            return Physics.SphereCast(position, radius, direction, out hitInfo, distance, castMask, queryTriggerInteraction);
        }
        public RaycastHit[] CastAll()
        {
            return Physics.SphereCastAll(position, radius, direction, distance, castMask, queryTriggerInteraction);
        }
        public Vector3 GetPosition()
        {
            return position;
        }
        public Vector3 GetDirection()
        {
            return direction;
        }
        public float GetSize()
        {
            return radius;
        }
    }
}