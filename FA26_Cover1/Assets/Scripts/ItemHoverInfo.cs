using UnityEngine;
using UnityEngine.EventSystems;

public class ItemHoverInfo : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private ItemData item;
    private ItemTooltipUI tooltip;

    public void Setup(ItemData itemData)
    {
        item = itemData;
        tooltip = GetComponentInParent<ItemTooltipUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 拖动经过物品时，不弹出提示
        if (item != null && tooltip != null && !Input.GetMouseButton(0))
        {
            tooltip.Show(this, item, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        if (tooltip != null)
        {
            tooltip.Hide(this);
        }
    }
}