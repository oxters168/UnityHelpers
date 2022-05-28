using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UnityHelpers
{
    /// <summary>
    /// This class is used to make a pool of objects that can be reused rather than constantly destroying and instantiating
    /// </summary>
    /// <typeparam name="T">The main component type to work with</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private Transform poolParent;
        private bool worldPositionStays = true;
        public int activeCount { get { return unavailableObjects.Count; } }

        internal T objectPrefab { get; private set; }
        private int objectIndex = 0;
        private Dictionary<int, T> objectPool = new Dictionary<int, T>();

        private List<int> availableObjects = new List<int>();
        private List<int> unavailableObjects = new List<int>();

        public bool reuseObjectsInUse, dynamicSize;

        /// <summary>
        /// Object pool constructor
        /// </summary>
        /// <param name="objectPrefab">The prefab to instantiate from</param>
        /// <param name="poolSize">The initial number of objects in the pool</param>
        /// <param name="reuseObjectsInUse">If true will reuse objects already active starting from the oldest</param>
        /// <param name="dynamicSize">If the pool is not reusing objects and runs out of disabled objects, should the pool increase its size automatically?</param>
        /// <param name="poolParent">Will set the parent of the objects in the pool to this</param>
        /// <param name="worldPositionStays">Sets world position stays when parenting pool objects</param>
        public ObjectPool(T objectPrefab, int poolSize = 5, bool reuseObjectsInUse = true, bool dynamicSize = false, Transform poolParent = null, bool worldPositionStays = true)
        {
            SetParent(poolParent, worldPositionStays);

            this.objectPrefab = objectPrefab;
            AdjustPoolSize(poolSize);

            this.dynamicSize = dynamicSize;
            this.reuseObjectsInUse = reuseObjectsInUse;
        }

        private void AdjustPoolSize(int newSize)
        {
            int sizeDiff = newSize - objectPool.Count;
            for (int i = 0; i < Mathf.Abs(sizeDiff); i++)
            {
                int currentObjectIndex;
                if (sizeDiff < 0)
                {
                    currentObjectIndex = objectPool.First().Key;
                    if (availableObjects.Count > 0)
                        currentObjectIndex = availableObjects.First();

                    UnityEngine.Object.Destroy(objectPool[currentObjectIndex]);
                    objectPool.Remove(currentObjectIndex);

                    availableObjects.Remove(currentObjectIndex);
                    unavailableObjects.Remove(currentObjectIndex);
                }
                else
                {
                    currentObjectIndex = objectIndex++;

                    objectPool.Add(currentObjectIndex, UnityEngine.Object.Instantiate(objectPrefab));
                    objectPool[currentObjectIndex].transform.SetParent(poolParent, worldPositionStays);
                    objectPool[currentObjectIndex].gameObject.SetActive(false);

                    availableObjects.Add(currentObjectIndex);
                }
            }
        }

        public T[] GetActiveObjects()
        {
            return unavailableObjects.Select(index => objectPool[index]).ToArray();
        }
        public T[] GetInactiveObjects()
        {
            return availableObjects.Select(index => objectPool[index]).ToArray();
        }
        public G Get<G>(Action<G> action = null) where G : Component
        {
            G componentOnObject = null;
            Get((poolObject) =>
            {
                componentOnObject = poolObject?.GetComponent<G>();
                if (componentOnObject != null)
                    action?.Invoke(componentOnObject);
            });
            return componentOnObject;
        }
        public T Get(Action<T> action = null)
        {
            int availableIndex = objectIndex;
            bool fromUnused = true;
            if (availableObjects.Count > 0)
                availableIndex = availableObjects.First();
            else
            {
                if (reuseObjectsInUse)
                {
                    fromUnused = false;
                    availableIndex = unavailableObjects.First();
                    unavailableObjects.Remove(availableIndex);
                }
                else if (dynamicSize)
                    AdjustPoolSize(objectPool.Count + 5);
            }

            T availableObject = null;

            if (availableObjects.Count > 0 || !fromUnused)
            {
                if (fromUnused)
                    availableObjects.Remove(availableIndex);
                unavailableObjects.Add(availableIndex);

                availableObject = objectPool[availableIndex];

                action?.Invoke(availableObject);

                availableObject.gameObject.SetActive(true);
            }

            return availableObject;
        }
        public void Return(T poolObject)
        {
            if (poolObject != null)
            {
                KeyValuePair<int, T> resultPair = objectPool.FirstOrDefault(pair => pair.Value == poolObject);
                if (resultPair.Value != null)
                {
                    int objectIndex = resultPair.Key;
                    Return(objectIndex);
                }
            }
        }
        private void Return(int objectIndex)
        {
            unavailableObjects.Remove(objectIndex);
            availableObjects.Add(objectIndex);
            objectPool[objectIndex].transform.SetParent(poolParent, worldPositionStays);
            objectPool[objectIndex].gameObject.SetActive(false);
        }

        public bool Has(T poolObject)
        {
            return objectPool.ContainsValue(poolObject);
        }
        public void ReturnAll()
        {
            for (int i = unavailableObjects.Count - 1; i >= 0; i--)
            {
                int objectIndex = unavailableObjects[i];
                availableObjects.Add(objectIndex);
                unavailableObjects.RemoveAt(i);
                objectPool[objectIndex].transform.SetParent(poolParent, worldPositionStays);
                objectPool[objectIndex].gameObject.SetActive(false);
            }
        }

        public void SetParent(Transform parent)
        {
            SetParent(parent, true);
        }
        public void SetParent(Transform parent, bool worldPositionStays)
        {
            poolParent = parent;
            this.worldPositionStays = worldPositionStays;

            foreach (KeyValuePair<int, T> pair in objectPool)
                pair.Value.transform.SetParent(poolParent, this.worldPositionStays);
        }
    }
}