using UnityEngine;

namespace UnityHelpers
{
    [CreateAssetMenu(fileName = "Entity", menuName = "UnityHelpers/Prefab Pool", order = 8)]
    public class PrefabPool : ScriptableObject
    {
        [Tooltip("This name works as a way of identifying each entity, in some cases as a key")]
        public string prefabName;
        [Tooltip("The prefab that will be duplicated")]
        public Component prefab;
        [Space(10), Tooltip("How large the object pool will be when first created")]
        public int poolSize = 5;
        [Tooltip("Whether the object pool should cycle through enabled and disabled instances")]
        public bool reuseObjectsInUse = false;
        [Tooltip("Should the object pool increase in size when requesting to spawn an entity and no disabled instances exist in the pool (only applicable if reuseObjectsInUse is set to false)")]
        public bool dynamicSize = true;
        [Space(10)]
        public Vector3 spawnOffset;
    }
}