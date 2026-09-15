using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// IngredientDrawerUIE
[CustomPropertyDrawer(typeof(GridSize))]
public class IngredientDrawerUIE : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        var header = new Label("Grid Size");
        
        var separator = new ToolbarSpacer();
        
        // Create property fields.
        var widthField = new PropertyField(property.FindPropertyRelative("Width"));
        var heightField = new PropertyField(property.FindPropertyRelative("Height"));

        // Add fields to the container.

        container.Add(header);
        container.Add(separator);
        container.Add(widthField);
        container.Add(heightField);
        
        return container;
    }
}
