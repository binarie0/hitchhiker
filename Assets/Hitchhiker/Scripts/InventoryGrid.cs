using UnityEngine;
public class InventoryGrid : MonoBehaviour
{

    /// <summary>
    /// The amount of spaces left in the grid. This can be used to determine whether an item can still fit in an inventory
    /// given perfect conditions.
    /// </summary>
    public int TotalSpace
    {
        get;
    }

    /// <summary>
    /// Whether the given inventory is able to be serialized or deserialized
    /// </summary>
    public bool Savable
    {
        get;
    }

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
