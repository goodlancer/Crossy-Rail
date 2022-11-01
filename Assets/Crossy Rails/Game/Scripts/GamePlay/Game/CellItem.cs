using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CellItem
{
    [SerializeField]
    private CellType cellType;
    public CellType Type
    {
        get { return cellType; }
    }

    [SerializeField]
    private CellBehaviour cellBehaviour;
    public CellBehaviour Cell
    {
        get { return cellBehaviour; }
    }

    public CellItem()
    {
        cellType = CellType.Empty;
        cellBehaviour = null;
    }

    public void InitEmptyItem()
    {
        cellType = CellType.Empty;
        cellBehaviour = null;
    }

    public void InitGroundItem(CellBehaviour cell)
    {
        cellBehaviour = cell;
        cellType = cell.Type;
    }
}