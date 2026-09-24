using System;
using UnityEngine;

/// <summary>
/// A GridSpace is a spacial area that an item can take up.
/// This has as visual editor inside the Inspector that should be used to interface with the setup of the object.
/// <br></br>
/// <br></br>
/// Note: all inputs are in column-major order in alignment with <see cref="Vector2Int.x"/>, <see cref="Vector2Int.y"/>.
/// <br></br>
/// <br></br>
/// To continue, all indexes are zero-based, so the range of any row input will be [0 - <see cref="Height"/>)
/// and the range of any column input will be [0 - <see cref="Width"/>)
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
    /// This may not line up with the actual space the GridSpace takes up. See <see cref="CanAccommodate(GridSpace, Vector2Int)"/>
    /// to check whether GridSpaces overlap.
    /// </summary>
    [SerializeField, Range(1, MaxOneDimensionalSize)]
    public uint Width = 1;

    /// <summary>
    /// The total height of the GridSpace. This has a max size of <see cref="MaxOneDimensionalSize"/> and a minimum size of 1.
    /// This may not line up with the actual space the GridSpace takes up. See <see cref="CanAccommodate(GridSpace, Vector2Int)"/>
    /// to check whether GridSpaces overlap.
    /// </summary>
    [SerializeField, Range(1, MaxOneDimensionalSize)]
    public uint Height = 1;

    /// <summary>
    /// The rows that the GridSpace takes up. Cells are stored in the bits of a uint array.
    /// </summary>
    [SerializeField]
    private uint[] Rows = { 0u };

    /// <summary>
    /// A default public constructor to be used by Unity when creating GridSpace objects. This sets the width and height to 1 automatically.
    /// </summary>
    public GridSpace(): this(1,1) { }

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
    /// Checks whether the space is occupied at the row and column specified.
    /// </summary>
    /// <param name="col">The column to check. This is zero-indexed.</param>
    /// <param name="row"></param>
    /// <returns>The bit value at <paramref name="col"/>,<paramref name="row"/>. </returns>
    public bool SpaceOccupied(uint col, uint row)
    {
        if (row >= Height || col >= Width)
        {
            return false;
        }
        return (Rows[row] & (1 << (int)col)) != 0;
    }

    /// <summary>
    /// Gets or sets the underlying bits of this GridSpace via the indexer.
    /// <br></br>
    /// For get, this function passes along the value returned by <see cref="SpaceOccupied(uint, uint)"/>.
    /// <br></br>
    /// For set, this function passes along the value specified to <see cref="SetCellValue(uint, uint, bool)"/>
    /// </summary>
    /// <param name="col">The column (x-axis) to edit.</param>
    /// <param name="row">The row (y-axis) to edit.</param>
    /// <returns>The bit value at <paramref name="col"/>,<paramref name="row"/>. </returns>
    public bool this[uint col, uint row]
    {
        get
        {
            return SpaceOccupied(col, row);
        }
        set
        {
            SetCellValue(col, row, value);
        }
    }

    /// <summary>
    /// Gets or sets the underlying bits of this GridSpace via the indexer.
    /// <br></br>
    /// For get, this function passes along the value returned by <see cref="SpaceOccupied(Vector2Int)"/>.
    /// <br></br>
    /// For set, this function passes along the value specified to <see cref="SetCellValue(Vector2Int, bool)"/>
    /// </summary>
    /// <param name="pos">The position (x, y) to check.</param>
    /// <returns>The bit value at the position specified. </returns>
    public bool this[Vector2Int pos]
    {
        get
        {
            return SpaceOccupied(pos);
        }
        set
        {
            SetCellValue(pos, value);
        }
    }

    /// <summary>
    /// Gets or sets the underlying bits of this GridSpace via the indexer.
    /// <br></br>
    /// For get, this function passes along the value returned by <see cref="SpaceOccupied(uint, uint)"/>.
    /// <br></br>
    /// For set, this function passes along the value specified to <see cref="SetCellValue(uint, uint, bool)"/>
    /// </summary>
    /// <param name="col">The column (x-axis) to edit.</param>
    /// <param name="row">The row (y-axis) to edit.</param>
    /// <returns>The bit value at <paramref name="col"/>,<paramref name="row"/>. </returns>
    public bool this[int col, int row]
    {
        get
        {
            return SpaceOccupied((uint)col, (uint)row);
        }
        set
        {
            SetCellValue((uint)col, (uint)row, value);
        }
    }

    /// <summary>
    /// Checks whether the space is occupied at the position specified. Under the hood, this calls <see cref="SpaceOccupied(uint, uint)"/>.
    /// </summary>
    /// <param name="position">The position to check. This is zero-indexed.</param>
    /// <returns></returns>
    public bool SpaceOccupied(Vector2Int position) => SpaceOccupied((uint)position.x, (uint)position.y);

    
    

    /// <summary>
    /// Checks whether this grid space can accommodate <paramref name="other"/> with a <paramref name="delta"/> offset.
    /// </summary>
    /// <param name="other">The other gridspace to check against. If other is larger than this object, then this check automatically returns false</param>
    /// <param name="delta"></param>
    /// <returns>A boolean that determines whether all bit checks have been satisfied. If this returns true, then <see cref="MarkSpaceAsReserved(GridSpace, Vector2Int)"/> is available.</returns>
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
    /// Marks all space that the reserved space takes up as not free. This sets all bits of overlap to false,
    /// meaning subsequent checks of that space will return false in <see cref="CanAccommodate(GridSpace, Vector2Int)"/>.
    /// </summary>
    /// <param name="reservedSpace">The space to reserve. The space to reserve must be contained by this object.</param>
    /// <param name="delta">The offset to use when marking the space as reserved.</param>
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
    /// Sets a cell's value directly.
    /// </summary>
    /// <param name="location">The offset to use. If the location is not contained within the bounds of this grid space, nothing happens.</param>
    /// <param name="on">Whether the cell should be on (available for reservation) or off (reserved)</param>
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

    /// <summary>
    /// Sets a cell's value directly.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <param name="on"></param>
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
        for (int rowIndex = Mathf.Max(delta.y, 0); rowIndex < delta.y + reservedSpace.Height; rowIndex++)
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

    /// <summary>
    /// Rotates a grid space from an orientation to a different orientation. Used by <see cref="InventoryItem.TurnClockwise"/>.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public GridSpace Rotate(GridSpaceOrientation from, GridSpaceOrientation to)
    {
        if (from == to)
            return this.Duplicate();

        int f = (int)from;
        int t = (int)to;
        //if we are just flipping the location of each value we can just directly make a copy
        if ((f - t) % 2 == 0)
        {
            return Flip180();
        }
        else
        {
            GridSpace r = new(Height, Width);
            for (uint row = 0; row < Height; row++)
            {
                for (uint col = 0; col < Width; col++)
                {
                    r.SetCellValue(row, Width - col - 1, SpaceOccupied(Width - col - 1, Height - row - 1));
                }
            }
            return f - t > 0 || (f - t < -2) ? r.Flip180() : r;
        }
    }

    /// <summary>
    /// Flips the grid space by 180*
    /// </summary>
    /// <returns></returns>
    private GridSpace Flip180()
    {
        GridSpace copy = new GridSpace(Width, Height);
        for (uint row = 0; row < Height; row++)
        {
            for (uint col = 0; col < Width; col++)
            {
                copy.SetCellValue(col, row, SpaceOccupied(Width - col - 1, Height - row - 1));
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