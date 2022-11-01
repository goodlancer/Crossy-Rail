using UnityEngine;

[System.Serializable]
public class LevelDBItem
{
    [SerializeField]
    private int number;
    public Level level;

    public LevelDBItem()
    {
        number = 0;
        level = null;
    }

    public LevelDBItem(int levelNumber)
    {
        number = levelNumber;
        level = null;
    }

    public LevelDBItem(int levelNumber, Level level)
    {
        number = levelNumber;
        this.level = level;
    }

    public int Number
    {
        get { return number; }
        set
        {
            number = value;
        }
    }
}