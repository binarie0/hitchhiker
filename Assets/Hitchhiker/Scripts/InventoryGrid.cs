using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
[RequireComponent(typeof(PanelRenderer))]
public class InventoryGrid : MonoBehaviour
{
    #region Events & Delegates

    public UnityEvent GridRendered = new UnityEvent();
    #endregion


    /// <summary>
    /// The orientation and organization of the inventory itself. This has every available spot as true.
    /// </summary>
    [SerializeField]
    private GridSpace InventorySpace;

    /// <summary>
    /// The available space left in the inventory. This has every available spot as true.
    /// </summary>
    internal GridSpace AvailableSpace;

    [SerializeField, Tooltip("If specifying a custom layout in the Panel Renderer, this should point to a VisualElement that" +
        "can house every cell. This VisualElement can have children, as the grid will absolutely position the background.")]
    private string BaseElementID = "InventoryBase";

    

    /// <summary>
    /// The size that the cells are. Calculated on setup.
    /// </summary>
    public Vector2Int CellSize
    {
        get; private set;
    }

    /// <summary>
    /// The configuration of the inventory. This is a required element for the Inventory to function!
    /// </summary>
    public InventorySettings Config
    {
        get
        {
            return config;
        }
    }


    [Tooltip("The configuration of the inventory. This is a required element for the Inventory to function!"),
        SerializeField]
    private InventorySettings config;

    /// <summary>
    /// The root element of the UI hierarchy.
    /// </summary>
    internal VisualElement RootElement
    {
        get
        {
            return rootElement;
        }
    }
    private VisualElement rootElement;

    /// <summary>
    /// The base element where all the inventory cells will be created.
    /// </summary>
    private VisualElement InventoryBase;

    /// <summary>
    /// The current item in the inventory.
    /// </summary>
    private InventoryItem CurrentItem;

    /// <summary>
    /// The mouse position in world space.
    /// </summary>
    internal Vector2 MousePosition
    {
        get; private set;
    }

    /// <summary>
    /// Gets the grid position that the mouse is currently over. This is compared to local space of <see cref="InventoryBase"/>
    /// </summary>
    /// <returns></returns>
    public Vector2Int GridMousePosition
    {
        get
        {

            //convert world mouse coords to local inventory coords
            Vector2 delta = rootElement.ChangeCoordinatesTo(InventoryBase, MousePosition);

            int dx = Mathf.RoundToInt(delta.x / CellSize.x);
            int dy = Mathf.RoundToInt(delta.y / CellSize.y);

            //Debug.Log($"Delta: {delta}. grid DX: {dx}. grid DY: {dy}");
            return new Vector2Int(dx, dy);
        }
    }
    /// <summary>
    /// The items currently in the grid.
    /// </summary>
    private readonly List<InventoryItem> Items = new List<InventoryItem>();
    
    /// <summary>
    /// The panel renderer that will draw out the UI.
    /// </summary>
    private PanelRenderer PanelRenderer;

    #region Unity-provided Methods
    /// <summary>
    /// Called on start.
    /// </summary>
    private void Start()
    {

        //get our renderer
        PanelRenderer = GetComponent<PanelRenderer>();
        
        //when the renderer reloads, build our real UI.
        PanelRenderer.RegisterUIReloadCallback(OnUIReload);

        //make a dupe to use as a space partition
        AvailableSpace = InventorySpace.Duplicate();
    }

    #endregion

    #region Grid Visibility
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

    #endregion

    #region Adding / Removing Items

    /// <summary>
    /// Returns whether this inventory can place the item at the specified position.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    internal bool CanBePlaced(InventoryItem item)
    {
        return AvailableSpace.CanAccommodate(item.CurrentSpace, item.InventoryPosition);
    }

    /// <summary>
    /// Adds an item to the inventory. 
    /// </summary>
    /// <param name="item"></param>
    public void AddItem(InventoryItem item)
    {
        Debug.Assert(item != null);

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
    /// If the item does not need to reserve space (i.e. we are creating items in a junk drawer to the right of the inventory)
    /// then we don't need to set up the item to reserve space.
    /// </summary>
    /// <param name="item"></param>
    internal void InitItem(InventoryItem item, bool reserveSpace = true)
    {

        //add item to inventory
        Items.Add(item);

        if (reserveSpace)
        {
            item.SpaceReserved = true;
            AvailableSpace.MarkSpaceAsReserved(item.CurrentSpace, item.InventoryPosition);
        }

        //setup required item parts
        item.ParentGrid = this;
        item.GenerateUI(CellSize, Config);

        //add item to UI if we haven't already
        if (!rootElement.Contains(item.UI))
        {
            rootElement.Add(item.UI);
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
        if (item.SpaceReserved)
        {
            AvailableSpace.UnreserveSpace(item.CurrentSpace, item.InventoryPosition);
        }
        item.SpaceReserved = false;
    }

    /// <summary>
    /// Places the item in the nearest slot.
    /// </summary>
    /// <param name="item"></param>
    public void PlaceNearest(InventoryItem item)
    {
        //TODO calculate the exact place to put the item
        AddItem(item);
    }

    #endregion

    #region Math Calculations

    /// <summary>
    /// Converts a grid space into world space coordinates.
    /// </summary>
    /// <param name="gridSpace"></param>
    /// <returns></returns>
    internal Vector2 ConvertToWorldSpace(Vector2Int gridSpace)
    {
        return InventoryBase.ChangeCoordinatesTo(rootElement, gridSpace * CellSize);
    }
    #endregion


    #region Moving & Rotating of Items
    /// <summary>
    /// Sets the active item (for use of rotation for right now).
    /// </summary>
    /// <param name="item"></param>
    internal void SetActiveItem(InventoryItem item)
    {
        CurrentItem = item;
    }


    #region Rotation

    /// <summary>
    /// Rotates the active item in the inventory. Use <see cref="RotateActiveItem()"/> for a more direct call.
    /// </summary>
    /// <param name="_ctx"></param>
    public void RotateActiveItem(CallbackContext _ctx)
    {

        //if we just pressed the button, we can rotate
        if (_ctx.ReadValueAsButton() && _ctx.performed)
        {
            RotateActiveItem();
        }
    }

    /// <summary>
    /// Rotates the active item (the item being held) in the inventory.
    /// </summary>
    public void RotateActiveItem()
    {

        //ignore if the user is pressing R for no reason
        if (CurrentItem == null)
            return;

        switch (Config.RotationDirection)
        {
            case RotationDirection.None:
                break;
            case RotationDirection.Clockwise:
                CurrentItem.TurnClockwise();
                break;
            case RotationDirection.Counterclockwise:
                CurrentItem.TurnCounterclockwise();
                break;
            default:
                Debug.Break();
                break;
        }


    }

    #endregion

    #endregion

    #region Inventory & Item Rendering

    /// <summary>
    /// When the UI loads for the first time, this will be called.
    /// </summary>
    /// <param name="panelRenderer"></param>
    /// <param name="rootElement"></param>
    /// <param name="version"></param>
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {

        this.rootElement = rootElement;


        //get our root element
        InventoryBase = this.rootElement.SearchByID(BaseElementID);
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
                if (InventorySpace.SpaceOccupied(x, y))
                {

                    Image i = CreateTileImage(x, y);
                    InventoryBase.Add(i);
                }

            }
        }

        //register global event and do not sync redraws
        this.rootElement.RegisterCallback(new EventCallback<MouseMoveEvent>(OnMouseMove));
        PanelRenderer.UnregisterUIReloadCallback(OnUIReload);

        GridRendered.Invoke();
    }

    


    private void OnMouseMove(MouseMoveEvent e)
    {
        MousePosition = e.localMousePosition - new Vector2(CellSize.x, CellSize.y) * 0.5f;
        
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

    #endregion



}



