using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
//[RequireComponent(typeof(Canvas))]
//[RequireComponent(typeof(CanvasRenderer))]
[RequireComponent(typeof(PanelRenderer))]
public class InventoryGrid : MonoBehaviour
{

    [SerializeField]
    public GridSpace InventorySpace;

    /// <summary>
    /// Whether the given inventory is able to be serialized or deserialized
    /// </summary>
    public bool Savable;

    public Vector2Int CellSize
    {
        get
        {
            return Vector2Int.zero;
            //return new Vector2Int((int)(Renderer.visualTreeAsset as TemplateContainer).worldBound.width / (int)InventorySpace.Width, (int)Canvas.pixelRect.height / (int)InventorySpace.Height);
        }
    }

    public InventorySettings Config;

    public Rect Container;

    private PanelRenderer PanelRenderer;
    private void Start()
    {
        PanelRenderer = GetComponent<PanelRenderer>();
        Vector2Int cs = CellSize;

        PanelRenderer.RegisterUIReloadCallback(OnUIReload);


    }

    private void OnDestroy()
    {
        PanelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement rootElement, int version)
    {
        Debug.Log("Reload of the root!");
        Debug.Log(rootElement.name);
        
    }

}
