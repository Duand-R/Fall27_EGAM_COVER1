using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float CellSize = 60f;
    private const float CellPitch = 64f;

    private ItemData item;
    private Action<ItemData> onTake;
    private Action<ItemData, int, int> onDrop;
    private RectTransform itemLayer;
    private RectTransform ghost;

    public void Setup(
        ItemData itemData,
        Action<ItemData> takeAction,
        Action<ItemData, int, int> dropAction,
        RectTransform backpackLayer)
    {
        item = itemData;
        onTake = takeAction;
        onDrop = dropAction;
        itemLayer = backpackLayer;

        TMP_Text label = GetComponentInChildren<TMP_Text>();
        label.text = $"{item.itemName}  ({item.width} x {item.height})";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 双击仍然使用原来的自动放置
        if (eventData.button == PointerEventData.InputButton.Left
            && eventData.clickCount == 2)
        {
            onTake?.Invoke(item);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left
            || itemLayer == null || item == null)
        {
            return;
        }

        // 创建临时预览，不移动右侧按钮，也不修改物品数据
        GameObject preview = new GameObject("DragPreview", typeof(Image));
        preview.transform.SetParent(itemLayer, false);

        Image image = preview.GetComponent<Image>();
        image.color = new Color(0.22f, 0.55f, 0.72f, 0.65f);
        image.raycastTarget = false;

        ghost = image.rectTransform;
        ghost.anchorMin = ghost.anchorMax = ghost.pivot = new Vector2(0f, 1f);
        ghost.sizeDelta = new Vector2(
            item.width * CellPitch - (CellPitch - CellSize),
            item.height * CellPitch - (CellPitch - CellSize)
        );

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null && GetGridPosition(eventData, out Vector2 position))
        {
            ghost.anchoredPosition = new Vector2(position.x, -position.y);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghost == null)
        {
            return;
        }

        ClearPreview();

        if (!GetGridPosition(eventData, out Vector2 position))
        {
            return;
        }

        // 鼠标必须在背包格子区域内
        if (position.x < 0f || position.y < 0f
            || position.x >= itemLayer.rect.width
            || position.y >= itemLayer.rect.height)
        {
            return;
        }

        int x = Mathf.FloorToInt(position.x / CellPitch);
        int y = Mathf.FloorToInt(position.y / CellPitch);
        onDrop?.Invoke(item, x, y);
    }

    private bool GetGridPosition(PointerEventData eventData, out Vector2 position)
    {
        position = Vector2.zero;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemLayer, eventData.position, eventData.pressEventCamera,
            out Vector2 local))
        {
            return false;
        }

        // 转为从背包左上角向右、向下计算的坐标
        position = new Vector2(
            local.x - itemLayer.rect.xMin,
            itemLayer.rect.yMax - local.y
        );

        return true;
    }

    private void OnDisable()
    {
        ClearPreview();
    }

    private void ClearPreview()
    {
        if (ghost != null)
        {
            Destroy(ghost.gameObject);
            ghost = null;
        }
    }
}