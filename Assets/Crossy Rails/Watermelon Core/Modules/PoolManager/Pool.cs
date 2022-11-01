using UnityEngine;
using System;
using System.Collections.Generic;

//Pool module v 1.5.1

/// <summary>
/// Basic pool class. Contains pool settings and references to pooled objects.
/// </summary>
[Serializable]
public class Pool
{
    /// <summary>
    /// Pool name, use it get pool reference at PoolManager.
    /// </summary>
    public string name;

    /// <summary>
    /// Reference to pool prefab.
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// List of multiple pool prefabs.
    /// </summary>
    public List<MultiPoolPrefab> prefabsList = new List<MultiPoolPrefab>();
    /// <summary>
    /// Number of objects which be created be deffault.
    /// </summary>
    public int poolSize = 10;
    /// <summary>
    /// True means: if there is no available object, the new one will be added to a pool.
    /// Otherwise will be returned null.
    /// </summary>
    public bool willGrow = true;
    /// <summary>
    /// Type of pool.
    /// Single - classic pool with one object. Multiple - pool with multiple objects returned randomly using weights.
    /// </summary>
    public PoolType poolType = PoolType.Single;
    /// <summary>
    /// Custom objects container for this pool's objects.
    /// </summary>
    public Transform objectsContainer;
    /// <summary>
    /// List of pooled objects.
    /// </summary>
    public List<GameObject> pooledObjects;
    /// <summary>
    /// List of pooled objects for multiple pull.
    /// </summary>
    public List<List<GameObject>> multiPooledObjects;

#if UNITY_EDITOR
    /// <summary>
    /// Number of objects that where active at one time.
    /// </summary>
    public int maxItemsUsedInOneTime = 0;
#endif

    public enum PoolType
    {
        Single = 0,
        Multi = 1,
    }

    [System.Serializable]
    public struct MultiPoolPrefab
    {
        public GameObject prefab;
        public int weight;
        public bool isWeightLocked;

        public MultiPoolPrefab(GameObject prefab, int weight, bool isWeightLocked)
        {
            this.prefab = prefab;
            this.weight = weight;
            this.isWeightLocked = isWeightLocked;
        }
    }

    /// <summary>
    /// Basic constructor.
    /// </summary>
    public Pool()
    {
        name = string.Empty;
    }

    /// <summary>
    /// Returns reference to pooled object if it's currently available.
    /// </summary>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
    public GameObject GetPooledObject(bool activateObject = true)
    {
        return GetPooledObject(true, activateObject, false, Vector3.zero);
    }

    /// <summary>
    /// Returns reference to pooled object if it's currently available.
    /// </summary>
    /// <param name="position">Sets object to specified position.</param>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
    public GameObject GetPooledObject(Vector3 position, bool activateObject = true)
    {
        return GetPooledObject(true, activateObject, true, position);
    }

    /// <summary>
    /// Returns reference to pooled object if it's currently available.
    /// </summary>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
    public GameObject GetHierarchyPooledObject(bool activateObject = true)
    {
        return GetPooledObject(false, activateObject, false, Vector3.zero);
    }

    /// <summary>
    /// Returns reference to pooled object if it's currently available.
    /// </summary>
    /// <param name="position">Sets object to specified position.</param>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <returns>Pooled object or null if there is no available objects and new one can not be created.</returns>
    public GameObject GetHierarchyPooledObject(Vector3 position, bool activateObject = true)
    {
        return GetPooledObject(false, activateObject, true, position);
    }

    /// <summary>
    /// Internal implementation of GetPooledObject and GetHierarchyPooledObject methods.
    /// </summary>
    /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <param name="position">Sets object to specified position.</param>
    /// <returns></returns>
    private GameObject GetPooledObject(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
    {
        if (poolType == PoolType.Single)
        {
            return GetPooledObjectSingleType(checkTypeActiveSelf, activateObject, setPosition, position);
        }
        else
        {
            return GetPooledObjectMultiType(checkTypeActiveSelf, activateObject, setPosition, position);
        }
    }

    /// <summary>
    /// Internal implementation of GetPooledObject and GetHierarchyPooledObject methods for Single type pool.
    /// </summary>
    /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <param name="position">Sets object to specified position.</param>
    /// <returns></returns>
    private GameObject GetPooledObjectSingleType(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
    {
#if UNITY_EDITOR
        if (PoolManager.instance.useCache)
        {
            int itemsUsed = 0;

            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (checkTypeActiveSelf ? pooledObjects[i].activeSelf : pooledObjects[i].activeInHierarchy)
                {
                    itemsUsed++;
                }
            }

            if (willGrow)
            {
                itemsUsed++; // adding one extra item which will be returned below 
            }

            if (itemsUsed > maxItemsUsedInOneTime)
            {
                maxItemsUsedInOneTime = itemsUsed;
            }
        }
#endif

        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (checkTypeActiveSelf ? !pooledObjects[i].activeSelf : !pooledObjects[i].activeInHierarchy)
            {
                if (setPosition)
                {
                    pooledObjects[i].transform.position = position;
                }
                pooledObjects[i].SetActive(activateObject);

                return pooledObjects[i];
            }
        }

        if (willGrow)
        {
            GameObject inst = PoolManager.AddObjectToPoolSingleType(this);
            if (setPosition)
            {
                inst.transform.position = position;
            }
            inst.SetActive(activateObject);

            return inst;
        }

        return null;
    }

    /// <summary>
    /// Internal implementation of GetPooledObject and GetHierarchyPooledObject methods for Multi type pool.
    /// </summary>
    /// <param name="checkTypeActiveSelf">Which type of checking object's activation state is used: active self or active in hierarchy.</param>
    /// <param name="activateObject">If true object will be set as active.</param>
    /// <param name="position">Sets object to specified position.</param>
    /// <returns></returns>
    private GameObject GetPooledObjectMultiType(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
    {
        int randomPoolIndex = 0;
        bool randomValueWasInRange = false;
        int randomValue = UnityEngine.Random.Range(0, 101);
        int currentValue = 0;

        for (int i = 0; i < prefabsList.Count; i++)
        {
            currentValue += prefabsList[i].weight;

            if (randomValue <= currentValue)
            {
                randomPoolIndex = i;
                randomValueWasInRange = true;
                break;
            }
        }

        if (!randomValueWasInRange)
        {
            Debug.LogError("[Pool Manager] Random value(" + randomValue + ") is out of weights sum range at pool: \"" + name + "\"");
        }

        List<GameObject> objectsList = multiPooledObjects[randomPoolIndex];


#if UNITY_EDITOR
        if (PoolManager.instance.useCache)
        {
            int itemsUsed = 0;

            for (int i = 0; i < objectsList.Count; i++)
            {
                if (checkTypeActiveSelf ? objectsList[i].activeSelf : objectsList[i].activeInHierarchy)
                {
                    itemsUsed++;
                }
            }

            if (willGrow)
            {
                itemsUsed++; // adding one extra item which will be returned below 
            }

            if (itemsUsed > maxItemsUsedInOneTime)
            {
                maxItemsUsedInOneTime = itemsUsed;
            }
        }
#endif


        for (int i = 0; i < objectsList.Count; i++)
        {
            if (checkTypeActiveSelf ? !objectsList[i].activeSelf : !objectsList[i].activeInHierarchy)
            {
                if (setPosition)
                {
                    objectsList[i].transform.position = position;
                }
                objectsList[i].SetActive(activateObject);

                return objectsList[i];
            }
        }

        if (willGrow)
        {
            GameObject inst = PoolManager.AddObjectToPoolMultiType(this, randomPoolIndex);
            if (setPosition)
            {
                inst.transform.position = position;
            }
            inst.SetActive(activateObject);

            return inst;
        }

        return null;
    }

    /// <summary>
    /// Sets initial parrents to all objects.
    /// </summary>
    public void ResetParrents()
    {
        if (poolType == PoolType.Single)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                pooledObjects[i].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.ObjectsContainerTransform);
            }
        }
        else
        {
            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                for (int j = 0; j < multiPooledObjects[i].Count; j++)
                {
                    multiPooledObjects[i][j].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.ObjectsContainerTransform);
                }
            }
        }
    }

    /// <summary>
    /// Disables all active objects from this pool.
    /// </summary>
    /// <param name="resetParrent">Sets default parrent if checked.</param>
    public void ReturnToPoolEverything(bool resetParrent = false)
    {
        if (poolType == PoolType.Single)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (resetParrent)
                {
                    pooledObjects[i].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.ObjectsContainerTransform);
                }

                pooledObjects[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < multiPooledObjects.Count; i++)
            {
                for (int j = 0; j < multiPooledObjects[i].Count; j++)
                {
                    if (resetParrent)
                    {
                        multiPooledObjects[i][j].transform.SetParent(objectsContainer != null ? objectsContainer : PoolManager.ObjectsContainerTransform);
                    }
                    multiPooledObjects[i][j].SetActive(false);
                }
            }
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// Evenly distributes the weight between multi pooled objects, leaving locked weights as is.
    /// </summary>
    public void RecalculateWeights()
    {
        List<MultiPoolPrefab> oldPrefabsList = new List<MultiPoolPrefab>(prefabsList);
        prefabsList = new List<MultiPoolPrefab>();

        if (oldPrefabsList.Count > 0)
        {
            int totalUnlockedPoints = 100;
            int unlockedPrefabsAmount = oldPrefabsList.Count;

            for (int i = 0; i < oldPrefabsList.Count; i++)
            {
                if (oldPrefabsList[i].isWeightLocked)
                {
                    totalUnlockedPoints -= oldPrefabsList[i].weight;
                    unlockedPrefabsAmount--;
                }
            }

            if (unlockedPrefabsAmount > 0)
            {
                int averagePoints = totalUnlockedPoints / unlockedPrefabsAmount;
                int additionalPoints = totalUnlockedPoints - averagePoints * unlockedPrefabsAmount;

                for (int j = 0; j < oldPrefabsList.Count; j++)
                {
                    if (oldPrefabsList[j].isWeightLocked)
                    {
                        prefabsList.Add(oldPrefabsList[j]);
                    }
                    else
                    {
                        prefabsList.Add(new MultiPoolPrefab(oldPrefabsList[j].prefab, averagePoints + (additionalPoints > 0 ? 1 : 0), false));
                        additionalPoints--;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks are all prefabs references assigned.
    /// </summary>
    public bool IsAllPrefabsAssigned()
    {
        if (poolType == PoolType.Single)
        {
            return prefab != null;
        }
        else
        {
            if (prefabsList.Count == 0)
                return true;

            for (int i = 0; i < prefabsList.Count; i++)
            {
                if (prefabsList[i].prefab == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

#endif
}