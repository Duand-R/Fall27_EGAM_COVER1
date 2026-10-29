using System.Text;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public TMP_Text itemsText;

    private void OnEnable()
    {
        RefreshItems();
    }

    public void RefreshItems()
    {
        // 检查玩家背包和文字是否已经连接
        if (playerInventory == null || itemsText == null)
        {
            Debug.LogWarning("Assign Player Inventory and Items Text.");
            return;
        }

        StringBuilder content = new StringBuilder();

        // 显示玩家携带的物品名称和占格尺寸
        foreach (ItemData item in playerInventory.items)
        {
            if (item == null)
            {
                continue;
            }

            content.AppendLine(
                $"{item.itemName}  ({item.width} x {item.height})"
            );
        }

        itemsText.text = content.Length > 0
            ? content.ToString()
            : "Empty";
    }
}