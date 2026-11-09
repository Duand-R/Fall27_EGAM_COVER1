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

        // 记录起点，避免鼠标抓住物品中间时发生跳动
        dragging = GetPointerPosition(eventData, out startPointer);
        startPosition = tile.anchoredPosition;

        if (dragging)
        {
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
                placement, eventData.position, eventData.pressEventCamera))
        {
            return;
        }

        if (GetPointerPosition(eventData, out Vector2 pointer))
        {
            Vector2 position = startPosition + pointer - startPointer;
            int x = Mathf.RoundToInt(position.x / cellPitch);
            int y = Mathf.RoundToInt(-position.y / cellPitch);

            // 只有位置合法时才更新背包数据
            inventory.TryMove(placement, x, y);
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
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            layer, eventData.position, eventData.pressEventCamera, out position
        );
    }
}