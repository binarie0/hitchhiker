using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[AddComponentMenu("Hitchhiker UI/Inventory Item")]
public class InventoryItem : MonoBehaviour
{
    public const int Max_Item_Stack = 99;

    [SerializeField]
    /// <summary>
    /// The space that the item takes up.
    /// </summary>
    private GridSpace ItemSpace;

    internal GridSpace CurrentSpace;

    /// <summary>
    /// The image that the item will use to display.
    /// </summary>
    [SerializeField]
    private Sprite image;

    /// <summary>
    /// The user interface that will be shown in-game. Can be created by calling 
    /// </summary>
    internal VisualElement UI
    {
        get; private set;
    }



    /// <summary>
    /// The position of the UI in world space.
    /// </summary>
    internal Vector2 UIPosition
    {
        get
        {
            if (UI == null)
                return Vector2.zero;
            return UI.style.translate.ToVector2();
        }
        set
        {
            if (UI != null)
            {
                UI.style.translate = value;
            }
        }
    }

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
    internal InventoryGrid ParentGrid;

    /// <summary>
    /// Whether the grid has reserved space for this element.
    /// </summary>
    internal bool SpaceReserved;



    /// <summary>
    /// The position of the item inside the inventory.
    /// </summary>
    internal Observer<Vector2Int> InventoryPosition = Vector2Int.zero;

    private Vector2Int PreviousPosition;

    private List<VisualElement> GridUI = new List<VisualElement>();

    /// <summary>
    /// The orientation of the item inside the inventory. This can be saved out to a file if need be later.
    /// </summary>
    internal GridSpaceOrientation Orientation = GridSpaceOrientation.Up;

    /// <summary>
    /// The current rotation to show the item in. This is ascertained through <see cref="Orientation"/>.
    /// </summary>
    public float Rotation
    {
        get
        {
            return -Mathf.PI * 0.5f * (int)Orientation;
        }
    }

    /// <summary>
    /// Turns the item clockwise.
    /// </summary>
    public void TurnClockwise()
    {
        Orientation = (GridSpaceOrientation)(((int)Orientation + 1) % ((int)GridSpaceOrientation.Left + 1));
        SyncRotation();
    }

    private void SyncRotation()
    {
        UI.style.rotate = new Rotate(new Angle(Rotation, AngleUnit.Radian));
        CurrentSpace = ItemSpace.Rotate(GridSpaceOrientation.Up, Orientation);
    }

    private void Start()
    {
        CurrentSpace = ItemSpace.Duplicate();
    }

    /// <summary>
    /// Turns the item counterclockwise.
    /// </summary>
    public void TurnCounterclockwise()
    {
        int n = (int)(Orientation) - 1;
        if (n < 0)
        {
            n += ((int)GridSpaceOrientation.Left + 1);
        }
        Orientation = (GridSpaceOrientation)n;
        SyncRotation();
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
        
        //generate our ui and add it to the grid
        GenerateUI(ParentGrid.CellSize, grid.Config);
        ParentGrid.RootElement.Add(UI);
    }

    internal void RevertPosition()
    {
        InventoryPosition.Value = PreviousPosition;
    }

    /// <summary>
    /// Generates the UI of the item.
    /// </summary>
    /// <param name="CellSize"></param>
    /// <returns></returns>
    internal void GenerateUI(Vector2Int CellSize, InventorySettings settings)
    {
        if (UI != null)
        {
            return;
        }

        //create base
        UI = new VisualElement();
        UI.style.transformOrigin = new TransformOrigin(0, 0);
        UI.style.position = Position.Absolute;

        Background celltexture = Background.FromSprite(settings.CellTexture);

        VisualElement gridContainer = new VisualElement();
        for (uint col = 0; col < ItemSpace.Width; col++)
        {
            for (uint row = 0; row < ItemSpace.Height; row++)
            {
                if (ItemSpace.SpaceOccupied(row, col))
                {
                    var ve = new VisualElement();
                    ve.style.backgroundImage = celltexture;
                    ve.style.width = CellSize.x;
                    ve.style.height = CellSize.y;
                    ve.style.position = Position.Absolute;
                    ve.style.top =0; ve.style.left = 0;
                    ve.style.translate = new Translate(CellSize.x * col, CellSize.y * row);
                    gridContainer.Add(ve);
                    GridUI.Add(ve);
                }
            }
        }
        gridContainer.style.position = Position.Absolute;
        UI.Add(gridContainer);
        //UI.style.backgroundImage = Background.FromSprite(settings.CellTexture);
        //UI.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left);
        //UI.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top);
        
        //UI.style.backgroundSize = new BackgroundSize(CellSize.x, CellSize.y);
        //UI.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
        


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
        //UI.style.backgroundColor = Color.red;
        UI.RegisterCallback(new EventCallback<MouseDownEvent>(MouseDownCallback));
        UI.RegisterCallback(new EventCallback<MouseUpEvent>(MouseUpCallback));

        InventoryPosition.ValueChanged += OnGridPositionUpdate;        
    }


    /// <summary>
    /// Gets called whenever the grid position updates.
    /// </summary>
    /// <param name="newPosition"></param>
    internal void OnGridPositionUpdate(Vector2Int newPosition)
    {
        foreach (VisualElement ve in GridUI)
        {
            ve.style.unityBackgroundImageTintColor = ParentGrid.CanBePlaced(this) ? Color.green : Color.red;
        }
        Debug.Log(newPosition);
    }

    /// <summary>
    /// What occurs when the user starts a grab.
    /// </summary>
    /// <param name="_mup"></param>
    private void MouseDownCallback(MouseDownEvent _mup)
    {
        Grabbed = true;
        PreviousPosition = InventoryPosition;

        ParentGrid.SetActiveItem(this);
        ParentGrid.RemoveItem(this);
    }

    /// <summary>
    /// What occurs when the user ends a grab.
    /// </summary>
    /// <param name="_mup"></param>
    private void MouseUpCallback(MouseUpEvent _mup)
    {
        Grabbed = false;
        ParentGrid.SetActiveItem(null);
        ParentGrid.AddItem(this);
    }



    private void Update()
    {
        if (Grabbed)
        {
            InventoryPosition.Value = ParentGrid.GridMousePosition;
            UI.style.translate = ParentGrid.MousePosition;

            //UI.style.translate = ParentGrid.TargetLocation - new Vector2(UI.style.width.value.value * 0.5f, UI.style.height.value.value * 0.5f);
        }
    }

}




