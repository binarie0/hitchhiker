using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    [SerializeField]
    public ItemData Data;

    [SerializeField]
    internal Vector2Int Position;
    /// <summary>
    /// The item's orientation. Orientation needs to be explicitly marked through an int standpoint for overlap checks.
    /// </summary>
    [SerializeField]
    InventoryItemOrientation Orientation;

    /// <summary>
    /// The current rotation to show the item in
    /// </summary>
    public float Rotation
    {
        get
        {
            return -Mathf.PI * 0.5f * (float)Orientation;
        }
    }

    /// <summary>
    /// Determines through <see cref="GridSpace.Overlaps(GridSpace, Vector2Int, InventoryItemOrientation, Vector2Int, InventoryItemOrientation)"/>
    /// whether an inventory item is overlapping with this item.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Overlaps(InventoryItem other)
    {
        return Data.Size.Overlaps(other.Data.Size, Position, Orientation, other.Position, other.Orientation);
    }

}


public enum InventoryItemOrientation
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    
}

