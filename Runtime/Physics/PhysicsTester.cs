using UnityEngine;
#if UNITY_EDITOR
using UnityHelpers.Editor;
#endif

namespace UnityHelpers
{
    public class PhysicsTester : MonoBehaviour
    {
        private Rigidbody body;

        public bool showTrajectory;
        public float trajectoryTimestep = 0.2f;
        public float trajectoryTime = 1f;
        [Space(10)]
        public bool setVelocity;
        #if UNITY_EDITOR
        [DraggablePoint(true)]
        #endif
        public Vector3 velocity;

        void FixedUpdate()
        {
            if (GetBody() != null)
            {
                if (setVelocity)
                    GetBody().velocity = velocity;
            }
        }

        private void OnDrawGizmos()
        {
            if (GetBody() != null && showTrajectory)
            {
                Gizmos.color = Color.red;
                Vector3 currentPosition = GetBody().position;
                Vector3 nextPosition = currentPosition;
                for (int i = 1; i <= trajectoryTime / trajectoryTimestep; i++)
                {
                    nextPosition = GetBody().PredictPosition(trajectoryTimestep * i);
                    Gizmos.DrawLine(currentPosition, nextPosition);
                    currentPosition = nextPosition;
                }
            }
        }

        private Rigidbody GetBody()
        {
            if (body == null)
                body = GetComponent<Rigidbody>();
            return body;
        }
    }
}