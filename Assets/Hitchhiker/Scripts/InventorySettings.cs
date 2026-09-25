using UnityEngine;

[CreateAssetMenu(fileName = "InventorySettings", menuName = "Scriptable Objects/InventorySettings")]
public class InventorySettings : ScriptableObject
{
    [SerializeField]
    internal Texture2D CellTexture;

    [SerializeField]
    internal Color BackgroundColor = Color.black;

    [SerializeField]
    internal RotationDirection RotationDirection = RotationDirection.Clockwise;

    [SerializeField, Range(20, 99)]
    internal int FontSize = 20;

}

public enum RotationDirection
{
    None, Clockwise, Counterclockwise
}
