using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private Transform poolParent;
    private bool worldPositionStays = true;

    private T objectPrefab;
    private int objectIndex = 0;
    private Dictionary<int, T> objectPool = new Dictionary<int, T>();

    private List<int> availableObjects = new List<int>();
    private List<int> unavailableObjects = new List<int>();

    public bool reuseObjectsInUse;

    public ObjectPool(T objectPrefab, int poolSize = 5, bool reuseObjectsInUse = true, Transform poolParent = null, bool worldPositionStays = true)
    {
        SetParent(poolParent, worldPositionStays);

        this.objectPrefab = objectPrefab;
        AdjustPoolSize(poolSize);

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

    public T Get(bool setPosition = false, Vector3 position = new Vector3(), bool localPosition = false, bool setRotation = false, Quaternion rotation = new Quaternion(), bool localRotation = false, bool setScale = false, Vector3 scale = new Vector3())
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
            else
                AdjustPoolSize(objectPool.Count + 5);
        }

        if (fromUnused)
            availableObjects.Remove(availableIndex);
        unavailableObjects.Add(availableIndex);

        T availableObject = (T)objectPool[availableIndex];

        Transform tempTransform = availableObject.transform;
        if (setPosition)
        {
            if (localPosition)
                tempTransform.localPosition = position;
            else
                tempTransform.position = position;

        }
        if (setRotation)
        {
            if (localRotation)
                tempTransform.localRotation = rotation;
            else
                tempTransform.rotation = rotation;
        }
        if (setScale)
            tempTransform.localScale = scale;

        availableObject.gameObject.SetActive(true);

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
