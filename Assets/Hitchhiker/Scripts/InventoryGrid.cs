using UnityEngine;

using UnityEngine.UIElements;
using static UnityEngine.InputSystem.InputAction;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
//[RequireComponent(typeof(Canvas))]
//[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(PanelRenderer))]
public class InventoryGrid : MonoBehaviour
{

    [SerializeField]
    public GridSpace InventorySpace;

    [SerializeField]
    private string BaseElementID;

    /// <summary>
    /// Whether the given inventory is able to be serialized or deserialized
    /// </summary>
    public bool Savable;

    public Vector2Int CellSize
    {
        get; private set;
    }

    public InventorySettings Config;

    public Rect Container;

    private PanelRenderer PanelRenderer;
    private void Start()
    {
        PanelRenderer = GetComponent<PanelRenderer>();
        

        PanelRenderer.RegisterUIReloadCallback(OnUIReload);


    }

    /// <summary>
    /// Toggles the visibility of the grid.
    /// </summary>
    public void ToggleGrid(CallbackContext ctx)
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    /// <summary>
    /// Gets called when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        PanelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    /// <summary>
    /// When the UI loads for the first time, this will be called.
    /// </summary>
    /// <param name="panelRenderer"></param>
    /// <param name="rootElement"></param>
    /// <param name="version"></param>
    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        
        //get our element (lol recursion)
        VisualElement inventoryBase = rootElement.SearchByID(BaseElementID);
        Debug.Assert(inventoryBase != null, "Draw ID mismatch!");
        
        //draw our content
        Rect r = inventoryBase.contentRect;
        CellSize = new Vector2Int((int)(r.width / InventorySpace.Width), (int)(r.height / InventorySpace.Height));
        Debug.Log(r);
        
        for (uint x = 0; x < InventorySpace.Width; x++)
        {
            for (uint y = 0; y < InventorySpace.Height; y++)
            {
                if (InventorySpace.SpaceOccupied(y, x))
                {

                    Image i = new Image
                    {
                        sprite = Config.CellTexture
                    };
                    i.style.position = Position.Absolute;
                    i.style.top = CellSize.y * y;
                    i.style.width = CellSize.x;
                    i.style.height = CellSize.y;
                    i.style.left = CellSize.x * x;
                    inventoryBase.Add(i);
                }

            }
        }

        PanelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

}
