using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    [DefaultExecutionOrder(-50)]
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager poolManagerInScene;
        public PoolInfo[] pools;
        private Dictionary<string, ObjectPool<Transform>> storedPools = new Dictionary<string, ObjectPool<Transform>>();

        private void Awake()
        {
            poolManagerInScene = this;
            foreach (PoolInfo pool in pools)
            {
                bool doesNotLearn = !storedPools.ContainsKey(pool.poolName);
                Debug.Assert(doesNotLearn);
                if (doesNotLearn)
                    storedPools.Add(pool.poolName, new ObjectPool<Transform>(pool.poolPrefab, pool.poolSize, pool.reuseObjectsInUse, pool.dynamicSize, pool.poolParent, pool.worldPositionStays));
            }
        }

        public static ObjectPool<Transform> GetPool(string poolName)
        {
            bool hasKey = poolManagerInScene.storedPools.ContainsKey(poolName);
            Debug.Assert(hasKey, "PoolManager: Could not find pool with the name '" + poolName + "'");
            return hasKey ? poolManagerInScene.storedPools[poolName] : null;
        }
    }
}