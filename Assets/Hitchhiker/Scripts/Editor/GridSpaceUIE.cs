using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;


[CustomPropertyDrawer(typeof(GridSpace))]
public class GridSpaceUIE : PropertyDrawer
{
    /// <summary>
    /// The container that houses the grid space UI.
    /// </summary>
    VisualElement ParentContainer;

    /// <summary>
    /// The serialized property of the GridSpace object's width.
    /// </summary>
    SerializedProperty WidthSerializedProperty;

    /// <summary>
    /// The serialized property of the GridSpace object's height.
    /// </summary>
    SerializedProperty HeightSerializedProperty;

    /// <summary>
    /// The serialized property of the GridSpace object's spacial size.
    /// </summary>
    SerializedProperty RowArraySerializedProperty;

    /// <summary>
    /// The container that houses all the buttons for the spacial grid.
    /// </summary>
    VisualElement GridContainer;


    

    /// <summary>
    /// Refreshes the grid and syncs up the visual aid with the properties set above.
    /// Copies over all previous data as well.
    /// </summary>
    private void RefreshGrid()
    {
        //remove grid if the grid exists
        if (GridContainer != null && ParentContainer.Contains(GridContainer))
            ParentContainer.Remove(GridContainer);

        //resize the inventory
        int previousSize = RowArraySerializedProperty.arraySize;
        RowArraySerializedProperty.arraySize = (int)HeightSerializedProperty.uintValue;
        
        //copy data over to a temp array
        uint[] arr = new uint[previousSize];
        for (int i = 0; i <  previousSize; i++)
        {
            arr[i] = i >= RowArraySerializedProperty.arraySize ? 
                0 : RowArraySerializedProperty.GetArrayElementAtIndex(i).uintValue;
        }

        //copy data back, filling in blanks if needed
        for (int i = 0; i < RowArraySerializedProperty.arraySize; i++)
        {
            RowArraySerializedProperty.GetArrayElementAtIndex(i).uintValue = i < arr.Length ? 
                arr[i] & ~(uint.MaxValue << (int)WidthSerializedProperty.uintValue) : 
                0u;
            
        }

        //generate our grid and add to container
        GridContainer = CreateGrid();
        ParentContainer.Add(GridContainer);

        PropogateChanges();
    }

    /// <summary>
    /// Updates the SerializedObject on file.
    /// </summary>
    private void PropogateChanges()
    {
        //propogate changes
        RowArraySerializedProperty.serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Creates a visual grid underneath the width/height data.
    /// </summary>
    /// <returns></returns>
    private VisualElement CreateGrid()
    {
        var c = new VisualElement();
        uint rows = HeightSerializedProperty.uintValue;
        uint columns = WidthSerializedProperty.uintValue;

        c.style.marginTop = 5;
        c.style.marginLeft = 15;
        for (uint row = 0; row < rows; row++)
        {
            var rowContainer = new VisualElement();

            rowContainer.style.flexDirection = FlexDirection.Row;

            for (uint col = 0; col < columns; col++)
            {
                
                rowContainer.Add(CreateToggle((int)row, (int)col));
            }
            c.Add(rowContainer);
        }
        return c;
    }

    /// <summary>
    /// Returns whether a specific bit has been flipped.
    /// </summary>
    /// <param name="row">The row to check.</param>
    /// <param name="col">The column to check.</param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException"></exception>
    private bool IsValueOn(int row, int col)
    {

        //handle OOB checks
        if (row >= RowArraySerializedProperty.arraySize || col >= WidthSerializedProperty.uintValue)
        {
            return false;
        }

        //check bit value
        return (RowArraySerializedProperty.GetArrayElementAtIndex(row).uintValue & (1u << col)) != 0;
    }

    /// <summary>
    /// Creates a toggle button for the grid.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="col"></param>
    /// <returns></returns>
    private Toggle CreateToggle(int row, int col)
    {
        //set up toggle with default value
        Toggle t = new Toggle
        {
            value = (IsValueOn(row, col))
        };

        //whenever a button is pressed we can update the underlying value
        
        //this needs to be a lambda to be able to pass through row/col
        t.onValidateValue += (e) =>
        {
            PropogateToggle(e, row, col);
            return e;
        };
        return t;
    }

    /// <summary>
    /// Updates the proper bit that needs to be updated.
    /// </summary>
    /// <param name="on"></param>
    /// <param name="row"></param>
    /// <param name="col"></param>
    private void PropogateToggle(bool on, int row, int col)
    {

        //if there is a change in value
        if (on != IsValueOn(row, col))
        {
            //toggle the value. this will be always synced with the state since the initial button state reflects the array.
            RowArraySerializedProperty.GetArrayElementAtIndex(row).uintValue ^= (1u << (int)col);
            
            RowArraySerializedProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    /// <summary>
    /// Fills every square specified in the grid.
    /// This will redraw the grid
    /// </summary>
    private void FloodFill()
    {
        CopyToAll(~(uint.MaxValue << (int)WidthSerializedProperty.uintValue));
    }

    /// <summary>
    /// Clears all values from the grid
    /// </summary>
    private void Clear()
    {
        CopyToAll(0);
    }
    /// <summary>
    /// Copies a value to every row on the grid
    /// </summary>
    /// <param name="value"></param>
    private void CopyToAll(uint value)
    {
        //sets every row to the max value we can possibly make it
        for (int i = 0; i < RowArraySerializedProperty.arraySize; i++)
        {
            RowArraySerializedProperty.GetArrayElementAtIndex(i).uintValue = value;
        }
        RefreshGrid();
    }

    
    
    /// <summary>
    /// Whenever a grid size changes, make sure to refresh the grid.
    /// </summary>
    /// <param name="p"></param>
    private void OnGridSizeValueChange(SerializedProperty p)
    {
        RefreshGrid();
    }

    /// <summary>
    /// Creates the grid in the Inspector View.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        ParentContainer = new VisualElement();
        //set up default
        WidthSerializedProperty = property.FindPropertyRelative("Width");
        HeightSerializedProperty = property.FindPropertyRelative("Height");
        RowArraySerializedProperty = property.FindPropertyRelative("Rows");

        ParentContainer.style.marginTop = 8;
        ParentContainer.style.marginBottom = 8;
        //get header
        var header = new Label("Grid Size");
        header.style.fontSize = 16;
        ParentContainer.Add(header);

        // Create property fields
        //sync up with width and also track the value
        var widthField = new PropertyField(WidthSerializedProperty, "Width");
        widthField.TrackPropertyValue(WidthSerializedProperty, OnGridSizeValueChange);
        var heightField = new PropertyField(HeightSerializedProperty, "Height");
        heightField.TrackPropertyValue(HeightSerializedProperty, OnGridSizeValueChange);
        
        //add these fields
        ParentContainer.Add(widthField);
        ParentContainer.Add(heightField);
        //add grid label and add grid
        header = new Label("Grid");
        header.style.fontSize = 16;
        ParentContainer.Add(header);
        var button = new Button(FloodFill)
        {
            text = "Flood"
        };
        button.style.marginTop = 8;
        ParentContainer.Add(button);
        button = new Button(Clear)
        {
            text = "Clear"
        };
        button.style.marginTop = 8;
        ParentContainer.Add(button);
        RefreshGrid();

        


        return ParentContainer;
    }
}