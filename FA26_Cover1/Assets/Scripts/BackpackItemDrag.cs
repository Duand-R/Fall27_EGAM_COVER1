using UnityEngine;
using UnityEngine.EventSystems;

public class BackpackItemDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private PlayerInventory inventory;
    private PlayerInventory.Placement placement;
    private RectTransform tile;
    private RectTransform layer;
    private float cellPitch;

    private bool dragging;
    private Vector2 startPosition;
    private Vector2 startPointer;
    private Vector2Int grabbedCell;

    public ContainerUI containerUI;

    public void Setup(
        PlayerInventory owner,
        PlayerInventory.Placement itemPlacement,
        RectTransform itemLayer,
        float pitch)
    {
        inventory = owner;
        placement = itemPlacement;
        tile = GetComponent<RectTransform>();
        layer = itemLayer;
        cellPitch = pitch;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // 用按下时的位置，记住鼠标抓住了物品的哪一格
        dragging = GetPointerPosition(
            eventData.pressPosition, eventData.pressEventCamera, out startPointer);
        startPosition = tile.anchoredPosition;

        if (dragging)
        {
            Vector2 offset = ToGridPosition(startPointer) -
                new Vector2(startPosition.x, -startPosition.y);

            grabbedCell = new Vector2Int(
                Mathf.Clamp(Mathf.FloorToInt(offset.x / cellPitch), 0, placement.cells.width - 1),
                Mathf.Clamp(Mathf.FloorToInt(offset.y / cellPitch), 0, placement.cells.height - 1)
            );
            tile.SetAsLastSibling();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragging && GetPointerPosition(eventData, out Vector2 pointer))
        {
            // 拖动时只移动画面，暂时不改变背包数据
            tile.anchoredPosition = startPosition + pointer - startPointer;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
        {
            return;
        }

        dragging = false;

        // 先检查是不是拖回了右侧箱子
        if (containerUI != null
            && containerUI.TryReturnItem(
                placement, eventData.position, eventData.pressEventCamera, grabbedCell))
        {
            return;
        }

        if (GetPointerPosition(eventData, out Vector2 pointer))
        {
            // 让抓住的那一格对准松手时鼠标所在的格子
            Vector2 position = ToGridPosition(pointer);
            if (position.x >= 0f && position.y >= 0f
                && position.x < layer.rect.width && position.y < layer.rect.height)
            {
                int x = Mathf.FloorToInt(position.x / cellPitch) - grabbedCell.x;
                int y = Mathf.FloorToInt(position.y / cellPitch) - grabbedCell.y;
                inventory.TryMove(placement, x, y);
            }
        }

        // 按数据中的位置对齐；放置失败时，数据仍是原位置
        tile.anchoredPosition = new Vector2(
            placement.cells.x * cellPitch,
            -placement.cells.y * cellPitch
        );
    }

    private bool GetPointerPosition(
        PointerEventData eventData, out Vector2 position)
    {
        return GetPointerPosition(eventData.position, eventData.pressEventCamera, out position);
    }

    private bool GetPointerPosition(Vector2 screenPosition, Camera eventCamera, out Vector2 position)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            layer, screenPosition, eventCamera, out position);
    }

    private Vector2 ToGridPosition(Vector2 point)
    {
        return new Vector2(point.x - layer.rect.xMin, layer.rect.yMax - point.y);
    }
}