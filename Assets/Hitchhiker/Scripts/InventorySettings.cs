using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "InventorySettings", menuName = "Scriptable Objects/InventorySettings")]
public class InventorySettings : ScriptableObject
{
    [SerializeField]
    internal Sprite CellTexture;

    [SerializeField]
    internal Color BackgroundColor = Color.black;


    [SerializeField]
    internal bool Savable;
}
