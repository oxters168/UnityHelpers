using UnityEngine;

namespace UnityHelpers
{
    [System.Serializable]
    public class RaycastInfo : ICastable
    {
        public Transform parent;
        public LayerMask castMask = ~0;
        public Vector3 position;
        public Vector3 direction;
        public float distance;
        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;

        public bool Cast(out RaycastHit hitInfo)
        {
            Vector3 castPosition = position;
            Vector3 castDirection = direction;
            if (parent != null)
            {
                castPosition = parent.TransformPoint(position);
                castDirection = parent.TransformDirection(direction);
            }

            return Physics.Raycast(castPosition, castDirection, out hitInfo, distance, castMask, queryTriggerInteraction);
        }
        public RaycastHit[] CastAll()
        {
            Vector3 castPosition = position;
            Vector3 castDirection = direction;
            if (parent != null)
            {
                castPosition = parent.TransformPoint(position);
                castDirection = parent.TransformDirection(direction);
            }

            return Physics.RaycastAll(castPosition, castDirection, distance, castMask, queryTriggerInteraction);
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
            return distance;
        }
        public Transform GetParent()
        {
            return parent;
        }
    }
}