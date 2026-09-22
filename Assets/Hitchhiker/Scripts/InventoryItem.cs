using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[AddComponentMenu("Hitchhiker UI/Inventory Item")]
public class InventoryItem : MonoBehaviour
{

    [SerializeField]
    /// <summary>
    /// The space that the item takes up.
    /// </summary>
    private GridSpace ItemSpace;

    /// <summary>
    /// The current space that the item takes up.
    /// </summary>
    internal GridSpace CurrentSpace
    {
        get; private set;
    }

    /// <summary>
    /// The image that the item will use to display.
    /// </summary>
    [SerializeField]
    private Sprite image;

    /// <summary>
    /// The user interface that will be shown in-game. Can be created by calling <see cref="GenerateUI(Vector2Int, InventorySettings)"/>.
    /// </summary>
    internal VisualElement UI
    {
        get; private set;
    } = null;

    /// <summary>
    /// The icon inside this item. This is only set when <see cref="GenerateUI(Vector2Int, InventorySettings)"/> is called.
    /// </summary>
    internal Image Icon
    {
        get; private set;
    } = null;



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
    

    /// <summary>
    /// The ID associated with this item. This is used to associate items of the same type.
    /// </summary>
    [SerializeField, Tooltip("The item ID associated with this Item. This is used to associate items of the same type.")]
    private string itemID;

    /// <summary>
    /// The ID associated with this item. This is used to associate items of the same type.
    /// </summary>
    internal string ItemID
    {
        get => itemID;
    }
    

    /// <summary>
    /// The maximum amount of items that can be stacked for this item. If set to 1, the item is not stackable.
    /// </summary>
    [Tooltip("The maximum amount of items that can be stacked for this item. If set to 1, the item is not stackable.\nNote: the max count range can be arbitrarily increased by editing the source code."),
        Range(1, 99), SerializeField]
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


    private Observer<int> stackCount = 1;

    /// <summary>
    /// The amount of items in this stack.
    /// </summary>
    public int StackCount
    {
        get
        {
            return stackCount.Value;
        }
        set
        {
            stackCount.Value = value;
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
    /// The grid that this item is contained in. 
    /// </summary>
    internal InventoryGrid ParentGrid;

    /// <summary>
    /// The configuration settings for this item.
    /// </summary>
    private InventorySettings Config;

    private Vector2Int CellSize;
    /// <summary>
    /// Whether the grid has reserved space for this element.
    /// </summary>
    internal bool SpaceReserved
    {
        get
        {
            return spaceReserved && InventoryPosition.Value != -Vector2Int.one;
        }
        set
        {
            spaceReserved = value;
        }
    }

    private bool spaceReserved;



    /// <summary>
    /// The position of the item inside the inventory.
    /// </summary>
    internal Observer<Vector2Int> InventoryPosition = Vector2Int.zero;

    /// <summary>
    /// The previous position of this item.
    /// </summary>
    private Vector2Int PreviousPosition;

    /// <summary>
    /// All grid items in the background of the UI.
    /// </summary>
    private List<VisualElement> GridUI = new List<VisualElement>();

    /// <summary>
    /// The orientation of the item inside the inventory. This can be saved out to a file if need be later.
    /// </summary>
    internal GridSpaceOrientation Orientation = GridSpaceOrientation.Up;

    private bool Ready;
    


    #region Unity-provided Methods
    private void Start()
    {
        CurrentSpace = ItemSpace.Duplicate();
        InventoryPosition = new Observer<Vector2Int>(-(Vector2Int)CurrentSpace, OnGridPositionUpdate);
        Ready = true;
    }

    private void Update()
    {
        if (Grabbed)
        {
            SyncPositionWithGrid();
        }
    }

    private void OnDestroy()
    {
        UI.RemoveFromHierarchy();   
    }
    #endregion

    #region Moving

    /// <summary>
    /// Syncs position with the parent grid
    /// </summary>
    internal void SyncPositionWithGrid()
    {
        InventoryPosition.Value = ParentGrid.GridMousePosition;
        UI.style.translate = ParentGrid.MousePosition;
    }
    /// <summary>
    /// Grabs this item. Sets the actiive item in the grid to this item.
    /// </summary>
    internal void GrabItem()
    {
        Grabbed = true;
        PreviousPosition = InventoryPosition;
        Icon.style.backgroundColor = new Color(0, 0, 0, 0.2f);
        ParentGrid.SetActiveItem(this);
        ParentGrid.RemoveItem(this);
    }

    /// <summary>
    /// Lets go of this item. Sets the active item in the grid to null.
    /// </summary>
    internal void LetGoOfItem()
    {
        Grabbed = false;
        Icon.style.backgroundColor = new Color(0, 0, 0, 0);
        ParentGrid.SetActiveItem(null);
        ParentGrid.AddItem(this);
    }
    #endregion

    #region Rotation

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
    /// Turns the item clockwise.
    /// </summary>
    public void TurnClockwise()
    {
        Orientation = (GridSpaceOrientation)(((int)Orientation + 1) % ((int)GridSpaceOrientation.Left + 1));
        SyncRotation();
    }

    /// <summary>
    /// Syncs up controller and view. Also updates the space by which the item takes up.
    /// </summary>
    private void SyncRotation()
    {
        CurrentSpace = ItemSpace.Rotate(GridSpaceOrientation.Up, Orientation);
        RegenerateUI();
    }

    #endregion

    #region Stacking
    /// <summary>
    /// Stacks a like item onto this item. Destroys this item if its 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public void StackInto(InventoryItem item)
    {
        // if we are already at capacity then can't do anything
        if (StackCount == MaxCount)
            //might want to emit an event here?
            return;

        int thisCount = StackCount;
        int otherCount = item.StackCount;

        item.StackCount += thisCount;
        if (item.StackCount > item.MaxCount)
        {
            int overflow = item.StackCount - item.MaxCount;
            this.StackCount = overflow;
        }
        else
        {
            this.StackCount = 0;
            Destroy(this.gameObject);
        }


    }

    #endregion

    #region Inventory Addition and Edge Case Handling
    internal void AddToInventory(InventoryGrid grid)
    {
        
        //generate our ui and add it to the grid
        GenerateUI(ParentGrid.CellSize, grid.Config);
        ParentGrid.RootElement.Add(UI);
    }

    /// <summary>
    /// Reverts the item's position. This is called by <see cref="InventoryGrid.AddItem(InventoryItem)"/> if the position is invalid.
    /// </summary>
    internal void RevertPosition()
    {
        InventoryPosition.Value = PreviousPosition;
    }

    #endregion

    #region UI Generation

    /// <summary>
    /// Regenerates UI upon changing the <see cref="CurrentSpace"/> of the object
    /// </summary>
    internal void RegenerateUI()
    {

        //clears all UI elements that are children
        UI.Clear(VisualElementClearOptions.RecursiveReleaseResources);
        GridUI.Clear();
        Background celltexture = Background.FromSprite(Config.CellTexture);

        VisualElement gridContainer = new VisualElement();
        for (uint col = 0; col < CurrentSpace.Width; col++)
        {
            for (uint row = 0; row < CurrentSpace.Height; row++)
            {
                if (CurrentSpace.SpaceOccupied(col, row))
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



        float imgw = CellSize.x * ItemSpace.Width;
        float imgh = CellSize.y * ItemSpace.Height;

        Vector2 imgPosition = new Vector2(imgw, imgh);

        float w = CellSize.x * CurrentSpace.Width;
        float h = CellSize.y * CurrentSpace.Height;

        Vector2 itemPosition = new Vector2(w, h);



        //creates icon in original orientation and rotates it
        Icon = new Image()
        {
            sprite = image,
            scaleMode = ScaleMode.StretchToFill,
            
        };
        
        Icon.style.width = imgw;
        Icon.style.position = Position.Absolute;
        Icon.style.height = imgh;
        Icon.style.rotate = new StyleRotate(new Angle((int)Orientation * Mathf.PI*0.5f, AngleUnit.Radian));

        //this is necessary to avoid offset issues for rendering.
        Icon.style.translate = (itemPosition - imgPosition) * 0.5f;
        UI.Add(Icon);


        //item stack text
        if (Stackable)
        {
            Label stackCountLabel = new Label();
            stackCount = new Observer<int>(stackCount.Value, (stack) =>
            {
                stackCountLabel.text = stackCount.Value.ToString();
                //TODO: add a different color indication for when this item is maxxed.
            });
            stackCountLabel.style.backgroundColor = new Color(0, 0, 0, 1f);
            stackCountLabel.style.position = Position.Absolute;
            stackCountLabel.style.color = new Color(1, 1, 1, 1);
            stackCountLabel.style.unityTextAlign = TextAnchor.UpperRight;
            stackCountLabel.style.transformOrigin = new TransformOrigin(0, 0);
            stackCountLabel.style.translate = new Translate(w * 0.5f, h - Config.FontSize * 1.5f);
            stackCountLabel.style.fontSize = Config.FontSize;
            stackCountLabel.style.paddingRight = Config.FontSize * 0.25f;
            stackCountLabel.style.width = w * 0.5f;
            stackCountLabel.style.height = Config.FontSize * 1.5f;
            
            
            UI.Add(stackCountLabel);
            stackCount.EmitChanged();


        }




        UI.style.width = w;
        UI.style.height = h;
        InventoryPosition.EmitChanged();

        if (Grabbed)
        {
            GrabItem();
        }

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
        this.CellSize = CellSize;
        Config = settings;
        
        if (!Ready)
        {
            Start();
        }

        //create base
        UI = new VisualElement();
        UI.style.transformOrigin = new TransformOrigin(0, 0);
        UI.style.position = Position.Absolute;

        RegenerateUI();
        
        //UI.style.backgroundColor = Color.red;
        UI.RegisterCallback(new EventCallback<MouseDownEvent>(MouseDownCallback));
        UI.RegisterCallback(new EventCallback<MouseUpEvent>(MouseUpCallback));

                
    }
    /// <summary>
    /// Gets called whenever the grid position updates.
    /// </summary>
    /// <param name="newPosition"></param>
    internal void OnGridPositionUpdate(Vector2Int newPosition)
    {
        InventoryItem potentialStacker = ParentGrid.GetItem(newPosition);
        //Debug.Log(potentialStacker?.ItemID ?? "No item at this position");
        Color c = ParentGrid.CanBePlaced(this) ? Color.green :
            potentialStacker != null && potentialStacker.ItemID == ItemID ?
            Color.blue : Color.red;

        foreach (VisualElement ve in GridUI)
        {
            ve.style.unityBackgroundImageTintColor = c;
        }
        //Debug.Log(newPosition);
    }

    
    /// <summary>
    /// What occurs when the user starts a grab.
    /// </summary>
    /// <param name="_mup"></param>
    private void MouseDownCallback(MouseDownEvent _mup)
    {
        GrabItem();
    }

    /// <summary>
    /// What occurs when the user ends a grab.
    /// </summary>
    /// <param name="_mup"></param>
    private void MouseUpCallback(MouseUpEvent _mup)
    {
        LetGoOfItem();
    }
    #endregion

    

}




