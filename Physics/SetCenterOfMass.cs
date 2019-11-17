using UnityEngine;

namespace UnityHelpers
{
    [ExecuteInEditMode]
    public class SetCenterOfMass : MonoBehaviour
    {
        private Rigidbody body;

        [SerializeField, HideInInspector]
        private Vector3 centerOfMassOffset;
        private Vector3 originalCenterOfMass;
        [DraggablePoint(true)]
        public Vector3 centerOfMass;

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
            if (!Application.isPlaying)
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
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(body.worldCenterOfMass, 0.1f);
        }
    }
}