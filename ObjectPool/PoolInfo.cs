using System;
using UnityEngine;

[Serializable]
public class PoolInfo
{
    public string poolName;
    public Transform poolPrefab;
    public int poolSize = 5;
    public bool reuseObjectsInUse = true;
    public Transform poolParent;
    public bool worldPositionStays = true;
}
