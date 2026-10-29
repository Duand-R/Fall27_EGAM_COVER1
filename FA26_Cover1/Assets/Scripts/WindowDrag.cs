using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDrag : MonoBehaviour,
    IBeginDragHandler, IDragHandler
{
    public RectTransform windowRect;

    private RectTransform parentRect;
    private Vector2 pointerOffset;
    private bool isDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 只允许鼠标左键拖动
        if (eventData.button != PointerEventData.InputButton.Left
            || windowRect == null)
        {
            return;
        }

        parentRect = windowRect.parent as RectTransform;

        if (parentRect == null)
        {
            return;
        }

        // 记录鼠标相对于窗口的位置，避免拖动时突然跳位
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            pointerOffset =
                (Vector2)windowRect.localPosition - pointerPosition;

            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging
            || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // 将鼠标位置转换到窗口父物体的坐标中
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 pointerPosition))
        {
            Vector2 position = pointerPosition + pointerOffset;

            windowRect.localPosition = new Vector3(
                position.x,
                position.y,
                windowRect.localPosition.z
            );
        }
    }
}