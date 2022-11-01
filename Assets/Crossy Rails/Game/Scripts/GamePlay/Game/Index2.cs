using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Index2
{
    public int x;
    public int y;

    public Index2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Index2(float x, float y)
    {
        this.x = (int)x;
        this.y = (int)y;
    }

    public static Index2 operator +(Index2 a, Index2 b)
    {
        return new Index2(a.x + b.x, a.y + b.y);
    }

    public static Index2 operator -(Index2 a, Index2 b)
    {
        return new Index2(a.x - b.x, a.y - b.y);
    }

    public static Index2 operator *(Index2 a, float b)
    {
        return new Index2(a.x * b, a.y * b);
    }

    public static Index2 operator *(Index2 a, int b)
    {
        return new Index2(a.x * b, a.y * b);
    }

    /// <summary>
    /// Shorthand for writing Index2(0, 0).
    /// </summary>
    public static Index2 zero
    {
        get { return new Index2(0, 0); }
    }

    /// <summary>
    /// Shorthand for writing Index2(1, 1).
    /// </summary>
    public static Index2 one
    {
        get { return new Index2(1, 1); }
    }

    /// <summary>
    /// Shorthand for writing Index2(-1, 0).
    /// </summary>
    public static Index2 left
    {
        get { return new Index2(-1, 0); }
    }

    /// <summary>
    /// Shorthand for writing Index2(1, 0).
    /// </summary>
    public static Index2 right
    {
        get { return new Index2(1, 0); }
    }

    /// <summary>
    /// Shorthand for writing Index2(0, 1).
    /// </summary>
    public static Index2 up
    {
        get { return new Index2(0, 1); }
    }

    /// <summary>
    /// Shorthand for writing Index2(0, -1).
    /// </summary>
    public static Index2 down
    {
        get { return new Index2(0, -1); }
    }

    public static bool operator ==(Index2 a, Index2 b)
    {
        if (a.x == b.x && a.y == b.y)
            return true;
        else
            return false;
    }

    public static bool operator !=(Index2 a, Index2 b)
    {
        if (a.x != b.x || a.y != b.y)
            return true;
        else
            return false;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Index2))
        {
            return false;
        }

        var index = (Index2)obj;
        return x == index.x && y == index.y;
    }

    public override int GetHashCode()
    {
        var hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + x.GetHashCode();
        hashCode = hashCode * -1521134295 + y.GetHashCode();
        return hashCode;
    }

    public override string ToString()
    {
        return "[" + x + ", " + y + "]";
    }


    public Vector3 ToVector3XZ(float yValue = 0f)
    {
        return new Vector3(x, yValue, y);
    }
}