using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float CellPitch = 64f;

    private ItemData item;
    private Action<ItemData> onTake;
    private Action<ItemData, int, int> onDrop;
    private RectTransform itemLayer;
    private RectTransform containerLayer;
    private RectTransform previewLayer;
    private Action<int, int> onMoveInContainer;
    private RectTransform ghost;
    private CanvasGroup sourceVisual;
    private float originalAlpha;
    private Vector2 grabOffset;
    private Vector2Int grabbedCell;

    public void Setup(
        ItemData itemData,
        Action<ItemData> takeAction,
        Action<ItemData, int, int> dropAction,
        RectTransform backpackLayer,
        Action<int, int> moveInContainer)
    {
        item = itemData;
        onTake = takeAction;
        onDrop = dropAction;
        itemLayer = backpackLayer;
        containerLayer = (RectTransform)transform.parent;
        onMoveInContainer = moveInContainer;

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

        RectTransform source = (RectTransform)transform;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || !GetGridPosition(source, eventData.pressPosition,
            eventData.pressEventCamera, out grabOffset))
        {
            return;
        }

        grabbedCell = new Vector2Int(
            Mathf.Clamp(Mathf.FloorToInt(grabOffset.x / CellPitch), 0, item.width - 1),
            Mathf.Clamp(Mathf.FloorToInt(grabOffset.y / CellPitch), 0, item.height - 1)
        );

        // 用和原物品一样的外观拖动，原格暂时隐藏
        previewLayer = (RectTransform)canvas.rootCanvas.transform;
        GameObject preview = new GameObject("DragPreview", typeof(Image));
        preview.transform.SetParent(previewLayer, false);

        Image image = preview.GetComponent<Image>();
        Image original = GetComponent<Image>();
        image.sprite = original.sprite;
        image.color = original.color;
        image.raycastTarget = false;

        ghost = image.rectTransform;
        ghost.anchorMin = ghost.anchorMax = ghost.pivot = new Vector2(0f, 1f);
        ghost.sizeDelta = source.rect.size;

        TMP_Text label = GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            TMP_Text previewLabel = Instantiate(label, ghost, false);
            previewLabel.raycastTarget = false;
        }

        sourceVisual = GetComponent<CanvasGroup>();
        if (sourceVisual == null)
        {
            sourceVisual = gameObject.AddComponent<CanvasGroup>();
        }

        originalAlpha = sourceVisual.alpha;
        sourceVisual.alpha = 0f;

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghost != null && GetGridPosition(previewLayer, eventData.position,
            eventData.pressEventCamera, out Vector2 position))
        {
            Vector2 topLeft = position - grabOffset;
            ghost.anchoredPosition = new Vector2(topLeft.x, -topLeft.y);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghost == null)
        {
            return;
        }

        ClearPreview();

        // 优先检查右侧箱子，再检查左侧背包
        if (TryGetCell(containerLayer, eventData.position, eventData.pressEventCamera,
            out Vector2Int containerCell))
        {
            onMoveInContainer?.Invoke(containerCell.x - grabbedCell.x, containerCell.y - grabbedCell.y);
        }
        else if (TryGetCell(itemLayer, eventData.position, eventData.pressEventCamera,
            out Vector2Int backpackCell))
        {
            onDrop?.Invoke(item, backpackCell.x - grabbedCell.x, backpackCell.y - grabbedCell.y);
        }
    }

    internal static bool TryGetCell(
        RectTransform layer, Vector2 screenPosition, Camera eventCamera, out Vector2Int cell)
    {
        cell = default;

        if (layer == null || !layer.gameObject.activeInHierarchy
            || !GetGridPosition(layer, screenPosition, eventCamera, out Vector2 position)
            || position.x < 0f || position.y < 0f
            || position.x >= layer.rect.width || position.y >= layer.rect.height)
        {
            return false;
        }

        cell = new Vector2Int(
            Mathf.FloorToInt(position.x / CellPitch),
            Mathf.FloorToInt(position.y / CellPitch)
        );
        return true;
    }

    private static bool GetGridPosition(
        RectTransform layer, Vector2 screenPosition, Camera eventCamera, out Vector2 position)
    {
        position = Vector2.zero;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            layer, screenPosition, eventCamera, out Vector2 local))
        {
            return false;
        }

        // 转成以左上角为原点的坐标
        position = new Vector2(local.x - layer.rect.xMin, layer.rect.yMax - local.y);
        return true;
    }

    private void OnDisable()
    {
        ClearPreview();
    }

    private void ClearPreview()
    {
        if (sourceVisual != null)
        {
            sourceVisual.alpha = originalAlpha;
            sourceVisual = null;
        }

        if (ghost != null)
        {
            ghost.gameObject.SetActive(false);
            Destroy(ghost.gameObject);
            ghost = null;
        }
    }
}