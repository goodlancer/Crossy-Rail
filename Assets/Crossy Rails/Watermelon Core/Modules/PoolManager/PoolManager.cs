#pragma warning disable 0414

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using Watermelon;

//Pool module v 1.5.1

/// <summary>
/// Class that manages all pool operations.
/// </summary>
public class PoolManager : MonoBehaviour
{
    /// <summary>
    /// Static referense to instance of a class.
    /// </summary>
    public static PoolManager instance;

    /// <summary>
    /// Empty object to store all pooled objects at scene (can be asigned manualy).
    /// </summary>
    public GameObject objectsContainer;

    /// <summary>
    /// List of all existing pools.
    /// </summary>
    public List<Pool> pools = new List<Pool>();

    /// <summary>
    /// When enabled PoolManager will automaticaly setup pooled objects amount using cashed data.
    /// </summary>
    public bool useCache = false;

    /// <summary>
    /// Dictionary which allows to acces Pool by name.
    /// </summary>
    private Dictionary<string, Pool> poolsDictionary;

    /// <summary>
    /// True when PoolManager is already initialized.
    /// </summary>
    private bool isInited;

    /// <summary>
    /// Amount of created objects.
    /// </summary>
    private int createdObjectsAmount = 0;

    private PoolManagerCache cache;

    public const string CACHE_FILE_NAME = "PoolManagerCache";

    private string currentCacheId = string.Empty;

    public static Transform ObjectsContainerTransform
    {
        get
        {
#if UNITY_EDITOR
            return instance.objectsContainer.transform;
#else
            return null;
#endif
        }
    }

    /// <summary>
    /// Initialize single instance of PoolManager.
    /// </summary>
    public static void InitSingletone()
    {
        PoolManager poolManager = FindObjectOfType<PoolManager>();

        if (poolManager != null)
        {
            poolManager.Init();

            instance = poolManager;
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogError("Please, add PoolManager behaviour at scene.");
        }
#endif
    }


    void Awake()
    {
        instance = this;

        Init();
    }


    /// <summary>
    /// Initialization of PoolManager.
    /// </summary>
    void Init()
    {
        if (instance == null)
            return;

#if UNITY_EDITOR
        if (objectsContainer == null)
        {
            objectsContainer = new GameObject("[PooledObjects]");
        }

        if (useCache)
        {
            LoadCache();
        }
#endif

        poolsDictionary = new Dictionary<string, Pool>();

        foreach (Pool pool in pools)
        {
            poolsDictionary.Add(pool.name, pool);
        }

        foreach (Pool pool in pools)
        {
            InitializePool(pool);
        }
    }


    /// <summary>
    /// Initializes pool.
    /// </summary>
    /// <param name="pool">Pool to initialize.</param>
    private void InitializePool(Pool pool)
    {
        if (pool.poolType == Pool.PoolType.Single)
        {
            InitializeSingleObjPool(pool);
        }
        else
        {
            InitializeMultiObjPool(pool);
        }
    }

    private void InitializeSingleObjPool(Pool pool)
    {
        pool.pooledObjects = new List<GameObject>();

        if (pool.prefab != null)
        {
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject inst = (GameObject)Instantiate(pool.prefab);

                inst.name += " " + createdObjectsAmount;
                createdObjectsAmount++;
                inst.SetActive(false);
                pool.pooledObjects.Add(inst);

                // seting object parrent
                if (pool.objectsContainer != null)
                {
                    inst.transform.SetParent(pool.objectsContainer.transform);
                }
#if UNITY_EDITOR
                else
                {
                    inst.transform.SetParent(objectsContainer.transform);
                }
#endif
            }
        }
        else
        {
            Debug.LogError("[PoolManager] There's no attached prefab at pool: \"" + pool.name + "\"");
        }
    }

    private void InitializeMultiObjPool(Pool pool)
    {
        pool.multiPooledObjects = new List<List<GameObject>>();

        for (int i = 0; i < pool.prefabsList.Count; i++)
        {
            List<GameObject> pooledObjects = new List<GameObject>();

            if (pool.prefabsList[i].prefab != null)
            {
                for (int j = 0; j < pool.poolSize; j++)
                {
                    GameObject inst = (GameObject)Instantiate(pool.prefabsList[i].prefab);

                    inst.name += " " + createdObjectsAmount;
                    createdObjectsAmount++;
                    inst.SetActive(false);
                    pooledObjects.Add(inst);

                    // seting object parrent
                    if (pool.objectsContainer != null)
                    {
                        inst.transform.SetParent(pool.objectsContainer.transform);
                    }
#if UNITY_EDITOR
                    else
                    {
                        inst.transform.SetParent(objectsContainer.transform);
                    }
#endif
                }
            }
            else
            {
                Debug.LogError("[PoolManager] There's not attached prefab at pool: \"" + pool.name + "\"");
            }

            pool.multiPooledObjects.Add(pooledObjects);
        }
    }

    /// <summary>
    /// Adds one more object to single type Pool.
    /// </summary>
    /// <param name="pool">Pool at which should be added new object.</param>
    /// <returns>Returns reference to just added object.</returns>
    public static GameObject AddObjectToPoolSingleType(Pool pool)
    {
        Transform parent = null;
        if (pool.objectsContainer != null)
        {
            parent = pool.objectsContainer.transform;
        }
#if UNITY_EDITOR
        else
        {
            parent = instance.objectsContainer.transform;
        }
#endif

        GameObject inst = (GameObject)Instantiate(pool.prefab, parent);
        instance.createdObjectsAmount++;
        inst.name += " e" + instance.createdObjectsAmount;
        inst.SetActive(false);
        pool.pooledObjects.Add(inst);

        return inst;
    }

    /// <summary>
    /// Adds one more object to multi type Pool.
    /// </summary>
    /// <param name="pool">Pool at which should be added new object.</param>
    /// <returns>Returns reference to just added object.</returns>
    public static GameObject AddObjectToPoolMultiType(Pool pool, int objectIndex)
    {
        Transform parent = null;
        if (pool.objectsContainer != null)
        {
            parent = pool.objectsContainer.transform;
        }
#if UNITY_EDITOR
        else
        {
            parent = instance.objectsContainer.transform;
        }
#endif

        GameObject inst = (GameObject)Instantiate(pool.prefabsList[objectIndex].prefab, parent);
        instance.createdObjectsAmount++;
        inst.name += " e" + instance.createdObjectsAmount;
        inst.SetActive(false);
        pool.multiPooledObjects[objectIndex].Add(inst);

        return inst;
    }


    /// <summary>
    /// Returns reference to Pool by it's name.
    /// </summary>
    /// <param name="poolName">Name of Pool which should be returned.</param>
    /// <returns>Reference to Pool.</returns>
    public static Pool GetPoolByName(string poolName)
    {
        if (instance == null)
        {
            InitSingletone();
        }

        if (instance.poolsDictionary.ContainsKey(poolName))
        {
            return instance.poolsDictionary[poolName];
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogError("[PoolManager] Not found pool with name: '" + poolName + "'");
#endif
            return null;
        }
    }



#if UNITY_EDITOR
#region Cache Management

    // //////////////////////////////////////////////////////////////////////////////////
    // New Chace system

    /// <summary>
    /// Loads cache from disc and initializing current scene's cache id.
    /// </summary>
    private void LoadCache()
    {
        cache = Serializer.DeserializeFromPDP<PoolManagerCache>(CACHE_FILE_NAME, logIfFileNotExists: false);

        string sceneMetaFile = Serializer.LoadTextFileAtPath(SceneManager.GetActiveScene().path + ".meta");

        int startIndex = sceneMetaFile.IndexOf("guid: ") + "guid: ".Length;
        int finalIndex = sceneMetaFile.LastIndexOf("timeCreated");

        currentCacheId = sceneMetaFile.Substring(startIndex, finalIndex - startIndex);
    }

    /// <summary>
    /// Updates cache after exit from play mode or creation of new pool.
    /// </summary>
    private void UpdateCache()
    {
        if (currentCacheId != "")
        {
            // true if there is no saved cache
            bool init = !cache.ContainsLevel(currentCacheId);

            List<PoolCache> newCache = new List<PoolCache>();

            for (int i = 0; i < pools.Count; i++)
            {
                // if we initializing cache we simple adding current pool info
                if (init)
                {
                    newCache.Add(new PoolCache(pools[i].name, pools[i].poolSize));
                }
                // if there is a cache, let's update this stuff
                else
                {
                    int index = cache.poolsCache[currentCacheId].FindIndex(x => x.poolName == pools[i].name);
                    if (index != -1)
                    {
                        // do not consider new data if it's (probably pool just was not used at all)
                        if (pools[i].maxItemsUsedInOneTime != 0)
                        {
                            cache.poolsCache[currentCacheId][index].UpdateSize(pools[i].maxItemsUsedInOneTime);
                        }

                        newCache.Add(cache.poolsCache[currentCacheId][index]);
                    }
                    else
                    {
                        newCache.Add(new PoolCache(pools[i].name, pools[i].poolSize));
                    }
                }
            }

            if (init)
            {
                cache.poolsCache.Add(currentCacheId, newCache);
            }
            else
            {
                cache.UpdateCache(currentCacheId, newCache);
            }

            Serializer.SerializeToPDP(cache, CACHE_FILE_NAME);
        }
        else
        {
            Debug.LogError("[PoolManager] Cache could not be updated. This level was not initialized.");
        }

    }

    private void OnDisable()
    {
        if (useCache)
        {
            UpdateCache();
        }
    }

    /// <summary>
    /// Updates and saves cache.
    /// </summary>
    public void SaveCache()
    {
        LoadCache();
        UpdateCache();
    }

#endregion
#endif
}

// Log
// v 1.0.0 
// Basic version of pool

// v 1.1.0 
// Added PoolManager editor

// v 1.2.1 
// Added cache system
// Fixed errors on build

// v 1.3.1 
// Added RandomPools system
// Added objectsContainer access property

// v 1.4.5
// Added editor changes save
// Updated cache system
// Added ability to ignore cache for required pools
// Fixed created object's names
// Core refactoring
// Editor UX improvements

// v 1.5.1
// Added Multi objects pool type
// Added drag n drop support



//ToDO
// Custom weights setup:
//  - weights validation
//  - weights lock in editor