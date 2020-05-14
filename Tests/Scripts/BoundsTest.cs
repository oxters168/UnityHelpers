using UnityEngine;

namespace UnityHelpers
{
    public class BoundsTest : MonoBehaviour
    {
        public bool showLocalSingleBounds = true;
        public bool showLocalTotalBounds = true;
        public bool showGlobalSingleBounds = true;
        public bool showGlobalTotalBounds = true;
        public bool showLocalSingleColliderBounds = true;
        public bool showLocalTotalColliderBounds = true;
        public bool showGlobalSingleColliderBounds = true;
        public bool showGlobalTotalColliderBounds = true;

        private void OnDrawGizmos()
        {
            if (showGlobalSingleBounds)
            {
                var globalBounds = transform.GetBounds(Space.World);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(globalBounds.center, globalBounds.size);
            }
            if (showGlobalTotalBounds)
            {
                var totalGlobalBounds = transform.GetTotalBounds(Space.World);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(totalGlobalBounds.center, totalGlobalBounds.size);
            }
            if (showGlobalSingleColliderBounds)
            {
                var singleGlobalColliderBounds = transform.GetBounds(Space.World, true);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(singleGlobalColliderBounds.center, singleGlobalColliderBounds.size);
            }
            if (showGlobalTotalColliderBounds)
            {
                var totalGlobalColliderBounds = transform.GetTotalBounds(Space.World, true);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(totalGlobalColliderBounds.center, totalGlobalColliderBounds.size);
            }


            Gizmos.matrix = transform.localToWorldMatrix;
            if (showLocalSingleBounds)
            {
                var localBounds = transform.GetBounds(Space.Self);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(localBounds.center, localBounds.size);
            }
            if (showLocalTotalBounds)
            {
                var totalLocalBounds = transform.GetTotalBounds(Space.Self);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(totalLocalBounds.center, totalLocalBounds.size);
            }
            if (showLocalSingleColliderBounds)
            {
                var singleLocalColliderBounds = transform.GetBounds(Space.Self, true);
                Gizmos.color = Color.black;
                Gizmos.DrawWireCube(singleLocalColliderBounds.center, singleLocalColliderBounds.size);
            }
            if (showLocalTotalColliderBounds)
            {
                var totalLocalColliderBounds = transform.GetTotalBounds(Space.Self, true);
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(totalLocalColliderBounds.center, totalLocalColliderBounds.size);
            }
        }
    }
}