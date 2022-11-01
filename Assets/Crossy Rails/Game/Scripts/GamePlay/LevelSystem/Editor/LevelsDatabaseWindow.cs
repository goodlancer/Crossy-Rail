using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Watermelon;

// todo list:
// - auto check for level correctness when opening editor

[InitializeOnLoadAttribute]
public class LevelsDatabaseWindow : EditorWindow
{
    private enum DBMode
    {
        Blank,
        Edit,
    }

    private enum ListDisplayMode
    {
        Normal = 0,
        Move = 1,
        Delete = 2,
    }

    private DBMode state;
    private ListDisplayMode listDisplayMode;
    private int selectedLevel = 0;
    private int tempLevelCreationNumber;

    private static readonly string DATABASE_PATH = @"Assets/Crossy Rails/Content/Levels/LevelsDatabase.asset";
    private static readonly string LEVELS_PATH = @"Assets/Crossy Rails/Content/Levels/Levels/";
    private static readonly string DELETED_LEVELS_PATH = @"Crossy Rails/Content/Levels/DeletedLevels/";
    private const float LIST_AREA_WIDTH = 250f;

    private LevelsDatabase levels;
    private Vector2 _scrollPos;

    private Color defaultColor;
    private Color selectedLevelColor;

    private GUIStyle infoLabelStyle;
    private GUIStyle buttonStyle;

    private CellType selectedCell = CellType.Empty;

    private Color emptyColor;
    private Color staticColor = new Color(0.25f, 0.25f, 0.25f);
    private Color moving = new Color(1f, 1f, 0.75f);
    private Color rails = new Color(1f, 0.5f, 0.3f);
    private Color color3 = new Color(0.5f, 0.8f, 1f);

    private Color disabledColor = new Color(0.6f, 0.6f, 0.6f);
    private bool solvedDisplayMode = true;

    [MenuItem("Tools/Levels Database")]
    public static void Init()
    {
        LevelsDatabaseWindow window = EditorWindow.GetWindow<LevelsDatabaseWindow>();
        window.minSize = new Vector2(400, 400);
        window.Show();
    }

    void OnEnable()
    {
        if (levels == null)
        {
            LoadDatabase();
        }

        //state = DBMode.Blank;
        listDisplayMode = ListDisplayMode.Normal;
        defaultColor = GUI.color;
        emptyColor = GUI.color;
        selectedLevelColor = new Color(0.77f, 0.77f, 0.77f, 1f);

        CheckIfLevelsCorrect();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        DisplayListArea();
        DisplayMainArea();
        EditorGUILayout.EndHorizontal();
    }

    void InitSyles()
    {
        infoLabelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
        };

        buttonStyle = new GUIStyle(GUI.skin.textField)
        {
            margin = new RectOffset(0, 0, 0, 0),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
    }

    void LoadDatabase()
    {
        levels = (LevelsDatabase)AssetDatabase.LoadAssetAtPath(DATABASE_PATH, typeof(LevelsDatabase));

        if (levels == null)
            CreateDatabase();
    }

    void CreateDatabase()
    {
        levels = ScriptableObject.CreateInstance<LevelsDatabase>();
        AssetDatabase.CreateAsset(levels, DATABASE_PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CheckIfLevelsCorrect()
    {
        for (int i = 1; i <= levels.LevelsAmount; i++)
        {
            levels.GetLevelDBItem(i).level.CheckLevel();
        }
    }

    void DisplayListArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(LIST_AREA_WIDTH));
        EditorGUILayout.Space();

        _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, "box", GUILayout.ExpandHeight(true));

        for (int levelNumber = 1; levelNumber <= levels.LevelsAmount; levelNumber++)
        {
            EditorGUILayout.BeginHorizontal();

            if (listDisplayMode == ListDisplayMode.Move)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(15));

                GUILayout.Space(4);

                if (GUILayout.Button("▲", GUILayout.Height(15)))
                {
                    if (levels.MoveUp(levelNumber))
                    {
                        selectedLevel = levelNumber - 1;

                        // level files renaming
                        AssetDatabase.RenameAsset(LEVELS_PATH + "Level" + levelNumber + ".asset", "TEMP.asset");
                        AssetDatabase.RenameAsset(LEVELS_PATH + "Level" + (levelNumber - 1) + ".asset", "Level" + levelNumber + ".asset");
                        AssetDatabase.RenameAsset(LEVELS_PATH + "TEMP.asset", "Level" + (levelNumber - 1) + ".asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    EditorUtility.SetDirty(levels);
                    return;
                }

                if (GUILayout.Button("▼", GUILayout.Height(15)))
                {
                    if (levels.MoveDown(levelNumber))
                    {
                        selectedLevel = levelNumber + 1;

                        // level files renaming
                        AssetDatabase.RenameAsset(LEVELS_PATH + "Level" + levelNumber + ".asset", "TEMP.asset");
                        AssetDatabase.RenameAsset(LEVELS_PATH + "Level" + (levelNumber + 1) + ".asset", "Level" + levelNumber + ".asset");
                        AssetDatabase.RenameAsset(LEVELS_PATH + "TEMP.asset", "Level" + (levelNumber + 1) + ".asset");

                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    EditorUtility.SetDirty(levels);
                    return;
                }

                EditorGUILayout.EndVertical();
            }
            else if (listDisplayMode == ListDisplayMode.Delete)
            {
                GUI.backgroundColor = rails;

                if (GUILayout.Button("✕", GUILayout.Width(25)))
                {
                    RemoveLevel(levelNumber);
                    EditorUtility.SetDirty(levels);
                    state = DBMode.Blank;
                    return;
                }

                GUI.backgroundColor = defaultColor;
            }

            LevelDBItem levelItem = levels.GetLevelDBItem(levelNumber);
            Level level = levelItem.level;

            string listItemName = "Level " + levelItem.Number + (level != null ? (!level.isLevelCorrect ? "[NOT CORRECT]" : "") : " (NO LEVEL FILE)");

            if (levelNumber == selectedLevel)
            {
                GUI.color = selectedLevelColor;
            }

            GUILayoutOption height = GUILayout.Height(listDisplayMode == ListDisplayMode.Move ? 35 : 22);

            // level select button
            if (GUILayout.Button(listItemName, "box", GUILayout.ExpandWidth(true), height))
            {
                if (selectedLevel != levelNumber)
                {
                    selectedLevel = levelNumber;
                    state = DBMode.Edit;

                    if (level != null)
                    {
                        level.CheckLevel();
                    }
                }
                else
                {
                    selectedLevel = 0;
                    state = DBMode.Blank;
                    InitSyles();

                    if (level != null)
                    {
                        level.CheckLevel();
                    }
                }

                solvedDisplayMode = true;
            }

            GUI.color = defaultColor;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.BeginHorizontal();

        if (listDisplayMode == ListDisplayMode.Normal)
            GUI.backgroundColor = selectedLevelColor;

        if (GUILayout.Button("List", "box", GUILayout.ExpandWidth(true)))
        {
            listDisplayMode = ListDisplayMode.Normal;
        }

        GUI.backgroundColor = defaultColor;
        if (listDisplayMode == ListDisplayMode.Move)
            GUI.backgroundColor = selectedLevelColor;

        if (GUILayout.Button("Move", "box", GUILayout.ExpandWidth(true)))
        {
            listDisplayMode = ListDisplayMode.Move;
        }

        GUI.backgroundColor = defaultColor;
        if (listDisplayMode == ListDisplayMode.Delete)
            GUI.backgroundColor = selectedLevelColor;

        if (GUILayout.Button("Delete", "box", GUILayout.ExpandWidth(true)))
        {
            listDisplayMode = ListDisplayMode.Delete;
        }

        GUI.backgroundColor = defaultColor;

        EditorGUILayout.EndHorizontal();

        if (state == DBMode.Edit && listDisplayMode == ListDisplayMode.Normal)
        {
            DrawCellsButtons();
        }

        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Levels amount: " + levels.LevelsAmount, GUILayout.Width(130));


        //if (listDisplayMode == ListDisplayMode.Normal)
        //{
        //    if (GUILayout.Button("New Lelel"))
        //    {

        //    }
        //}

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    private void RemoveLevel(int levelNumber)
    {
        Debug.Log("remove level: " + levelNumber);
        if (!Directory.Exists(Application.dataPath + DELETED_LEVELS_PATH))
        {
            Directory.CreateDirectory(Application.dataPath + DELETED_LEVELS_PATH);
        }

        if (levels.GetLevelDBItem(levelNumber).level != null)
        {
            AssetDatabase.RenameAsset(LEVELS_PATH + levels.GetLevelDBItem(levelNumber).level.name + ".asset", "DeletedLevel" + System.DateTime.UtcNow.ToString("MM_dd_yyyy_HH_mm_ss") + ".asset");
            AssetDatabase.MoveAsset(LEVELS_PATH + levels.GetLevelDBItem(levelNumber).level.name + ".asset", @"Assets/" + DELETED_LEVELS_PATH + levels.GetLevelDBItem(levelNumber).level.name + ".asset");
        }

        levels.RemoveLevel(levelNumber);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        OnFileDeletedUpdateLevelFileNames();
    }

    private void OnFileDeletedUpdateLevelFileNames()
    {
        for (int i = 0; i < levels.LevelsAmount; i++)
        {
            if (levels.GetLevelDBItem(i + 1).level.name != "Level" + (i + 1))
            {
                levels.GetLevelDBItem(i + 1).Number = i + 1;
                AssetDatabase.RenameAsset(LEVELS_PATH + levels.GetLevelDBItem(i + 1).level.name + ".asset", "Level" + (i + 1) + ".asset");
            }
        }
    }

    private void OnFileInsertedUpdateLevelFileNames()
    {
        for (int i = levels.LevelsAmount - 1; i >= 0; i--)
        {
            if (levels.GetLevelDBItem(i + 1).level.name != "Level" + (i + 1))
            {
                levels.GetLevelDBItem(i + 1).Number = i + 1;
                AssetDatabase.RenameAsset(LEVELS_PATH + levels.GetLevelDBItem(i + 1).level.name + ".asset", "Level" + (i + 1) + ".asset");
            }
        }
    }

    void DrawCellsButtons()
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Space(8);

        GUI.backgroundColor = emptyColor;
        if (GUILayout.Button("Empty" + (selectedCell == CellType.Empty ? " (Selected)" : ""), "box", GUILayout.ExpandWidth(true)))
        {
            selectedCell = CellType.Empty;
        }

        GUI.backgroundColor = staticColor;
        if (GUILayout.Button("Static" + (selectedCell == CellType.Static ? " (Selected)" : ""), "box", GUILayout.ExpandWidth(true)))
        {
            selectedCell = CellType.Static;
        }

        GUI.backgroundColor = moving;
        if (GUILayout.Button("Moving" + (selectedCell == CellType.Movable ? " (Selected)" : ""), "box", GUILayout.ExpandWidth(true)))
        {
            selectedCell = CellType.Movable;
        }

        GUI.backgroundColor = rails;
        if (GUILayout.Button("Rails" + (selectedCell == CellType.Rails ? " (Selected)" : ""), "box", GUILayout.ExpandWidth(true)))
        {
            selectedCell = CellType.Rails;
        }

        GUI.backgroundColor = defaultColor;
        EditorGUILayout.EndVertical();
    }

    void DisplayMainArea()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        switch (state)
        {
            case DBMode.Edit:
                DisplayEditMainArea();
                break;
            default:
                DisplayBlankMainArea();
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();
    }

    void DisplayBlankMainArea()
    {
        InitSyles();

        EditorGUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("Create new or select existing level.", infoLabelStyle, GUILayout.Height(30));
        GUILayout.Space(20);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("New Lelel", GUILayout.Width(120), GUILayout.Height(40)))
        {
            Level level = CreateNewLevel(levels.LevelsAmount + 1);
            LevelDBItem levelDBItem = new LevelDBItem(levels.LevelsAmount + 1, level);

            levels.Add(levelDBItem);
            OnFileInsertedUpdateLevelFileNames();

            EditorUtility.SetDirty(levels);
            state = DBMode.Blank;
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }


    void DisplayEditMainArea()
    {
        LevelDBItem levelDBItem = levels.GetLevelDBItem(selectedLevel);
        Level level = levelDBItem.level;

        if (level == null)
        {
            EditorGUILayout.LabelField("Please, assign level file.");
            levelDBItem.level = (Level)EditorGUILayout.ObjectField("Level: ", level, typeof(Level), true);

            return;
        }

        EditorGUILayout.LabelField("Level " + selectedLevel + ((!level.isLevelCorrect || level.needUpdateShuffle) ? "  [NOT CORRECT]" : ""));

        if (!level.isLevelCorrect)
        {
            EditorGUILayout.HelpBox(level.errorString, MessageType.Error);
        }

        levelDBItem.level = (Level)EditorGUILayout.ObjectField("Level: ", level, typeof(Level), true);

        Vector2 size = EditorGUILayout.Vector2Field("Size:", new Vector2(level.size.x, level.size.y));
        level.size = new Index2(size.x, size.y);

        if (size.x < 2 || size.y < 2)
        {
            EditorGUILayout.LabelField("Please, set level size above.");
            return;
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (solvedDisplayMode)
        {
            GUI.color = selectedLevelColor;
        }

        if (GUILayout.Button("Solved", "box", GUILayout.MinWidth(50f), GUILayout.MaxWidth(200f), GUILayout.Height(30f)))
        {
            solvedDisplayMode = true;
        }

        GUI.color = defaultColor;
        if (!solvedDisplayMode)
        {
            GUI.color = selectedLevelColor;
        }

        if (GUILayout.Button(level.needUpdateShuffle ? "!!! Shuffle Required !!!" : "Shuffled", "box", GUILayout.MinWidth(50f), GUILayout.MaxWidth(200f), GUILayout.Height(30f)))
        {
            solvedDisplayMode = false;

            level.CheckLevel();
        }
        GUI.color = defaultColor;


        if (!solvedDisplayMode)
        {
            if (GUILayout.Button("Shuffle", "box", GUILayout.MinWidth(50f), GUILayout.Height(30f)))
            {
                level.ShuffleItems();

                EditorUtility.SetDirty(level);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.EndHorizontal();

        DrawLevel(level);

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();

        //if (GUILayout.Button("Test", GUILayout.Width(100)))
        //{
        //    GameSettingsPrefs.Set("[Editor] TestLevelNumber:", selectedLevel);

        //    if (!Application.isPlaying)
        //    {
        //        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "", false);
        //    }

        //    EditorApplication.ExecuteMenuItem("Edit/Play");
        //}

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Apply", GUILayout.Width(100)))
        {
            EditorUtility.SetDirty(levels);
            level.CheckLevel();
            state = DBMode.Blank;
            selectedLevel = -1;
        }
        GUILayout.EndHorizontal();
    }


    private void DrawLevel(Level level)
    {
        Event currEvent = Event.current;


        if (level.items == null || level.size.x != level.items.Length || level.size.y != level.items[0].ints.Length)
        {
            InitNewLevel(level);
        }

        Rect arrayRect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));

        Vector2 arrayStart = arrayRect.position;

        float biggerSideSize = level.size.x >= level.size.y ? level.size.x : level.size.y;

        float elementSize = (float)(EditorGUIUtility.currentViewWidth - LIST_AREA_WIDTH - GUI.skin.box.padding.right - GUI.skin.box.margin.right - GUI.skin.box.margin.left) / biggerSideSize;

        Level.IntArray[] list = solvedDisplayMode ? level.items : level.itemsShuffled;

        // Drawing
        if (currEvent.type == EventType.MouseDrag)
        {
            Vector2 elementUnderMouseIndex = (currEvent.mousePosition - arrayStart) / elementSize;
            elementUnderMouseIndex = new Vector2(Mathf.Clamp(Mathf.FloorToInt(elementUnderMouseIndex.x), 0, list.Length - 1), Mathf.Clamp(list[0].ints.Length - 1 - Mathf.FloorToInt(elementUnderMouseIndex.y), 0, list[0].ints.Length - 1));

            Index2 index = new Index2(elementUnderMouseIndex.x, elementUnderMouseIndex.y);

            if (list[index.x].ints[index.y] != (int)selectedCell)
            {
                list[index.x].ints[index.y] = (int)selectedCell;

                if (selectedCell != CellType.Rails)
                {
                    level.extraProps[index.x].ints[index.y] = (int)CellBehaviour.ExtraProperty.Normal;
                }

                if (solvedDisplayMode)
                {
                    level.needUpdateShuffle = true;
                }
            }

            currEvent.Use();
        }

        for (int z = list[0].ints.Length - 1; z >= 0; z--)
        {
            Rect lineRect = EditorGUILayout.BeginHorizontal();
            Rect fullLineRect = new Rect(0, lineRect.y, Screen.width, lineRect.height);

            for (int x = 0; x < list.Length; x++)
            {
                GUI.backgroundColor = GetColor((CellType)list[x].ints[z]);
                string label = ((CellBehaviour.ExtraProperty)level.extraProps[x].ints[z]).ToString();
                if (label.Equals("Normal"))
                {
                    label = "";
                }

                Vector2 cellStart = arrayStart + new Vector2(x * elementSize, (list[0].ints.Length - 1 - z) * elementSize);


                if (GUILayout.Button(label, buttonStyle, GUILayout.Width(elementSize), GUILayout.Height(elementSize)))
                {
                    if (currEvent.button == 0)
                    {
                        if (solvedDisplayMode)
                        {
                            level.needUpdateShuffle = true;
                        }

                        Undo.RecordObject(levels, "Edit");

                        if (list[x].ints[z] != (int)selectedCell)
                        {
                            list[x].ints[z] = (int)selectedCell;

                            if (selectedCell != CellType.Rails)
                            {
                                level.extraProps[x].ints[z] = (int)CellBehaviour.ExtraProperty.Normal;
                            }
                        }
                        else
                        {
                            list[x].ints[z] = list[x].ints[z] == (int)CellType.Movable ? (int)CellType.Rails : (int)CellType.Movable;
                            level.extraProps[x].ints[z] = (int)CellBehaviour.ExtraProperty.Normal;
                        }

                        currEvent.Use();
                    }
                    else if (currEvent.button == 1 && list[x].ints[z] == (int)CellType.Rails)
                    {
                        GenericMenu menu = new GenericMenu();

                        int tempX = x;
                        int tempZ = z;

                        menu.AddItem(new GUIContent("Normal"), level.extraProps[x].ints[z] == (int)CellBehaviour.ExtraProperty.Normal, () => level.extraProps[tempX].ints[tempZ] = (int)CellBehaviour.ExtraProperty.Normal);
                        menu.AddItem(new GUIContent("Gold"), level.extraProps[x].ints[z] == (int)CellBehaviour.ExtraProperty.Gold, () => level.extraProps[tempX].ints[tempZ] = (int)CellBehaviour.ExtraProperty.Gold);
                        menu.AddItem(new GUIContent("Mine"), level.extraProps[x].ints[z] == (int)CellBehaviour.ExtraProperty.Mine, () => level.extraProps[tempX].ints[tempZ] = (int)CellBehaviour.ExtraProperty.Mine);

                        menu.ShowAsContext();
                    }
                }

                GUI.backgroundColor = defaultColor;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();


        if (GUI.changed)
        {
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private bool IsMouseAboveCell(Vector2 mousePos, Vector2 cellStart, float cellSize)
    {
        return mousePos.x >= cellStart.x && (mousePos.x <= cellStart.x + cellSize) && mousePos.y >= cellStart.y && (mousePos.y <= cellStart.y + cellSize);
    }

    private void InitNewLevel(Level level)
    {
        level.items = new Level.IntArray[(int)level.size.x];
        level.itemsShuffled = new Level.IntArray[(int)level.size.x];
        level.extraProps = new Level.IntArray[(int)level.size.x];

        for (int i = 0; i < level.size.x; i++)
        {
            level.items[i] = new Level.IntArray(new int[(int)level.size.y]);
            level.itemsShuffled[i] = new Level.IntArray(new int[(int)level.size.y]);
            level.extraProps[i] = new Level.IntArray(new int[(int)level.size.y]);

            for (int j = 0; j < level.size.y; j++)
            {
                level.items[i].ints[j] = (int)CellType.Movable;
                level.itemsShuffled[i].ints[j] = (int)CellType.Movable;
                level.extraProps[i].ints[j] = (int)CellBehaviour.ExtraProperty.Normal;
            }
        }

        EditorUtility.SetDirty(level);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private Color GetColor(CellType type)
    {
        if (type == CellType.Movable)
        {
            return moving;
        }
        else if (type == CellType.Rails)
        {
            return rails;
        }
        else if (type == CellType.Static)
        {
            return staticColor;
        }
        else
        {
            return emptyColor;
        }
    }


    private Level CreateNewLevel(int levelNumber)
    {
        Level level = ScriptableObject.CreateInstance<Level>();
        AssetDatabase.CreateAsset(level, LEVELS_PATH + "NewLevel.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return level;
    }

    private void OnDisable()
    {
        if (levels != null)
        {
            EditorUtility.SetDirty(levels);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}