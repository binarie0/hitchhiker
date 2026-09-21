using System;
using UnityEngine;

/// <summary>
/// A GridSpace is a spacial area that an item can take up.
/// This has as visual editor inside the Inspector that should be used to interface with the setup of the object.
/// </summary>
[Serializable]
public class GridSpace
{
    /// <summary>
    /// The max size that either the width or height can be. In this case, it is the size in bits of <see cref="UInt64"/>
    /// </summary>
    internal const uint MaxOneDimensionalSize = 32;


    /// <summary>
    /// The total width of the GridSpace. This has a max size of <see cref="MaxOneDimensionalSize"/> and a minimum size of 1.
    /// This may not line up with the actual space the GridSpace takes up. See <see cref="Overlaps(GridSpace, Vector2Int, Vector2Int)"/>
    /// to check whether GridSpaces overlap.
    /// </summary>
    [SerializeField, Range(1, MaxOneDimensionalSize)]
    public uint Width = 1;

    /// <summary>
    /// The total height of the GridSpace. This has a max size of <see cref="MaxOneDimensionalSize"/> and a minimum size of 1.
    /// This may not line up with the actual space the GridSpace takes up. See <see cref="Overlaps(GridSpace, Vector2Int, Vector2Int)"/>
    /// to check whether GridSpaces overlap.
    /// </summary>
    [SerializeField, Range(1, MaxOneDimensionalSize)]
    public uint Height = 1;

    /// <summary>
    /// The rows that the GridSpace takes up. These are stored in the bits of a uint array.
    /// </summary>
    [SerializeField]
    private uint[] Rows = { 0u };


    public GridSpace()
    {

    }
    /// <summary>
    /// Creates a new GridSpace given a set width and height.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    private GridSpace(uint width, uint height)
    {
        Width = width;
        Height = height;
        Rows = new uint[Height];
    }



    /// <summary>
    /// Checks whether the space is occupied at the row and column specified
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    public bool SpaceOccupied(uint row, uint col)
    {
        if (row >= Height || col >= Width)
        {
            return false;
        }
        return (Rows[row] & (1 << (int)col)) != 0;
    }

    
    

    /// <summary>
    /// Checks whether this grid space can accommodate <paramref name="other"/> with a <paramref name="delta"/> offset.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    internal bool CanAccommodate(GridSpace other, Vector2Int delta)
    {
        //simple bounds check
        if (delta.x < 0
            || other.Width + delta.x > Width
            || delta.y < 0
            || other.Height + delta.y > Height
            ) return false;

        //check every row
        for (int rowIndex = delta.y; rowIndex < delta.y + other.Height; rowIndex++)
        {
            //shift the column over
            uint row = other.Rows[rowIndex - delta.y] << delta.x;
            //if it matches the shifted row, then all columns match meaning we want to check the next row
            if ((Rows[rowIndex] & row) != row)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Marks all space that the reserved space takes up as not free.
    /// </summary>
    /// <param name="reservedSpace"></param>
    /// <param name="delta"></param>
    internal void MarkSpaceAsReserved(GridSpace reservedSpace, Vector2Int delta)
    {
        if (!CanAccommodate(reservedSpace, delta))
        {
            return;
        }
        //check every row
        for (int rowIndex = delta.y; rowIndex < delta.y + reservedSpace.Height; rowIndex++)
        {
            //shift the column over
            uint row = reservedSpace.Rows[rowIndex - delta.y] << delta.x;
            Rows[rowIndex] &= ~row;
        }

    }

    /// <summary>
    /// Sets a cell's values directly.
    /// </summary>
    /// <param name="location"></param>
    /// <param name="on"></param>
    internal void SetCellValue(Vector2Int location, bool on)
    {
        if (location.x < 0
            ||
            location.y < 0
            ||
            location.x >= Width
            || location.y >= Height)
            return;

        if (on)
        {
            Rows[location.y] |= (1u << (int)location.x);
        }
        else
        {
            Rows[location.y] &= ~(1u << (int)location.x);
        }
    }

    internal void SetCellValue(uint col, uint row, bool on)
    {
        SetCellValue(new Vector2Int((int)col, (int)row), on);
    }

    /// <summary>
    /// Unreserves space and marks all spaces as free.
    /// </summary>
    /// <param name="reservedSpace"></param>
    /// <param name="delta"></param>
    internal void UnreserveSpace(GridSpace reservedSpace, Vector2Int delta)
    {
        //check every row
        for (int rowIndex = delta.y; rowIndex < delta.y + reservedSpace.Height; rowIndex++)
        {
            //shift the column over
            uint row = reservedSpace.Rows[rowIndex - delta.y] << delta.x;
            Rows[rowIndex] |= row;
        }
    }

    /// <summary>
    /// AUtomatically gets a quick size measurement for the grid space.
    /// </summary>
    /// <param name="o"></param>
    public static implicit operator Vector2Int(GridSpace o)
    {
        return new Vector2Int((int)o.Width, (int)o.Height);
    }

    /// <summary>
    /// Duplicates the GridSpace. Used by <see cref="InventoryGrid"/> to have a GridSpace for available space and for the shape of the grid.
    /// </summary>
    /// <returns></returns>
    public GridSpace Duplicate()
    {
        GridSpace e = new GridSpace(Width, Height);        
        Array.Copy(Rows, e.Rows, Rows.Length);
        return e;
    }

    public GridSpace Rotate(GridSpaceOrientation from, GridSpaceOrientation to)
    {
        if (from == to)
            return this;

        int f = (int)from;
        int t = (int)to;
        //if we are just flipping the location of each value we can just directly make a copy
        if ((f - t) % 2 == 0)
        {
            return Flip180();
        }
        else
        {
            GridSpace r = new GridSpace(Height, Width);
            for (uint row = 0; row < Height; row++)
            {
                for (uint col = 0; col < Width; col++)
                {
                    r.SetCellValue(row, col, SpaceOccupied(Height - row - 1, Width - row - 1));
                }
            }
            return f - t > 0 || (f - t < -2) ? r.Flip180() : r;
        }
    }

    private GridSpace Flip180()
    {
        GridSpace copy = new GridSpace(Width, Height);
        for (uint row = 0; row < Height; row++)
        {
            for (uint col = 0; col < Width; col++)
            {
                copy.SetCellValue(col, row, SpaceOccupied(Height - row - 1, Width - row - 1));
            }
        }
        return copy;
    }
}

/// <summary>
/// The orientation of an item.
/// <br></br><br></br>
/// When designing the item inside the Inspector, you are designing the item with the Up orientation.
/// </summary>
public enum GridSpaceOrientation
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,

}