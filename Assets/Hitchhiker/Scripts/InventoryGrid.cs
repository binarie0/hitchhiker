using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
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

    internal Vector2 MousePosition
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

            //convert world mouse coords to local inventory coords
            Vector2 delta = RootElement.ChangeCoordinatesTo(InventoryBase, MousePosition);

            int dx = Mathf.RoundToInt(delta.x / CellSize.x);
            int dy = Mathf.RoundToInt(delta.y / CellSize.y);

            //Debug.Log($"Delta: {delta}. grid DX: {dx}. grid DY: {dy}");
            return new Vector2Int(dx, dy);
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


    private void OnMouseMove(MouseMoveEvent e)
    {
        MousePosition = e.localMousePosition - new Vector2(CellSize.x, CellSize.y) * 0.5f;
        
    }

    

    /// <summary>
    /// Converts a grid space into world space coordinates.
    /// </summary>
    /// <param name="gridSpace"></param>
    /// <returns></returns>
    internal Vector2 ConvertToWorldSpace(Vector2Int gridSpace)
    {
        return InventoryBase.ChangeCoordinatesTo(RootElement, gridSpace * CellSize);
    }

    /// <summary>
    /// Returns whether this inventory can place the item at the specified position.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal bool CanBePlaced(InventoryItem item)
    {
        return AvailableSpace.CanAccommodate(item.ItemSpace, item.InventoryPosition);
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
        
        //if we cannot place the item then move back
        if (!CanBePlaced(item))
        {
            item.RevertPosition();
        }

        item.UIPosition = ConvertToWorldSpace(item.InventoryPosition);
        
        InventoryItem potentialMatch = Items.Find((i) => i.ItemID == item.ItemID);
        if (potentialMatch && potentialMatch.Stackable)
        {
            potentialMatch.StackItem(item);
            //check for alive-ness maybe?
               
        }
        else
        {
            InitItem(item);
        }
    }

    /// <summary>
    /// Sets up an item to be visually displayed in the inventory.
    /// </summary>
    /// <param name="item"></param>
    private void InitItem(InventoryItem item)
    {

        //add item to inventory
        Items.Add(item);

        
        AvailableSpace.MarkSpaceAsReserved(item.ItemSpace, item.InventoryPosition);
        
        //setup required item parts
        item.ParentGrid = this;
        item.GenerateUI(CellSize);
        
        //add item to UI if we haven't already
        if (!RootElement.Contains(item.UI))
        {
            RootElement.Add(item.UI);
        }
    }

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="item"></param>
    public void RemoveItem(InventoryItem item)
    {
        Items.Remove(item);
        //item.ParentGrid = null;
        AvailableSpace.UnreserveSpace(item.ItemSpace, item.InventoryPosition);
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



