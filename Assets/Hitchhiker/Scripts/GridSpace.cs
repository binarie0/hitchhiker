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
    /// Checks whether GridSpaces overlap.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="thisOffset"></param>
    /// <param name="otherOffset"></param>
    /// <returns></returns>
    public bool Overlaps(GridSpace other, Vector2Int thisOffset, InventoryItemOrientation thisOrientation, 
        Vector2Int otherOffset, InventoryItemOrientation otherOrientation)
    {
        Vector2Int diff = otherOffset - thisOffset;

        uint[] thisTransform = Transform(Vector2Int.zero, thisOrientation);
        uint[] otherTransform = other.Transform(diff, otherOrientation);

        return false;

    }

    private uint[] Transform(Vector2Int offset, InventoryItemOrientation orientation = InventoryItemOrientation.Up)
    {
        return Rows;    
    
    }



    public static implicit operator Vector2Int(GridSpace o)
    {
        return new Vector2Int((int)o.Width, (int)o.Height);
    }

}
