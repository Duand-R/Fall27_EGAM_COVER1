using TMPro;
using UnityEngine;

public class ItemTooltipUI : MonoBehaviour
{
    public RectTransform tooltipPanel;
    public TMP_Text detailsText;

    private ItemHoverInfo currentOwner;

    private void Awake()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    public void Show(ItemHoverInfo owner, ItemData item, Vector2 screenPosition)
    {
        if (tooltipPanel == null || detailsText == null)
        {
            return;
        }

        // 把屏幕上的鼠标位置转成 Canvas 内的位置
        RectTransform canvasRect = (RectTransform)tooltipPanel.parent;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPosition, null, out Vector2 pointer))
        {
            return;
        }

        currentOwner = owner;
        detailsText.text =
            $"{item.itemName}\nValue: {item.value:N0}\nSize: {item.width} x {item.height}";

        // 把提示框放在鼠标旁边，不让它超出 Canvas
        tooltipPanel.anchorMin = tooltipPanel.anchorMax = canvasRect.pivot;
        tooltipPanel.pivot = new Vector2(0f, 1f);

        Rect bounds = canvasRect.rect;
        tooltipPanel.anchoredPosition = new Vector2(
            Mathf.Clamp(pointer.x + 18f, bounds.xMin,
                bounds.xMax - tooltipPanel.rect.width),
            Mathf.Clamp(pointer.y - 14f, bounds.yMin + tooltipPanel.rect.height,
                bounds.yMax)
        );

        tooltipPanel.SetAsLastSibling();
        tooltipPanel.gameObject.SetActive(true);
    }

    public void Hide(ItemHoverInfo owner)
    {
        // 只让当前显示的物品关闭自己的提示
        if (currentOwner != owner)
        {
            return;
        }

        currentOwner = null;
        tooltipPanel.gameObject.SetActive(false);
    }
}