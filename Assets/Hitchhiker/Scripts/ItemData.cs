using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    
    public bool Stackable
    {
        get
        {
            return MaxCount > 1;
        }
    }

    [SerializeField]
    internal int MaxCount = 1;

    [SerializeField]
    internal GridSize Size;
}
