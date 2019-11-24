using System;
using UnityEngine;

namespace UnityHelpers
{
    [Serializable]
    public class PoolInfo
    {
        public string poolName;
        public Transform poolPrefab;
        public int poolSize = 5;
        public bool reuseObjectsInUse = true;
        public bool dynamicSize = false;
        public Transform poolParent;
        public bool worldPositionStays = true;
    }
}