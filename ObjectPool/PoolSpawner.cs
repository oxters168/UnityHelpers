using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This lets you spawn in items at a rate using a pool
    /// </summary>
    public class PoolSpawner : MonoBehaviour
    {
        private ObjectPool<Transform> pool;
        [Tooltip("This is the name of the pool from the pool manager")]
        public string poolName;
        public Bounds spawnArea;
        public bool spawn;
        public float spawnMinRate = 2, spawnMaxRate = 5;
        private float prevSpawnTime, currentSpawnTime;
        public float minScale = 1, maxScale = 1;
        public uint maxConcurrentlySpawned = 5;

        //Until I figure out a way to make this better, this will have to do
        //For future me, this will help: https://answers.unity.com/questions/666127/how-do-i-generate-a-drop-down-list-of-functions-on.html
        public SpawnEvent onSpawn;

        private void Start()
        {
            pool = PoolManager.GetPool(poolName);
            SetNextSpawnTime();
        }
        private void Update()
        {
            if (spawn && Time.time - prevSpawnTime >= currentSpawnTime && pool.activeCount < maxConcurrentlySpawned)
            {
                var spawnedItem = pool.Get(spawned =>
                {
                    spawned.forward = transform.forward;
                    spawned.position = GetRandomSpawnPoint();
                    spawned.localScale = Vector3.one * Random.Range(minScale, maxScale);
                });
                if (spawnedItem != null)
                    onSpawn?.Invoke(spawnedItem, poolName);
                SetNextSpawnTime();
            }
        }

        public void ReturnAll()
        {
            pool?.ReturnAll();
        }
        private Vector3 GetRandomSpawnPoint()
        {
            Vector3 startCorner = transform.position - transform.forward * spawnArea.size.z / 2 - transform.right * spawnArea.size.x / 2 - transform.up * spawnArea.size.y / 2;
            float randomRight = Random.Range(0, spawnArea.size.x);
            float randomUp = Random.Range(0, spawnArea.size.y);
            return startCorner + transform.right * randomRight + transform.up * randomUp;
        }
        private void SetNextSpawnTime()
        {
            prevSpawnTime = Time.time;
            currentSpawnTime = Random.Range(spawnMinRate, spawnMaxRate);
        }
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var cubeMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix *= cubeMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnArea.size.x, spawnArea.size.y, spawnArea.size.z));
            Gizmos.matrix = oldMatrix;
            Vector3 startCorner = transform.position - transform.forward * spawnArea.size.z / 2 - transform.right * spawnArea.size.x / 2 - transform.up * spawnArea.size.y / 2;
            Gizmos.DrawSphere(startCorner, 1);
        }

        [System.Serializable]
        public class SpawnEvent : UnityEngine.Events.UnityEvent<Transform, string> { }
    }
}