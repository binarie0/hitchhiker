# InventoryItem

An inventory item is an item that can be slooted in an InventoryGrid. This has its own space as well as its own setup parameters.

## Members

`ItemSpace: GridSpace`
The original space that the item takes up. This is equivalent to the space when the item's orientation is `GridSpaceOrientation.Up`.

`CurrentSpace: GridSpace`
The current space the item takes up. This accounts for the item's orientation.

`Image: Sprite`
The sprite that will be displayed for its image. It is advised to scale this by a known factor, as this sprite is arbirarily scaled to the item's dimensions, which may lead to stretching/squashing.

`UI: VisualElement`
The user interface parent shown during run-time. This is created when `GenerateUI(Vector2Int, InventorySettings)` is called.

`Icon: Image`
The icon VisualElement inside this item. Set alongside `UI`

`UIPosition: Vector2`
The position of the UI in world space.

`Grabbed: bool`
Whether the item has been grabbed.

`ItemID: string`
The unique ID given to this inventory item. This should be unique per prefab.

`MaxCount: int`
The maximum count of items in a stack. This can be between 1 and 99. If set to 1, `Stackable` will be false.

`StackCount: int`
The current amount of items in this stack. Triggers an event on set.

`Stackable: bool`
Whether the item is stackable. This is equivalent to checking whether the max count is greater than 1.

`ParentGrid: InventoryGrid`
The grid this item is contained in. Set outside during initialization.

`Config: InventorySettings`
The configuration settings for this item. Set alongside `UI`.

`CellSize: Vector2Int`
An internal copy of the cell size of the grid. This is used during regeneration of UI.

`SpaceReserved: bool`
Whether this grid has reserved any space.

`InventoryPosition: Observer<Vector2Int>`
The position of the item inside the inventory. Triggers an event on set.

`PreviousPosition: Vector2Int`
The previous position of this element. Used during placement to undo changes.

`GridUI: List<VisualElement>`
All grid images in the background. Used during callbacks to set colors to indicate availability of underlying space.

`Orientation: GridSpaceOrientation`
The current orientation of this item.

`Ready: bool`
Whether this item is ready. This is used by some testing methods as oftentimes items are created too quickly.

## Methods

`SyncPositionWithGrid() -> void`
Syncs position with the parent grid.

`GrabItem() -> void`
Marks this item as being grabbed and lets the parent grid know.

`MouseDownCallback(_mup: MouseDownEvent)`
A callback for when the mouse is down on `UI`. Calls `GrabItem()`

`LetGoOfItem() -> void`
Lets go of this item. Lets the parent grid know.

`MouseUpCallback(_mup: MouseUpEvent)`
A callback for when the mouse is up on `UI`. Calls `LetGoOfItem()`

`TurnCounterclockwise() / TurnClockwise() -> void`
Changes the orientation in a set direction.

`SyncRotation() -> void`
Syncs up this controller and the visuals. Sets `CurrentSpace` and calls `RegenerateUI()`

`StackInto(item: InventoryItem)`
Stacks this item into another item. If this stack is gone, destroys this item.

`RevertPosition() -> void`
Reverts this item's position back to when it was originally grabbed.

`RegenerateUI() -> void`
Regenerates UI. This gets called whenever a rotation occurs, since a stylistic rotation would not sync positions up.

`GenerateUI(CellSize: Vector2Int, settings: InventorySettings) -> void`
Sets up the UI for generation. Registers callbacks and then calls `RegenerateUI()`

`OnGridPositionUpdate(newPosition: Vector2Int)`
This callback is run whenever `GridPosition` is changed. This checks whether this can be placed and/or stacked and then gives a visual indicator.
