using TMPro;
using UnityEngine;

public class ContainerUI : MonoBehaviour
{
    public LootContainer container;
    public TMP_Text itemsText;

    public Transform itemList;
    public ItemButtonUI itemButtonPrefab;

    public PlayerInventory playerInventory;
    public InventoryUI inventoryUI;

    private void Start()
    {
        RefreshItems();
    }

    public void RefreshItems()
    {
        // 检查显示按钮需要的引用
        if (container == null
            || itemList == null
            || itemButtonPrefab == null)
        {
            Debug.LogWarning(
                "Assign Container, Item List and Item Button Prefab."
            );
            return;
        }

        // 先隐藏并删除旧按钮，避免重复显示
        foreach (Transform child in itemList)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        bool hasItems = false;

        // 为箱子中的每件物品生成一个按钮
        foreach (ItemData item in container.items)
        {
            if (item == null)
            {
                continue;
            }

            ItemButtonUI button =
                Instantiate(itemButtonPrefab, itemList);

            button.Setup(item, TakeItem);
            hasItems = true;
        }

        // 原来的文字区域只用于显示空箱提示
        if (itemsText != null)
        {
            itemsText.text = "Empty";
            itemsText.gameObject.SetActive(!hasItems);
        }
    }

    private void TakeItem(ItemData item)
    {
        // 确认玩家背包和界面已经连接
        if (playerInventory == null || inventoryUI == null)
        {
            Debug.LogWarning(
                "Assign Player Inventory and Inventory UI."
            );
            return;
        }

        // 只有成功从箱子移除，才加入玩家背包
        if (!container.items.Remove(item))
        {
            return;
        }

        playerInventory.items.Add(item);

        // 同时刷新箱子和背包的显示
        RefreshItems();
        inventoryUI.RefreshItems();
    }
}