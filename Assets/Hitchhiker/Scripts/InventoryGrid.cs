using UnityEngine;
using UnityEngine.UI;
[AddComponentMenu("Hitchhiker UI/Inventory Grid")]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasRenderer))]
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
            return new Vector2Int((int)Canvas.pixelRect.width / (int)InventorySpace.Width, (int)Canvas.pixelRect.height / (int)InventorySpace.Height);
        }
    }

    public InventorySettings Config;

    public Rect Container;

    private Canvas Canvas;
    private void Start()
    {
        Canvas = GetComponent<Canvas>();
        Vector2Int cs = CellSize;
        for (uint y = 0; y < InventorySpace.Height; y++)
        {
            for (uint x = 0; x < InventorySpace.Width; x++)
            {
                if (InventorySpace.SpaceOccupied(x, y))
                {
                    GameObject o = new GameObject();

                    Image i = o.AddComponent<Image>();
                    i.sprite = Config.CellTexture;
                    i.rectTransform.position = new Vector2(Container.x + cs.x * x, Container.y + cs.y * y);
                    o.transform.SetParent(transform, false);
                }
            }
        }

    }
}
