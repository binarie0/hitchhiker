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
            || other.Width + delta.x >= Width
            || delta.y < 0
            || other.Height + delta.y >= Height
            ) return false;

        //check every row
        for (int rowIndex = delta.y; rowIndex < delta.y + other.Height; rowIndex++)
        {
            //shift the column over
            uint row = other.Rows[rowIndex] << delta.x;
            //if it matches the shifted row, then all columns match meaning we want to check the next row
            if ((Rows[rowIndex] & row) != row)
            {
                return false;
            }
        }
        return true;
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
        GridSpace e = new GridSpace
        {
            Width = Width,
            Height = Height,
            Rows = new uint[Rows.Length]
        };
        Array.Copy(Rows, e.Rows, Rows.Length);
        return e;
    }

}
