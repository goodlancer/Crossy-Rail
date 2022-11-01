using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Watermelon;

[CreateAssetMenu(fileName = "NewLevelsDatabase", menuName = "LevelSystem/LevelDatabase")]
public class LevelsDatabase : ScriptableObject, IInitialized
{
    private static LevelsDatabase instance;

    [SerializeField]
    private List<LevelDBItem> database;

    public void Init()
    {
        instance = this;
    }

    void OnEnable()
    {
        if (database == null)
        {
            database = new List<LevelDBItem>();
        }
    }

    public void Add(LevelDBItem level)
    {
        // if level should be added to the end of list
        if (level.Number == database.Count + 1)
        {
            database.Add(level);
        }
        // if level should be inserted somewhere inside list
        else
        {
            database.Insert(level.Number - 1, level);
        }
    }

    public bool MoveUp(int levelNumber)
    {
        int indexOfItemToMove = levelNumber - 1;

        if (indexOfItemToMove >= 0 && indexOfItemToMove < database.Count)
        {
            // item already is at top
            if (indexOfItemToMove == 0)
            {
                return false;
            }

            // item moving
            LevelDBItem itemToMove = database[indexOfItemToMove];
            database.RemoveAt(indexOfItemToMove);
            database.Insert(indexOfItemToMove - 1, itemToMove);

            // numbers upgrade
            database[indexOfItemToMove - 1].Number = levelNumber - 1;
            database[indexOfItemToMove].Number = levelNumber;

            return true;
        }
        else
        {
            Debug.LogError("Level number is incorrect");
            return false;
        }
    }

    public bool MoveDown(int levelNumber)
    {
        int indexOfItemToMove = levelNumber - 1;

        if (indexOfItemToMove >= 0 && indexOfItemToMove < database.Count)
        {
            // item already is at top
            if (indexOfItemToMove == database.Count - 1)
            {
                return false;
            }

            // item moving
            LevelDBItem itemToMove = database[indexOfItemToMove];
            database.RemoveAt(indexOfItemToMove);
            database.Insert(indexOfItemToMove + 1, itemToMove);

            // numbers update
            database[indexOfItemToMove + 1].Number = levelNumber + 1;
            database[indexOfItemToMove].Number = levelNumber;




            return true;
        }
        else
        {
            Debug.LogError("Level number is incorrect");
            return false;
        }
    }

    public void RemoveLevel(int levelNumber)
    {
        database.RemoveAt(levelNumber - 1);
    }

    public static int LevelsCount
    {
        get { return instance.database.Count; }
    }

    public int LevelsAmount
    {
        get { return database.Count; }
    }

    public LevelDBItem GetLevelDBItem(int levelNumber)
    {
        levelNumber = Mathf.Clamp(levelNumber - 1, 0, database.Count - 1);
        return database[levelNumber];
    }

    public static Level GetLevel(int index)
    {
        index = Mathf.Clamp(index - 1, 0, instance.database.Count - 1);
        return instance.database[index].level;
    }
}