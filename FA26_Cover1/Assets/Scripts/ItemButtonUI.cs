using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemButtonUI : MonoBehaviour, IPointerClickHandler
{
    private ItemData item;
    private Action<ItemData> onTake;
    private bool taken;

    public void Setup(ItemData itemData, Action<ItemData> takeAction)
    {
        // 保存这个按钮代表的物品和拿取操作
        item = itemData;
        onTake = takeAction;
        taken = false;

        TMP_Text label = GetComponentInChildren<TMP_Text>();

        label.text =
            $"{item.itemName}  ({item.width} x {item.height})";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 只响应鼠标左键双击
        if (eventData.button != PointerEventData.InputButton.Left
            || eventData.clickCount != 2
            || taken)
        {
            return;
        }

        // 防止同一个按钮重复执行拿取
        taken = true;
        onTake?.Invoke(item);
    }
}