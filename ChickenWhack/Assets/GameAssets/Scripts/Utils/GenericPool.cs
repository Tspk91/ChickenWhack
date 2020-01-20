// Copyright (c) 2020 Alejandro Martín Carrillo, All rights reserved.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic pool of unity components.
/// </summary>
public class GenericPool<T> where T : Component
{
    public T prefab;
    
    //This contains all objects in pools, note this makes pool instance members exclusive to that pool instance
    private static Dictionary<T, PoolRef> registeredObjects = new Dictionary<T, PoolRef>();

    private List<T> pooledObjects = new List<T>();

    public GenericPool(T prefab, int preloadAmount)
    {
        this.prefab = prefab;

        for(int i = 0; i < preloadAmount; i++)
        {
            pooledObjects.Add(CreateObject());
        }
    }

    /// <summary>
    /// Returns all objects to the pool.
    /// </summary>
    public void ClearObjects()
    {
        ClearObjects(null);
    }

    /// <summary>
    /// Returns all objects to the pool executing a delegate first for each returnee.
    /// </summary>
    public void ClearObjects(System.Action<T> clearFunc)
    {
        foreach(var kvp in registeredObjects)
        {
            T obj = kvp.Key;
            if(!pooledObjects.Contains(obj))
            {
                clearFunc?.Invoke(obj);
                ReturnObject(obj);
            }
        }
    }

    /// <summary>
    /// Gets an object from the pool or creates one if empty. If despawnTime is positive, the object will return automatically.
    /// </summary>
    public T GetObject(float despawnTime = -1f)
    {
        if (pooledObjects.Count > 0)
        {
            T obj = pooledObjects[pooledObjects.Count - 1];
            pooledObjects.RemoveAt(pooledObjects.Count - 1);

            if (despawnTime > 0f)
                registeredObjects[obj].SetReturnTime(despawnTime);

            return obj;
        }

        T newObj = CreateObject();

        if (despawnTime > 0f)
            registeredObjects[newObj].SetReturnTime(despawnTime);

        return newObj;
    }

    public static void ReturnObject(T obj)
    {
        registeredObjects[obj].ReturnToPool();
    }

    private void ReturnObjectInternal(T obj)
    {
        if (!pooledObjects.Contains(obj))
            pooledObjects.Add(obj);
    }

    private T CreateObject()
    {
        T newObj = Object.Instantiate(prefab.gameObject).GetComponent<T>();
        PoolRef poolRef = new PoolRef(this, newObj);

        registeredObjects.Add(newObj, poolRef);

        return newObj;
    }

    /// <summary>
    /// Auxilliary class to handle each pooled object references
    /// </summary>
    private class PoolRef
    {
        private GenericPool<T> pool;
        private T component;

        private uint invokeHandle;

        public PoolRef(GenericPool<T> pool, T component)
        {
            this.pool = pool;
            this.component = component;
        }

        public void SetReturnTime(float returnTime = -1f)
        {
            if (returnTime > 0f)
            {
                invokeHandle = this.DelayedAction(ReturnToPool, returnTime);
            }
        }

        public void ReturnToPool()
        {
            this.CancelAction(invokeHandle);

            pool.ReturnObjectInternal(component);
        }
    }
}