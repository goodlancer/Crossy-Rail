using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using UnityEditorInternal;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Watermelon.Core;
using Watermelon;

//Pool module v 1.5.1

[CustomEditor(typeof(PoolManager))]
sealed internal class PoolManagerEditor : Editor
{
    private List<Pool> poolsList = new List<Pool>();
    private List<int> poolsCacheDeltaList = new List<int>();
    private List<PoolCache> poolsCacheList = new List<PoolCache>();

    private SerializedProperty poolsListProperty;

    private PoolManager poolManagerRef;
    private Pool selectedPool;
    private Pool newPool = null;
    private Rect inspectorRect = new Rect();
    private Rect dragAndDropRect = new Rect();

    private bool isNameAllowed = true;
    private bool isNameAlreadyExisting = false;
    private bool showSettings = false;
    private bool dragAndDropActive = false;
    private bool skipEmptyNameWarning = false;

    private const string poolsListPropertyName = "pools";
    private const string RENAMING_EMPTY_STRING = "[PoolManager: empty]";

    private string searchText = string.Empty;
    private string prevNewPoolName = string.Empty;
    private string prevSelectedPoolName = string.Empty;
    private string lastRenamingName = string.Empty;

    private Color defaultColor;

    private GUIStyle boldStyle = new GUIStyle();
    private GUIStyle headerStyle = new GUIStyle();
    private GUIStyle bigHeaderStyle = new GUIStyle();
    private GUIStyle centeredTextStyle = new GUIStyle();
    private GUIStyle multiListLablesStyle = new GUIStyle();
    private GUIStyle dragAndDropBoxStyle = new GUIStyle();

    private GUIContent warningIconGUIContent;
    private GUIContent lockedIconGUIContent;
    private GUIContent unlockedIconGUIContent;


    private void OnEnable()
    {
        poolManagerRef = (PoolManager)target;

        poolsListProperty = serializedObject.FindProperty(poolsListPropertyName);

        lastRenamingName = RENAMING_EMPTY_STRING;
        selectedPool = null;
        newPool = null;

        ReloadPoolManager();
        InitStyles();

        Undo.undoRedoPerformed += UndoCallback;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoCallback;
    }

    private void UndoCallback()
    {
        UpdatePools();
    }

    public void InitStyles()
    {
        boldStyle.fontStyle = FontStyle.Bold;

        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 12;

        bigHeaderStyle.alignment = TextAnchor.MiddleCenter;
        bigHeaderStyle.fontStyle = FontStyle.Bold;
        bigHeaderStyle.fontSize = 14;

        centeredTextStyle.alignment = TextAnchor.MiddleCenter;

        multiListLablesStyle.fontSize = 8;
        multiListLablesStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f);

        Texture warningIconTexture = AssetDatabase.LoadAssetAtPath<Texture>(@"Assets\Watermelon Core\Plugins\Editor\Resources\UI\Sprites\Icons\icon_warning.png");
        warningIconGUIContent = new GUIContent(warningIconTexture);

        Texture lockedTexture = AssetDatabase.LoadAssetAtPath<Texture>(@"Assets\Watermelon Core\Plugins\Editor\Resources\UI\Sprites\Icons\icon_locked.png");
        lockedIconGUIContent = new GUIContent(lockedTexture);

        Texture unlockedTexture = AssetDatabase.LoadAssetAtPath<Texture>(@"Assets\Watermelon Core\Plugins\Editor\Resources\UI\Sprites\Icons\icon_unlocked.png");
        unlockedIconGUIContent = new GUIContent(unlockedTexture);


        defaultColor = GUI.contentColor;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (dragAndDropActive)
        {
            dragAndDropBoxStyle = GUI.skin.box;
            dragAndDropBoxStyle.alignment = TextAnchor.MiddleCenter;
            dragAndDropBoxStyle.fontStyle = FontStyle.Bold;
            dragAndDropBoxStyle.fontSize = 12;

            GUILayout.Box("Drag objects here", dragAndDropBoxStyle, GUILayout.Width(EditorGUIUtility.currentViewWidth - 21), GUILayout.Height(inspectorRect.size.y));
        }
        else
        {
            inspectorRect = EditorGUILayout.BeginVertical();


            // Control bar /////////////////////////////////////////////////////////////////////////////
            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (newPool == null)
            {
                EditorGUI.indentLevel++;

                showSettings = EditorGUILayout.Foldout(showSettings, "Settings");

                if (showSettings)
                {
                    EditorGUI.BeginChangeCheck();

                    // Cache support disabled currently
                    //poolManagerRef.useCache = EditorGUILayout.Toggle("Use cache :", poolManagerRef.useCache);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }

                    if (poolManagerRef.useCache)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Apply", EditorStyles.miniButtonLeft))
                        {
                            ApplyCache();
                        }
                        if (GUILayout.Button("Display", EditorStyles.miniButtonMid))
                        {
                            DisplayCacheState();
                        }
                        if (GUILayout.Button("Clear", EditorStyles.miniButtonRight))
                        {
                            ClearCurrentChache();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space();
                    }

                    EditorGUI.BeginChangeCheck();


                    poolManagerRef.objectsContainer = (GameObject)EditorGUILayout.ObjectField("Objects container: ", poolManagerRef.objectsContainer, typeof(GameObject), true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }

                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;


                if (GUILayout.Button("Add pool", GUILayout.Height(30)))
                {
                    skipEmptyNameWarning = true;
                    AddNewSinglePool();
                }
            }

            // Pool creation bar //////////////////////////////////////////////////////////////////////////
            if (newPool != null)
            {
                //EditorGUILayout.BeginVertical(GUI.skin.box);

                GUILayout.Space(3f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Pool creation:", headerStyle, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Cancel", GUILayout.Width(60)))
                {
                    CancelNewPoolCreation();

                    return;
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4f);

                DrawPool(newPool);

                GUILayout.Space(5f);

                if (GUILayout.Button("Confirm", GUILayout.Height(25)))
                {
                    ConfirmPoolCreation();

                    return;
                }

                GUILayout.Space(5f);
                //EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();


            // Pools displaying region /////////////////////////////////////////////////////////////////////

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Pool objects", headerStyle);

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();

            // searching
            searchText = EditorGUILayout.TextField(searchText, GUI.skin.FindStyle("ToolbarSeachTextField"));

            if (!string.IsNullOrEmpty(searchText))
            {
                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
                {
                    // Remove focus if cleared
                    searchText = "";
                    GUI.FocusControl(null);
                }
            }
            else
            {
                GUILayout.Button(GUIContent.none, GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty"));
            }

            if (EditorGUI.EndChangeCheck())
            {
                UpdatePools();
            }
            GUILayout.EndHorizontal();

            if (poolsList.Count == 0)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    EditorGUILayout.HelpBox("There's no pools.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Pool \"" + searchText + "\" is not found.", MessageType.Info);
                }
            }
            else
            {
                for (int i = 0; i < poolsList.Count; i++)
                {
                    Pool pool = poolsList[i];
                    Rect clickRect = EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUI.indentLevel++;
                    if (selectedPool == null || pool.name != selectedPool.name)
                    {
                        if (selectedPool != null)
                        {
                            CancelNewPoolCreation();
                        }

                        if (pool.IsAllPrefabsAssigned())
                        {
                            EditorGUILayout.LabelField(GetPoolName(i), centeredTextStyle);
                        }
                        else
                        {
                            EditorGUILayout.BeginHorizontal();

                            GUI.contentColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                            EditorGUILayout.LabelField(warningIconGUIContent, GUILayout.Width(30));
                            GUI.contentColor = defaultColor;

                            GUILayout.Space(-35f);
                            EditorGUILayout.LabelField(GetPoolName(i), centeredTextStyle);

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        GUILayout.Space(5);

                        // pool drawing ///////////
                        DrawPool(pool);

                        GUILayout.Space(5);

                        // cache system region ///////////
                        if (poolManagerRef.useCache && poolsCacheList[i] != null)
                        {
                            EditorGUI.BeginChangeCheck();
                            poolsCacheList[i].ignoreCache = EditorGUILayout.Toggle("Ignore cache: ", poolsCacheList[i].ignoreCache);

                            if (EditorGUI.EndChangeCheck())
                            {
                                UpdateIgnoreCacheStateOfPool(poolsCacheList[i].poolName, poolsCacheList[i].ignoreCache);
                                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            }
                        }

                        if (poolManagerRef.useCache && poolsCacheDeltaList[i] != 0 && poolsCacheList[i] != null)
                        {
                            if (poolsCacheList[i].ignoreCache)
                            {
                                GUI.enabled = false;
                                EditorGUILayout.LabelField("Cached value: " + poolsCacheList[i].poolSize);
                                GUI.enabled = true;
                            }
                            else
                            {
                                if (GUILayout.Button("Apply cache: " + (pool.poolSize + poolsCacheDeltaList[i])))
                                {
                                    Undo.RecordObject(target, "Apply cache");

                                    poolManagerRef.pools[i].poolSize = poolsCacheList[i].poolSize;

                                    ClearObsoleteCache();
                                    UpdateCacheStateList();
                                }
                            }
                        }

                        // delete button ///////////

                        if (GUILayout.Button("Delete"))
                        {
                            if (EditorUtility.DisplayDialog("This pool will be removed!", "Are you sure?", "Remove", "Cancel"))
                            {
                                DeletePool(pool);

                                EditorApplication.delayCall += delegate
                                {
                                    EditorUtility.FocusProjectWindow();
                                };
                            }
                        }

                        GUILayout.Space(5);
                    }

                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();

                    if (GUI.Button(clickRect, GUIContent.none, GUIStyle.none))
                    {
                        if (selectedPool == null || selectedPool != pool)
                        {
                            selectedPool = pool;
                            lastRenamingName = RENAMING_EMPTY_STRING;
                            newPool = null;
                        }
                        else
                        {
                            selectedPool = null;
                            lastRenamingName = RENAMING_EMPTY_STRING;
                        }
                    }
                }
            }

            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndVertical();

        }


        serializedObject.ApplyModifiedProperties();

        // Drag n Drop region /////////////////////////////////////////////////////////////////////

        Event currentEvent = Event.current;

        if (inspectorRect.Contains(currentEvent.mousePosition) && selectedPool == null && newPool == null)
        {
            if (currentEvent.type == EventType.DragUpdated)
            {
                dragAndDropActive = true;
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
            }
            else if (currentEvent.type == EventType.DragPerform)
            {
                dragAndDropActive = false;
                List<Pool.MultiPoolPrefab> draggedObjects = new List<Pool.MultiPoolPrefab>();

                foreach (UnityEngine.Object obj in DragAndDrop.objectReferences)
                {
                    if (obj.GetType() == typeof(GameObject))
                    {
                        draggedObjects.Add(new Pool.MultiPoolPrefab(obj as GameObject, 0, false));
                    }
                }

                if (draggedObjects.Count == 1)
                {
                    AddNewSinglePool(draggedObjects[0].prefab);
                }
                else
                {
                    AddNewMultiPool(draggedObjects);
                }

                currentEvent.Use();
            }
        }
        else
        {
            if (currentEvent.type == EventType.Repaint)
            {
                dragAndDropActive = false;
            }
        }

    }

    private void DrawPool(Pool pool)
    {
        EditorGUI.BeginChangeCheck();

        // name ///////////
        string newName = EditorGUILayout.TextField("Name: ", lastRenamingName != RENAMING_EMPTY_STRING ? lastRenamingName : pool.name);

        if (newName == pool.name)
        {
            lastRenamingName = RENAMING_EMPTY_STRING;
        }

        if (!isNameAllowed || newName == string.Empty || newName != pool.name || lastRenamingName != RENAMING_EMPTY_STRING)
        {
            if (newName != pool.name && newName != lastRenamingName)
            {
                lastRenamingName = newName;
            }

            if (IsNameAllowed(newName))
            {
                if (poolManagerRef.useCache)
                {
                    RenameCachedPool(pool.name, newName);
                }

                pool.name = newName;
                lastRenamingName = RENAMING_EMPTY_STRING;
            }
            else
            {
                if (isNameAlreadyExisting)
                {
                    EditorGUILayout.HelpBox("Name already exists", MessageType.Warning);
                }
                else
                {
                    if (!skipEmptyNameWarning)
                    {
                        EditorGUILayout.HelpBox("Name can't be empty", MessageType.Warning);
                    }
                }
            }
        }

        if(newPool != null)
        {
            newPool.name = newName;
        }

        // type ///////////
        pool.poolType = (Pool.PoolType)EditorGUILayout.EnumPopup("Pool type:", pool.poolType);

        // prefabs field ///////////
        if (pool.poolType == Pool.PoolType.Single)
        {
            // single prefab pool editor
            GameObject prefab = (GameObject)EditorGUILayout.ObjectField("Prefab: ", pool.prefab, typeof(GameObject), true);

            if (pool.prefab != prefab && pool.name == string.Empty)
            {
                pool.name = prefab.name;
            }
            pool.prefab = prefab;
        }
        else
        {
            // multiple prefabs pool editor
            GUILayout.Space(5f);

            int prefabsAmount = EditorGUILayout.IntField("Prefabs amount:", pool.prefabsList.Count);

            if (prefabsAmount != pool.prefabsList.Count)
            {
                if (prefabsAmount == 0)
                {
                    pool.prefabsList.Clear();
                }
                else if (prefabsAmount < pool.prefabsList.Count)
                {
                    int itemsToRemove = pool.prefabsList.Count - prefabsAmount;
                    pool.prefabsList.RemoveRange(pool.prefabsList.Count - itemsToRemove - 1, itemsToRemove);
                }
                else
                {
                    int itemsToAdd = prefabsAmount - pool.prefabsList.Count;
                    for (int j = 0; j < itemsToAdd; j++)
                    {
                        pool.prefabsList.Add(new Pool.MultiPoolPrefab());
                    }
                }

                pool.RecalculateWeights();
            }

            // prefabs list
            GUILayout.Space(-2f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("objects", multiListLablesStyle, GUILayout.MaxHeight(10f));
            GUILayout.Space(-25);
            EditorGUILayout.LabelField("weights", multiListLablesStyle, GUILayout.Width(75), GUILayout.MaxHeight(10f));
            EditorGUILayout.EndHorizontal();
            float weightsSum = 0f;

            for (int j = 0; j < pool.prefabsList.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();

                // object 
                GameObject prefab = (GameObject)EditorGUILayout.ObjectField(pool.prefabsList[j].prefab, typeof(GameObject), true);

                if (prefab != pool.prefabsList[j].prefab)
                {
                    pool.prefabsList[j] = new Pool.MultiPoolPrefab(prefab, pool.prefabsList[j].weight, pool.prefabsList[j].isWeightLocked);
                }

                // weight
                EditorGUI.BeginDisabledGroup(pool.prefabsList[j].isWeightLocked);
                int newWeight = EditorGUILayout.DelayedIntField(Math.Abs(pool.prefabsList[j].weight), GUILayout.Width(75));
                if (newWeight != pool.prefabsList[j].weight)
                {
                    pool.prefabsList[j] = new Pool.MultiPoolPrefab(prefab, newWeight, pool.prefabsList[j].isWeightLocked);
                }
                EditorGUI.EndDisabledGroup();
                weightsSum += newWeight;

                // lock
                GUI.contentColor = new Color(0.3f, 0.3f, 0.3f, 1f);
                if (GUILayout.Button(pool.prefabsList[j].isWeightLocked ? lockedIconGUIContent : unlockedIconGUIContent, centeredTextStyle, GUILayout.Height(13f), GUILayout.Width(13f)))
                {
                    pool.prefabsList[j] = new Pool.MultiPoolPrefab(prefab, pool.prefabsList[j].weight, !pool.prefabsList[j].isWeightLocked);
                }
                GUI.contentColor = defaultColor;

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(5f);

            if (pool.prefabsList.Count != 0 && weightsSum != 100)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.HelpBox("Weights sum should be 100 (current " + weightsSum + ").", MessageType.Warning);

                if (GUILayout.Button("Recalculate", GUILayout.Height(40f), GUILayout.Width(76)))
                {
                    pool.RecalculateWeights();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        if (!pool.IsAllPrefabsAssigned() && newPool == null)
        {
            EditorGUILayout.HelpBox("Please assign prefab reference.", MessageType.Warning);
        }


        // pool size ///////////
        int oldSize = pool.poolSize;

        if (pool.poolType == Pool.PoolType.Single)
        {
            pool.poolSize = EditorGUILayout.IntField("Pool size: ", pool.poolSize);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            pool.poolSize = EditorGUILayout.IntField("Pool size: ", pool.poolSize);
            GUILayout.FlexibleSpace();
            int multiPrefabsAmount = pool.prefabsList != null ? pool.prefabsList.Count : 0;
            string labelString = "x " + multiPrefabsAmount + " = " + (pool.poolSize * multiPrefabsAmount);
            GUILayout.Space(-18);
            EditorGUILayout.LabelField(labelString);

            EditorGUILayout.EndHorizontal();
        }

        if (pool.poolSize < 0)
        {
            pool.poolSize = 0;
        }

        if (poolManagerRef.useCache && oldSize != pool.poolSize)
        {
            UpdateCacheStateList();
        }

        // will grow toggle   |   objects parrent ///////////
        pool.willGrow = EditorGUILayout.Toggle("Will grow: ", pool.willGrow);
        pool.objectsContainer = (Transform)EditorGUILayout.ObjectField("Objects parrent", pool.objectsContainer, typeof(Transform), true);

        if (EditorGUI.EndChangeCheck())
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private void CancelNewPoolCreation()
    {
        newPool = null;
        lastRenamingName = RENAMING_EMPTY_STRING;
        isNameAllowed = true;
        skipEmptyNameWarning = false;
    }

    private string GetPoolName(int poolIndex)
    {
        string poolName = poolsList[poolIndex].name;

        if (poolManagerRef.useCache)
        {
            if (poolsCacheList.IsNullOrEmpty() || poolsCacheDeltaList.IsNullOrEmpty() || poolIndex > poolsCacheDeltaList.Count || poolIndex > poolsCacheList.Count)
            {
                UpdateCacheStateList();
            }

            // there is not cache for current scene returning
            if (poolsCacheList.IsNullOrEmpty())
            {
                return poolName;
            }

            int delta = poolsCacheDeltaList[poolIndex];

            if (poolsCacheList[poolIndex] != null && poolsCacheList[poolIndex].ignoreCache)
            {
                poolName += "   [cache ignored]";
            }
            else if (delta != 0)
            {
                poolName += "   " + CacheDeltaToState(delta);
            }
        }

        return poolName;
    }

    private void AddNewSinglePool(GameObject prefab = null)
    {
        selectedPool = null;
        newPool = new Pool();

        if (prefab != null)
        {
            newPool.prefab = prefab;
            newPool.name = prefab.name;

            IsNameAllowed(newPool.name);
        }
    }

    private void AddNewMultiPool(List<Pool.MultiPoolPrefab> prefabs = null)
    {
        selectedPool = null;
        newPool = new Pool();
        newPool.poolType = Pool.PoolType.Multi;

        if (prefabs != null && prefabs.Count != 0)
        {
            newPool.prefabsList = prefabs;
            newPool.name = prefabs[0].prefab.name;

            IsNameAllowed(newPool.name);
        }

        newPool.RecalculateWeights();
    }

    private void ConfirmPoolCreation()
    {
        skipEmptyNameWarning = false;

        if (IsNameAllowed(newPool.name))
        {
            Undo.RecordObject(target, "Add pool");

            poolManagerRef.pools.Add(newPool);

            if (poolManagerRef.useCache)
            {
                poolManagerRef.SaveCache();
            }

            ReloadPoolManager(true);
            newPool = null;
            prevNewPoolName = string.Empty;

            searchText = "";
        }
    }

    private void DeletePool(Pool poolToDelete)
    {
        Undo.RecordObject(target, "Remove");

        poolManagerRef.pools.Remove(poolToDelete);
        selectedPool = null;
        lastRenamingName = RENAMING_EMPTY_STRING;

        ReloadPoolManager();
    }

    private bool IsNameAllowed(string nameToCheck)
    {
        if (nameToCheck.Equals(string.Empty))
        {
            isNameAllowed = false;
            isNameAlreadyExisting = false;
            return false;
        }

        if (poolManagerRef.pools.IsNullOrEmpty())
        {
            isNameAllowed = true;
            isNameAlreadyExisting = false;
            return true;
        }

        if (poolManagerRef.pools.Find(x => x.name.Equals(nameToCheck)) != null)
        {
            isNameAllowed = false;
            isNameAlreadyExisting = true;
            return false;
        }
        else
        {
            isNameAllowed = true;
            isNameAlreadyExisting = false;
            return true;
        }
    }

    private void ReloadPoolManager(bool sortPool = false)
    {
        poolsList.Clear();

        UpdatePools(sortPool);

        UpdateCacheStateList();
    }

    private void UpdatePools(bool needToSort = false)
    {
        if (needToSort)
        {
            poolManagerRef.pools.Sort((x, y) => x.name.CompareTo(y.name));
        }

        if (poolManagerRef.pools != null)
        {
            poolsList = poolManagerRef.pools.FindAll(x => x.name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    #region Cache management

    private void ApplyCache()
    {
        List<PoolCache> currentLevelCache = LoadCurrentCache();

        if (!currentLevelCache.IsNullOrEmpty())
        {
            Undo.RecordObject(target, "Apply cache");

            for (int i = 0; i < poolManagerRef.pools.Count; i++)
            {
                int index = currentLevelCache.FindIndex(x => x.poolName == poolManagerRef.pools[i].name);
                if (index != -1 && !currentLevelCache[index].ignoreCache)
                {
                    poolManagerRef.pools[i].poolSize = currentLevelCache[index].poolSize;
                }
            }

            ClearObsoleteCache();
            UpdateCacheStateList();
        }
        else
        {
            Debug.Log("[PoolManager] There's no saved cache for current scene.");
        }
    }


    private void DisplayCacheState()
    {
        List<PoolCache> currentLevelCache = LoadCurrentCache();

        if (!currentLevelCache.IsNullOrEmpty())
        {
            List<PoolCache> cacheToDelete = new List<PoolCache>();

            string cacheInfo = string.Empty;
            foreach (PoolCache poolCache in currentLevelCache)
            {
                // if pool not exists - delete it's cache
                int index = poolManagerRef.pools.FindIndex(x => x.name == poolCache.poolName);
                if (index == -1)
                {
                    cacheToDelete.Add(poolCache);
                }
                // otherwise adding pool and cache stats to log
                else
                {
                    cacheInfo += poolCache.poolName + "\tcurrent size: " + poolManagerRef.pools[index].poolSize + "\tcached size: " + poolCache.poolSize + "\t(updates count: " + poolCache.updatesCount + ")\n";
                }
            }

            // deleting all obsolete cache
            if (cacheToDelete.Count > 0)
            {
                if (cacheInfo != string.Empty)
                {
                    cacheInfo += "\n";
                }

                foreach (PoolCache currentCache in cacheToDelete)
                {
                    currentLevelCache.Remove(currentCache);
                    cacheInfo += "deleted cache for unexisting pool: \"" + currentCache.poolName + "\"\n";
                }

                PoolManagerCache allCache = LoadAllCache();

                allCache.UpdateCache(GetCurrentCacheId(), currentLevelCache);
                Serializer.SerializeToPDP(allCache, PoolManager.CACHE_FILE_NAME);
            }

            Debug.Log(cacheInfo);
        }
        else
        {
            Debug.Log("[PoolManager] There's no saved cache for current scene.");
        }
    }

    private void ClearObsoleteCache()
    {
        List<PoolCache> currentLevelCache = LoadCurrentCache();

        if (currentLevelCache != null)
        {
            List<PoolCache> cacheToDelete = new List<PoolCache>();

            foreach (PoolCache poolCache in currentLevelCache)
            {
                // if pool not exists - delete it's cache
                int index = poolManagerRef.pools.FindIndex(x => x.name == poolCache.poolName);
                if (index == -1)
                {
                    cacheToDelete.Add(poolCache);
                }
            }

            // deleting all obsolete cache
            if (cacheToDelete.Count > 0)
            {
                string updateLog = "";

                foreach (PoolCache currentCache in cacheToDelete)
                {
                    currentLevelCache.Remove(currentCache);
                    updateLog += "deleted cache for unexisting pool: \"" + currentCache.poolName + "\"\n";
                }

                Debug.Log(updateLog);
                PoolManagerCache allCache = LoadAllCache();

                allCache.UpdateCache(GetCurrentCacheId(), currentLevelCache);
                Serializer.SerializeToPDP(allCache, PoolManager.CACHE_FILE_NAME);
            }
        }
    }

    public void ClearCurrentChache()
    {
        if (EditorUtility.DisplayDialog("Delete all cache", "All cache for current scene will be cleared", "Delete", "Cancel"))
        {
            PoolManagerCache allCache = LoadAllCache();

            allCache.DeleteCache(GetCurrentCacheId());
            Serializer.SerializeToPDP(allCache, PoolManager.CACHE_FILE_NAME);

            Debug.Log("Cache for current scene cleared");
        }
    }

    private PoolManagerCache LoadAllCache()
    {
        return Serializer.DeserializeFromPDP<PoolManagerCache>(PoolManager.CACHE_FILE_NAME, logIfFileNotExists: false);
    }

    private List<PoolCache> LoadCurrentCache()
    {
        PoolManagerCache allCache = LoadAllCache();

        string currentCacheId = GetCurrentCacheId();

        return allCache.GetPoolCache(currentCacheId);
    }

    private string GetCurrentCacheId()
    {
        string sceneMetaFile = Serializer.LoadTextFileAtPath(SceneManager.GetActiveScene().path + ".meta");

        int startIndex = sceneMetaFile.IndexOf("guid: ") + "guid: ".Length;
        int finalIndex = sceneMetaFile.LastIndexOf("DefaultImporter:");

        return sceneMetaFile.Substring(startIndex, finalIndex - startIndex);
    }

    private void UpdateCacheStateList()
    {
        poolsCacheDeltaList = new List<int>();
        poolsCacheList = new List<PoolCache>();

        for (int i = 0; i < poolManagerRef.pools.Count; i++)
        {
            poolsCacheDeltaList.Add(0);
            poolsCacheList.Add(null);
        }

        if (!poolManagerRef.useCache)
            return;

        List<PoolCache> cache = LoadCurrentCache();

        if (!cache.IsNullOrEmpty())
        {
            for (int i = 0; i < poolManagerRef.pools.Count; i++)
            {
                int index = cache.FindIndex(x => x.poolName == poolManagerRef.pools[i].name);
                if (index != -1)
                {
                    int delta = cache[index].poolSize - poolManagerRef.pools[i].poolSize;

                    poolsCacheDeltaList[i] = delta;
                    poolsCacheList[i] = cache[index];
                }
            }
        }
    }

    private string CacheDeltaToState(int delta)
    {
        string state = string.Empty;

        if (delta > 0)
        {
            state = "+" + delta;
        }
        else if (delta < 0)
        {
            state = delta.ToString();
        }

        return state;
    }

    private void RenameCachedPool(string oldName, string newName)
    {
        List<PoolCache> poolCacheList = LoadCurrentCache();

        int index = poolCacheList.FindIndex(x => x.poolName == oldName);
        if (index != -1)
        {
            poolCacheList[index].poolName = newName;
        }

        PoolManagerCache allCache = LoadAllCache();

        allCache.UpdateCache(GetCurrentCacheId(), poolCacheList);
        Serializer.SerializeToPDP(allCache, PoolManager.CACHE_FILE_NAME);
    }

    private void UpdateIgnoreCacheStateOfPool(string poolName, bool newState)
    {
        List<PoolCache> poolCacheList = LoadCurrentCache();

        int index = poolCacheList.FindIndex(x => x.poolName == poolName);
        if (index != -1)
        {
            poolCacheList[index].ignoreCache = newState;
        }

        PoolManagerCache allCache = LoadAllCache();

        allCache.UpdateCache(GetCurrentCacheId(), poolCacheList);
        Serializer.SerializeToPDP(allCache, PoolManager.CACHE_FILE_NAME);
    }

    #endregion
}