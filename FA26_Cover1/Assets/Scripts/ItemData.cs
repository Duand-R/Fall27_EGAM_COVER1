using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Loot/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Information")]
    public string itemName;

    public Sprite icon;

    [Header("Inventory Size")]
    [Min(1)]
    public int width = 1;

    [Min(1)]
    public int height = 1;
}