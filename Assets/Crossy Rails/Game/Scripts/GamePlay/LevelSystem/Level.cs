using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.Core;

[CreateAssetMenu(fileName = "Level", menuName = "LevelSystem/Level")]
[System.Serializable]
public class Level : ScriptableObject
{
    public bool isLevelCorrect = false;
    public bool needUpdateShuffle = false;

    public Index2 size;

    public IntArray[] items;
    public IntArray[] itemsShuffled;
    public IntArray[] extraProps;

    public Index2 start;
    public Index2 finish;

    public string errorString = string.Empty;

    private static Index2[] randomOffsets = new Index2[] { Index2.left, Index2.right, Index2.down, Index2.up };


    public void ShuffleItems()
    {
        needUpdateShuffle = false;
        itemsShuffled = new IntArray[size.x];

        Index2 emptyIndex = Index2.zero;

        for (int i = 0; i < size.x; i++)
        {
            itemsShuffled[i] = new IntArray(new int[size.y]);

            for (int j = 0; j < size.y; j++)
            {
                itemsShuffled[i].ints[j] = items[i].ints[j];

                if (itemsShuffled[i].ints[j] == (int)CellType.Empty)
                {
                    emptyIndex = new Index2(i, j);
                }
            }
        }

        int shufflesAmount = Random.Range(15, 30);

        for (int i = 0; i < shufflesAmount; i++)
        {
            Index2 movableItemIndex = GetMovableNeighbour(emptyIndex);

            if (movableItemIndex == new Index2(-1, -1))
            {
                Debug.Log("Breaking shuffle");
                return;
            }

            int movableItemValue = GetShuffledItem(movableItemIndex);
            SetShuffledItem(movableItemIndex, (int)CellType.Empty);
            SetShuffledItem(emptyIndex, movableItemValue);
            emptyIndex = movableItemIndex;
        }
    }

    public void CheckLevel()
    {
        int emptyCellsCount = 0;
        int startsAndFinishesAmount = 0;
        int minesAmount = 0;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                if (items[i].ints[j] == (int)CellType.Empty)
                {
                    emptyCellsCount++;
                }
                else if (j == 0 && items[i].ints[j] == (int)CellType.Rails && GetPathNeighboursAmount(i, j) == 1)
                {
                    start = new Index2(i, j);
                    startsAndFinishesAmount++;
                }
                else if (j == size.y - 1 && items[i].ints[j] == (int)CellType.Rails && GetPathNeighboursAmount(i, j) == 1)
                {
                    finish = new Index2(i, j);
                    startsAndFinishesAmount++;
                }
                else if (extraProps[i].ints[j] == (int)CellBehaviour.ExtraProperty.Mine)
                {
                    minesAmount++;
                }
            }
        }

        //Debug.Log("empty: " + emptyCellsCount + "  starts and finishes: " + startsAndFinishesAmount);

        isLevelCorrect = emptyCellsCount == 1 && startsAndFinishesAmount == 2 && (minesAmount == 0 || minesAmount == 2);

        if (isLevelCorrect)
        {
            errorString = string.Empty;
        }
        else
        {
            if (emptyCellsCount != 1)
            {
                errorString = "Level should have 1 empty cell (to be able move other)." + (emptyCellsCount == 0 ? " Please add one." : "Please remove excessive.");
            }
            else if (startsAndFinishesAmount != 2)
            {
                errorString = "Level should have one enterence (on the bottom) and one exit (on the top)." + (startsAndFinishesAmount < 2 ? " Please add both." : "Please remove excessive.");
            }
            else if (minesAmount != 0 && minesAmount != 2)
            {
                errorString = "Level can contain only 2 mines. Current amount: " + minesAmount;
            }
        }
    }

    private int GetPathNeighboursAmount(int x, int z)
    {
        int amount = 0;

        if (z + 1 < size.y && items[x].ints[z + 1] == (int)CellType.Rails)
        {
            amount++;
        }

        if (z - 1 >= 0 && items[x].ints[z - 1] == (int)CellType.Rails)
        {
            amount++;
        }

        if (x + 1 < size.x && items[x + 1].ints[z] == (int)CellType.Rails)
        {
            amount++;
        }

        if (x - 1 >= 0 && items[x - 1].ints[z] == (int)CellType.Rails)
        {
            amount++;
        }

        return amount;
    }

    private Index2 GetMovableNeighbour(Index2 index)
    {
        randomOffsets.Shuffle();

        for (int i = 0; i < 4; i++)
        {
            Index2 tempItemIndex = index + randomOffsets[i];

            if (tempItemIndex.x >= 0 && tempItemIndex.x < size.x && tempItemIndex.y >= 0 && tempItemIndex.y < size.y)
            {
                if (IsMovable(tempItemIndex))
                {
                    return tempItemIndex;
                }
            }
        }

        return new Index2(-1, -1);
    }

    private bool IsMovable(Index2 index)
    {
        int item = GetShuffledItem(index);
        return (item == (int)CellType.Movable || item == (int)CellType.Rails) && extraProps[index.x].ints[index.y] == (int)CellBehaviour.ExtraProperty.Normal;
    }

    private int GetShuffledItem(Index2 index)
    {
        return itemsShuffled[index.x].ints[index.y];
    }

    private void SetShuffledItem(Index2 index, int value)
    {
        itemsShuffled[index.x].ints[index.y] = value;
    }

    [System.Serializable]
    public struct IntArray
    {
        public int[] ints;

        public IntArray(int[] list)
        {
            ints = list;
        }
    }
}