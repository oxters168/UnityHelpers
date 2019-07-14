using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
    public class CompoundCollision : MonoBehaviour
    {
        private Dictionary<GameObject, int> hits = new Dictionary<GameObject, int>();
        public event CollisionHandler onCollisionExit;
        public delegate void CollisionHandler(Collision col);

        private void OnCollisionEnter(Collision col)
        {
            if (col.rigidbody)
            {
                if (!hits.ContainsKey(col.gameObject))
                    hits[col.gameObject] = 0;
                hits[col.gameObject]++;
            }
        }
        private void OnCollisionExit(Collision col)
        {
            if (col.rigidbody)
            {
                hits[col.gameObject]--;
                Debug.Assert(hits[col.gameObject] >= 0);
                if (hits[col.gameObject] <= 0)
                {
                    onCollisionExit?.Invoke(col);
                    hits.Remove(col.gameObject);
                }
            }
        }
    }
}