using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(InventoryGrid))]
[AddComponentMenu("Hitchhiker UI/Inventory Saver/Loader")]
public class InventorySaver : MonoBehaviour
{
    /// <summary>
    /// The grid to save / load.
    /// </summary>
    private InventoryGrid Grid;

    /// <summary>
    /// If the inventory has a save path.
    /// </summary>
    [Tooltip("The save and load path.")]
    public string LoadSavePath;

    private void Start()
    {
        Grid = GetComponent<InventoryGrid>();
        
    }

    /// <summary>
    /// Saves a grid to <see cref="LoadSavePath"/>.
    /// </summary>
    public void SaveGrid()
    {
        //TODO: save grid here
    }

    /// <summary>
    /// Loads a grid from <see cref="LoadSavePath"/> and loads the data into the grid component on this <see cref="Component.gameObject"/>.
    /// </summary>
    public void LoadGrid(bool overwrite = true)
    {
        if (overwrite)
        {
            //clear inventory
        }
        //TODO: load grid here
    }



}
