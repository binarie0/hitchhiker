using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class InventoryItem : MonoBehaviour
{

    


    public const int Max_Item_Stack = 99;

    /// <summary>
    /// The space that the item takes up.
    /// </summary>
    public GridSpace ItemSpace;

    [SerializeField]
    private Sprite image;

    /// <summary>
    /// The user interface that will be shown in-game. Can be created by calling 
    /// </summary>
    private VisualElement UI;

    /// <summary>
    /// Whether the UI has been grabbed.
    /// </summary>
    private bool Grabbed;
    

    [SerializeField, Tooltip("The item ID associated with this Item. This is used to associate items of the same type.")]
    private int inventoryID;

    /// <summary>
    /// The item ID associated with this Item.
    /// </summary>
    internal int ItemID
    {
        get
        {
            return inventoryID;
        }
    }

    /// <summary>
    /// The maximum amount of items that can be stacked for this item. If set to 1, the item is not stackable.
    /// </summary>
    [Tooltip("The maximum amount of items that can be stacked for this item. If set to 1, the item is not stackable."),
        Range(1, Max_Item_Stack)]
    private int maxCount = 1;

    /// <summary>
    /// The maximum count of this item. If set to 1, this item is not <see cref="Stackable"/>.
    /// </summary>
    public int MaxCount
    {
        get
        {
            return maxCount;
        }
    }


    private int stackCount = 1;

    /// <summary>
    /// The amount of items in this stack.
    /// </summary>
    public int StackCount
    {
        get
        {
            return stackCount;
        }
    }

    


    /// <summary>
    /// Whether the item is stackable. This is ascertained through <see cref="MaxCount"/>.
    /// </summary>
    public bool Stackable
    {
        get
        {
            return MaxCount > 1;
        }
    }



    /// <summary>
    /// Gets called whenever the amount of items in the stack has been updated.
    /// </summary>
    public UnityEvent<int> StackUpdated = new();

    /// <summary>
    /// Gets called when the item should be destroyed.
    /// </summary>
    public UnityEvent ItemDestroyed = new();

    /// <summary>
    /// The grid that this item is contained in.
    /// </summary>
    InventoryGrid ParentGrid;



    /// <summary>
    /// The position of the item inside the inventory.
    /// </summary>
    internal Vector2Int InventoryPosition = Vector2Int.zero;

    /// <summary>
    /// The orientation of the item inside the inventory. This can be saved out to a file if need be later.
    /// </summary>
    internal InventoryItemOrientation Orientation = InventoryItemOrientation.Up;

    /// <summary>
    /// The current rotation to show the item in. This is ascertained through <see cref="Orientation"/>.
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
        //TODO
        return false;
    }
    
    /// <summary>
    /// Stacks a like item onto this item.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public void StackItem(InventoryItem item)
    {

        //asserts same id (same item)
        Debug.Assert(ItemID == item.ItemID);
        Debug.Assert(Stackable && item.Stackable);

        // if we are already at capacity then can't do anything
        if (StackCount == MaxCount)
            //might want to emit an event here?
            return;

        //add the other stack to this one
        stackCount += item.StackCount;
        int overflow = 0;
        if (stackCount > MaxCount)
        {
            overflow = stackCount - MaxCount;
            stackCount = MaxCount;
        }
        item.stackCount = overflow;

        this.StackUpdated.Invoke(stackCount);
        item.StackUpdated.Invoke(stackCount);
        if (item.stackCount < 1)
        {
            item.ItemDestroyed.Invoke();
        }
    }

    internal void AddToInventory(InventoryGrid grid)
    {
        //dependency injection
        ParentGrid = grid;
        
        //generate our ui and add it to the grid
        GenerateUI(ParentGrid.CellSize);
        ParentGrid.RootElement.Add(UI);
    }


    /// <summary>
    /// Generates the UI of the item.
    /// </summary>
    /// <param name="CellSize"></param>
    /// <returns></returns>
    internal VisualElement GenerateUI(Vector2Int CellSize)
    {
        if (UI != null)
        {
            return UI;
        }

        //create base
        UI = new VisualElement();
        UI.style.position = Position.Absolute;
        UI.style.transformOrigin = new TransformOrigin(0f, 0f);
        


        float w = CellSize.x * ItemSpace.Width;
        float h = CellSize.y * ItemSpace.Height;


        Image img = new Image()
        {
            sprite = image,
            scaleMode = ScaleMode.StretchToFill
        };
        
        img.style.width = w;
        img.style.height = h;
        UI.Add(img);

        UI.style.width = w;
        UI.style.height = h;
        UI.style.backgroundColor = Color.red;
        UI.RegisterCallback(new EventCallback<MouseDownEvent>(MouseDownCallback));
        UI.RegisterCallback(new EventCallback<MouseUpEvent>(MouseUpCallback));
        return UI;
        
    }

    private void MouseDownCallback(MouseDownEvent _mup)
    {
        Grabbed = true;
        ParentGrid.RemoveItem(this);
    }

    private void MouseUpCallback(MouseUpEvent _mup)
    {
        Grabbed = false;
        ParentGrid.AddItem(this);
    }


    private void Update()
    {
        if (Grabbed)
        {
            UI.style.translate = ParentGrid.TargetLocation - new Vector2(UI.style.width.value.value * 0.5f, UI.style.height.value.value * 0.5f);
        }
    }

}


/// <summary>
/// The orientation of an item.
/// <br></br><br></br>
/// When designing the item inside the Inspector, you are designing the item with the Up orientation.
/// </summary>
public enum InventoryItemOrientation
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3,
    
}

