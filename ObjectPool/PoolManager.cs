using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour
{
    private static PoolManager poolManagerInScene;
    public PoolInfo[] pools;
    private Dictionary<string, ObjectPool<Transform>> storedPools = new Dictionary<string, ObjectPool<Transform>>();

    private void Awake()
    {
        poolManagerInScene = this;
        foreach(PoolInfo pool in pools)
        {
            bool doesNotLearn = !storedPools.ContainsKey(pool.poolName);
            Debug.Assert(doesNotLearn);
            if (doesNotLearn)
                storedPools.Add(pool.poolName, new ObjectPool<Transform>(pool.poolPrefab, pool.poolSize, pool.reuseObjectsInUse, pool.poolParent, pool.worldPositionStays));
        }
    }

    public static ObjectPool<Transform> GetPool(string poolName)
    {
        bool hasKey = poolManagerInScene.storedPools.ContainsKey(poolName);
        Debug.Assert(hasKey);
        return hasKey ? poolManagerInScene.storedPools[poolName] : null;
    }
}
