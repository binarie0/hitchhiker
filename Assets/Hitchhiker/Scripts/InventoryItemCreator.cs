using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// The InventoryItem
/// </summary>
[RequireComponent(typeof(InventoryGrid))]
public class InventoryItemCreator : MonoBehaviour
{
    
    /// <summary>
    /// The grid this creator requires. When creating items, this grid will house that item.
    /// </summary>
    InventoryGrid Grid;

    [Header("Note: this is a testing class.\nUse this to test whether your items\nare set up properly," +
        "but\ndon't keep this in your final game.")]
    /// <summary>
    /// The objects that should be used to test.
    /// </summary>
    [SerializeField]
    private GameObject[] TestingObjects = new GameObject[0];

    #region Unity-provided Methods
    void Awake()
    {
        Grid = GetComponent<InventoryGrid>();
        Grid.GridRendered.AddListener(OnGridRendered);
    }

    #endregion

    #region Event-Driven Methods
    /// <summary>
    /// Gets executed when <see cref="Grid"/> is finished rendering. 
    /// </summary>
    void OnGridRendered()
    {
        Button b = new Button(CreateRandomTestingObject);
        b.text = "Create Random Item";
        Grid.RootElement.Add(b);
    }

    /// <summary>
    /// Creates an object from the list of testing objects and throws it into the inventory.
    /// </summary>
    void CreateRandomTestingObject()
    {
        Debug.Assert(TestingObjects.Length > 0, "No objects to test!");
        GameObject o = Instantiate(TestingObjects[Random.Range(0, TestingObjects.Length)]);
        bool successful = o.TryGetComponent(out InventoryItem item);
        Debug.Assert(successful, "GameObject given as a testing object does not have an InventoryItem component!");
        Grid.InitItem(item, false);
    }

    #endregion


}
