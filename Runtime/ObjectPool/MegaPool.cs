using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityHelpers
{
    /// <summary>
    /// Oversees pools shared among different objects
    /// </summary>
    public class MegaPool
    {
        public static Transform poolsParent;
        private static Dictionary<string, ObjectPool<Component>> maga = new Dictionary<string, ObjectPool<Component>>();
        private static Func<Component, Component> defaultInit;
        private static Action<Component> defaultDestroy;

        public static Component Spawn(PrefabPool prefabPool)
        {
            return FindOrCreatePool(prefabPool).Get();
        }
        public static void Return(PrefabPool prefabPool, Component instance)
        {
            if (maga.ContainsKey(prefabPool.prefabName))
                maga[prefabPool.prefabName].Return(instance);
            else
                Debug.LogError("Could not return instance of non-existant " + prefabPool.prefabName + " pool");
        }
        public static void ReturnAll(PrefabPool entity)
        {
            if (maga.ContainsKey(entity.prefabName))
                maga[entity.prefabName].ReturnAll();
            else
                Debug.LogError("Could not call return all of non-existant " + entity.prefabName + " pool");
        }
        public static void SetDefaultInstantiator(Func<Component, Component> instantiator)
        {
            defaultInit = instantiator;
        }
        public static void SetDefaultDestroyer(Action<Component> destroyer)
        {
            defaultDestroy = destroyer;
        }
        public static void SetInstantiator(PrefabPool prefabPool, Func<Component, Component> instantiator)
        {
            FindOrCreatePool(prefabPool).SetInstantiator(instantiator);
        }
        public static void SetParent(PrefabPool prefabPool, Transform parent)
        {
            FindOrCreatePool(prefabPool).SetParent(parent);
        }
        public static void SetDestroyer(PrefabPool prefabPool, Action<Component> destroyer)
        {
            FindOrCreatePool(prefabPool).SetDestroyer(destroyer);
        }

        private static ObjectPool<Component> FindOrCreatePool(PrefabPool prefabPool)
        {
            ObjectPool<Component> entityPool;
            if (maga.ContainsKey(prefabPool.prefabName))
                entityPool = maga[prefabPool.prefabName];
            else
            {
                GameObject parent = new GameObject();
                parent.name = prefabPool.prefabName + "_Pool";
                if (poolsParent == null)
                    poolsParent = new GameObject("MegaPool").transform;
                parent.transform.SetParent(poolsParent.transform);
                entityPool = new ObjectPool<Component>(prefabPool.prefab, prefabPool.poolSize, prefabPool.reuseObjectsInUse, prefabPool.dynamicSize, parent.transform);
                if (defaultInit != null)
                    entityPool.SetInstantiator(defaultInit);
                if (defaultDestroy != null)
                    entityPool.SetDestroyer(defaultDestroy);
                maga[prefabPool.prefabName] = entityPool;
            }
            return entityPool;
        }
    }
}