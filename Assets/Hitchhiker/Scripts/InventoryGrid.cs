using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
//[RequireComponent(typeof(Canvas))]
//[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(PanelRenderer))]
public class InventoryGrid : MonoBehaviour
{

    public bool DEBUG = true;

    [SerializeField]
    private GameObject TestingObject;


    /// <summary>
    /// The orientation and organization of the inventory itself. This has every available spot as true.
    /// </summary>
    [SerializeField]
    public GridSpace InventorySpace;

    /// <summary>
    /// The available space left in the inventory. This has every available spot as true.
    /// </summary>
    internal GridSpace AvailableSpace;

    [SerializeField, Tooltip("If specifying a custom layout in the Panel Renderer, this should point to a VisualElement that" +
        "can house every cell. This VisualElement can have children, as the grid will absolutely position the background.")]
    private string BaseElementID = "InventoryBase";

    /// <summary>
    /// If the inventory has a save path.
    /// </summary>
    [Tooltip("The save path.")]
    public string SavePath;

    /// <summary>
    /// The size that the cells are. Calculated on setup.
    /// </summary>
    public Vector2Int CellSize
    {
        get; private set;
    }

    [Tooltip("The configuration of the inventory. This is a required element for the Inventory to function!")]
    public InventorySettings Config;


    internal VisualElement RootElement;
    private VisualElement InventoryBase;

    private InventoryItem CurrentItem;

    internal Vector2 TargetLocation
    {
        get; private set;
    }

    /// <summary>
    /// The items currently in the grid.
    /// </summary>
    private readonly List<InventoryItem> Items = new List<InventoryItem>();
    
    private PanelRenderer PanelRenderer;
    private void Start()
    {
        PanelRenderer = GetComponent<PanelRenderer>();
        PanelRenderer.RegisterUIReloadCallback(OnUIReload);

        AvailableSpace = InventorySpace.Duplicate();

        


    }

    
    /// <summary>
    /// Toggles the visibility of the grid. Use <see cref="SetGridVisibility(bool)"/> if you want to manually open/close the inventory through code.
    /// </summary>
    public void ToggleGrid(CallbackContext _ctx)
    {
        SetGridVisibility(!gameObject.activeSelf);
    }

    /// <summary>
    /// Sets the visibility of the grid.
    /// </summary>
    /// <param name="visible"></param>
    public void SetGridVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
    /// <summary>
    /// Opens the inventory if not already open.
    /// </summary>
    public void OpenInventory() => SetGridVisibility(true);

    /// <summary>
    /// Closes the inventory if not already closed.
    /// </summary>
    public void CloseInventory() => SetGridVisibility(false);

    /// <summary>
    /// Gets the grid position that the mouse is currently over.
    /// </summary>
    /// <returns></returns>
    public Vector2Int GridMousePosition
    {
        get
        {
            
            Vector2 delta = TargetLocation - InventoryBase.GetAbsoluteLocation();
            int dx = Mathf.RoundToInt(delta.x / CellSize.x);
            int dy = Mathf.RoundToInt(delta.y / CellSize.y);
            //Debug.Log($"Target location: {TargetLocation}. Delta: {delta}");
            Debug.Log($"Delta: {delta}. grid DX: {dx}. grid DY: {dy}");
            return new Vector2Int(dx, dy);
        }
    }

    /// <summary>
    /// Gets the position of the mouse in local space of <see cref="InventoryBase"/>.
    /// </summary>
    public Vector2 MousePosition
    {
        get
        {
            return InventoryBase.GetAbsoluteLocation() + GridMousePosition * CellSize;
        }
    }

    /// <summary>
    /// When the UI loads for the first time, this will be called.
    /// </summary>
    /// <param name="panelRenderer"></param>
    /// <param name="rootElement"></param>
    /// <param name="version"></param>
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {

        RootElement = rootElement;
        if (DEBUG)
        {
            Button spawnButton = new Button(SpawnItem) { text = "Spawn Inventory Item" };
            RootElement.Add(spawnButton);
        }

        //get our root element
        InventoryBase = RootElement.SearchByID(BaseElementID);
        Debug.Assert(InventoryBase != null, $"Unable to find the base of the inventory! Please make sure this is the correct ID: {BaseElementID}");
        
        
        //draw our content
        Rect r = InventoryBase.contentRect;

        //calculate proper cell size
        int cellsizex = (int)(r.width / InventorySpace.Width);
        int cellsizey = (int)(r.height / InventorySpace.Height);
        CellSize = new Vector2Int(Mathf.Min(cellsizex, cellsizey), Mathf.Min(cellsizex, cellsizey));

        //if we resize let the user know
        if (CellSize.x != cellsizex ^ CellSize.y != cellsizey)
        {
            InventoryBase.style.width = CellSize.x * InventorySpace.Width;
            InventoryBase.style.height = CellSize.y * InventorySpace.Height;
            Debug.Log($"Inventory has been auto-resized to keep aspect ratio. New size: {InventoryBase.contentRect.size}");
        }
        
        //create tiles for the proper spaces
        for (uint x = 0; x < InventorySpace.Width; x++)
        {
            for (uint y = 0; y < InventorySpace.Height; y++)
            {
                if (InventorySpace.SpaceOccupied(y, x))
                {

                    Image i = CreateTileImage(x, y);
                    InventoryBase.Add(i);
                }

            }
        }

        RootElement.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
        PanelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    /// <summary>
    /// Spawns a test item.
    /// </summary>
    private void SpawnItem()
    {
        GameObject o = Instantiate(TestingObject);
        o.transform.parent = transform;
        AddItem(o.GetComponent<InventoryItem>());
        
    }

    ///// <summary>
    ///// Adds item to the UI hierarchy.
    ///// </summary>
    ///// <param name="item"></param>
    //public void AddItemToUI(InventoryItem item)
    //{
    //    VisualElement e = BuildItemUI(item);
    //    InventoryBase.Add(e);
    //}


    //public VisualElement BuildItemUI(InventoryItem item)
    //{
    //    VisualElement ui = new VisualElement
    //    {
    //        userData = item
    //    };
    //    ui.style.position = Position.Absolute;
        
    //    //draw the boxes
    //    for (uint row = 0; row < item.ItemSpace.Height; row++)
    //    {
    //        for (uint col = 0; col < item.ItemSpace.Width; col++)
    //        {
    //            if (item.ItemSpace.SpaceOccupied(row, col))
    //            {
    //                Image i = CreateTileImage(col, row, false);

    //                ui.Add(i);
    //            }
    //        }
    //    }

    //    //draw the overlay
    //    Image a = new Image
    //    {
    //        sprite = item.Image
    //    };
        
    //    a.style.position = Position.Absolute;
        
    //    a.style.width = CellSize.x * item.ItemSpace.Width;
    //    a.style.height = CellSize.y * item.ItemSpace.Height;
    //    a.style.top = 0;
    //    a.style.left = 0;
    //    a.style.backgroundColor = new Color(0, 0, 0, 0.5f);
    //    ui.Add(a);


    //    //handle the callbacks
    //    ui.RegisterCallback(new EventCallback<MouseDownEvent>(OnItemMouseDown));
    //    return ui;
    //}

    //private void OnItemMouseDown(MouseDownEvent e)
    //{
    //    VisualElement ui = e.target as VisualElement;
    //    InventoryItem item = ui.userData as InventoryItem;
    //    RemoveItem(item);
    //    Debug.Log("Mouse clicked on item!");

    //    CurrentItem = ui;
    //}

    //private void OnMouseUpUI(MouseUpEvent e)
    //{
    //    InventoryItem item = CurrentItem.userData as InventoryItem;
    //    //TODO: add item
    //    Debug.Log("Mouse let go of item!");
    //    CurrentItem = null;
    //    AddItem(item);
    //}

    private void OnMouseMove(MouseMoveEvent e)
    {
        TargetLocation = e.localMousePosition;
    }



    /// <summary>
    /// Creates an image to add to the background of the grid inventory system.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="row"></param>
    /// <returns></returns>
    private Image CreateTileImage(float col, float row, bool addCallbacks = true)
    {
        Image i = new Image
        {
            sprite = Config.CellTexture
        };
        if (addCallbacks)
        {
            i.RegisterCallback(new EventCallback<MouseOverEvent>(OnMouseOverBackgroundImageCallback));
            i.RegisterCallback(new EventCallback<MouseOutEvent>(OnMouseOutBackgroundImageCallback));
        }
        i.style.position = Position.Absolute;
        i.style.backgroundColor = Config.BackgroundColor;
        i.style.top = CellSize.y * row;
        i.style.width = CellSize.x;
        i.style.height = CellSize.y;
        i.style.left = CellSize.x * col;
        return i;
    }

    

    /// <summary>
    /// TODO: make this a different color and have this be more modular
    /// </summary>
    /// <param name="e"></param>
    private void OnMouseOverBackgroundImageCallback(MouseOverEvent e)
    {
        Image i = e.target as Image;
        i.tintColor = Color.red;
    }


    /// <summary>
    /// TODO: make this a different color and have this be more modular
    /// </summary>
    /// <param name="e"></param>
    private void OnMouseOutBackgroundImageCallback(MouseOutEvent e)
    {
        Image i = e.target as Image;
        i.tintColor = Color.white;
    }

    /// <summary>
    /// Adds an item to the inventory. 
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(InventoryItem item)
    {
        if (item == null)
        {
            Debug.Log("Item is null!");
            return;
        }
        InventoryItem potentialMatch = Items.Find((i) => i.ItemID == item.ItemID);
        if (potentialMatch && potentialMatch.Stackable)
        {
            potentialMatch.StackItem(item);
            //check for alive-ness maybe?
               
        }
        else
        {
            Items.Add(item);
            item.AddToInventory(this);

        }
    }

    public void RemoveItem(InventoryItem item)
    {
        Items.Remove(item);
    }

    public void PlaceNearest(InventoryItem item)
    {

        //TODO calculate the exact place to put the item
        PlaceItem(item);
    }

    public bool PlaceItem(InventoryItem item)
    {

        //TODO: place the item
        return true;
    }
}



