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
        public float distance = 1;
        public QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal;
        public bool raycastHit;
        public RaycastHit[] hitData;

        public bool Cast()
        {
            Vector3 castPosition = position;
            Vector3 castDirection = direction;
            if (parent != null)
            {
                castPosition = parent.TransformPoint(position);
                castDirection = parent.TransformDirection(direction);
            }

            RaycastHit hitInfo;
            raycastHit = Physics.Raycast(castPosition, castDirection, out hitInfo, distance, castMask, queryTriggerInteraction);
            if (raycastHit)
            {
                hitData = new RaycastHit[1];
                hitData[0] = hitInfo;
            }
            return raycastHit;
        }
        public bool Cast(out RaycastHit hitInfo)
        {
            hitInfo = default;
            Cast();
            if (hitData != null && hitData.Length > 0)
                hitInfo = hitData[0];
            return raycastHit;
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

            hitData = Physics.RaycastAll(castPosition, castDirection, distance, castMask, queryTriggerInteraction);
            raycastHit = hitData != null && hitData.Length > 0;
            return hitData;
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