using UnityEngine;
using System;
using System.Collections.Generic;

//Pool module v 1.5.1

namespace Watermelon
{
    /// <summary>
    /// Basic pool class. Contains pool settings and references to pooled objects.
    /// </summary>
    [Serializable]
    public class PoolSimple
    {
        /// <summary>
        /// Pool name, use it get reference to pool at PoolManager
        /// </summary>
        public string name;

        /// <summary>
        /// Reference to object which shood be pooled.
        /// </summary>
        [Space(5)]
        public GameObject objectToPool;
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
        /// Custom objects container for this pool's objects.
        /// </summary>
        public Transform objectsContainer;
        /// <summary>
        /// List of pooled objects.
        /// </summary>
        public List<GameObject> pooledObjects;

        /// <summary>
        /// Basic constructor.
        /// </summary>
        public PoolSimple()
        {
            name = string.Empty;
        }

        public PoolSimple(string name, GameObject prefab, int poolSize, bool willGrow, Transform container = null)
        {
            this.name = name;
            this.objectToPool = prefab;
            this.poolSize = poolSize;
            this.willGrow = willGrow;
            this.objectsContainer = container;

            InitializePool();
        }

        public void InitializePool()
        {
            pooledObjects = new List<GameObject>();

            if (objectToPool != null)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject inst = GameObject.Instantiate(objectToPool);

                    inst.SetActive(false);
                    pooledObjects.Add(inst);

                    // seting object parrent
                    if (objectsContainer != null)
                    {
                        inst.transform.SetParent(objectsContainer.transform);
                    }

                    InitGameObject(inst);
                }
            }
        }

        protected virtual void InitGameObject(GameObject prefab) { }

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
        /// <param name="checkTypeActiveSelf">Which tyep of checking object's activation state is used: active self or active in hierarchy.</param>
        /// <param name="activateObject">If true object will be set as active.</param>
        /// <param name="position">Sets object to specified position.</param>
        /// <returns></returns>
        private GameObject GetPooledObject(bool checkTypeActiveSelf, bool activateObject, bool setPosition, Vector3 position)
        {
            int pooledObjectIndex = GetPooledObjectIndex(checkTypeActiveSelf);
            if (pooledObjectIndex != -1)
            {
                pooledObjects[pooledObjectIndex].SetActive(activateObject);
                if (setPosition)
                    pooledObjects[pooledObjectIndex].transform.position = position;

                return pooledObjects[pooledObjectIndex];
            }

            return null;
        }

        protected int GetPooledObjectIndex(bool checkTypeActiveSelf)
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                if (checkTypeActiveSelf ? !pooledObjects[i].activeSelf : !pooledObjects[i].activeInHierarchy)
                {
                    return i;
                }
            }

            if (willGrow)
            {
                AddObjectToPool();

                return pooledObjects.Count - 1;
            }

            return -1;
        }

        /// <summary>
        /// Disables all active objects from this pool.
        /// </summary>
        public void ReturnToPoolEverything()
        {
            for (int i = 0; i < pooledObjects.Count; i++)
            {
                pooledObjects[i].SetActive(false);
            }
        }

        public GameObject AddObjectToPool()
        {
            GameObject inst = GameObject.Instantiate(objectToPool, objectsContainer != null ? objectsContainer.transform : null);
            inst.SetActive(false);

            pooledObjects.Add(inst);

            InitGameObject(inst);

            return inst;
        }
    }
}