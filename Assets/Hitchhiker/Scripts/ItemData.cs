using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    
    /// <summary>
    /// Whether the item is stackable. This is ascertained by <see cref="MaxCount"/>, but is much easier to write this way.
    /// </summary>
    public bool Stackable
    {
        get
        {
            return MaxCount > 1;
        }
    }


    /// <summary>
    /// The maximum count an item can have. If left at 1, the item will be unable to be stacked.
    /// </summary>
    [SerializeField, Tooltip("The maximum count an item can have. If left at 1, the item will be unable to be stacked.")]
    internal uint MaxCount = 1;

    /// <summary>
    /// The amount of space the item will take up in the grid.
    /// </summary>
    [SerializeField]
    internal GridSpace Size;
}
