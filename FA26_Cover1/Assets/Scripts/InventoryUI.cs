using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public RectTransform itemLayer;
    public TMP_Text totalValueText;
    public ContainerUI containerUI;

    private const float CellSize = 60f;
    private const float Spacing = 4f;

    private void OnEnable()
    {
        RefreshItems();
    }

    public void RefreshItems()
    {
        if (playerInventory == null || itemLayer == null)
        {
            Debug.LogWarning("请给 InventoryUI 指定 Player Inventory 和 Item Layer。");
            return;
        }

        AlignItemLayer(itemLayer);

        // 清除旧的物品显示，保留 SlotGrid 中的空格背景
        foreach (Transform child in itemLayer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        RefreshTotalValue();

        foreach (PlayerInventory.Placement placement in playerInventory.Placements)
        {
            RectInt cells = placement.cells;
            GameObject tile = new GameObject(placement.item.itemName, typeof(Image));
            tile.transform.SetParent(itemLayer, false);

            Image image = tile.GetComponent<Image>();
            // 背包使用与箱子相同的稀有度配色
            switch (placement.item.rarity)
            {
                case "rare":
                    image.color = new Color(0.55f, 0.35f, 0.10f);
                    break;

                case "uncommon":
                    image.color = new Color(0.18f, 0.36f, 0.60f);
                    break;

                default:
                    image.color = new Color(0.28f, 0.34f, 0.40f);
                    break;
            }
            image.raycastTarget = true;
            tile.AddComponent<ItemHoverInfo>().Setup(placement.item);

            BackpackItemDrag drag = tile.AddComponent<BackpackItemDrag>();
            drag.Setup(playerInventory, placement, itemLayer, CellSize + Spacing);
            drag.containerUI = containerUI;

            // 从物品层左上角计算位置；多格物品覆盖内部间距
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(
                cells.x * (CellSize + Spacing),
                -cells.y * (CellSize + Spacing)
            );
            rect.sizeDelta = new Vector2(
                cells.width * CellSize + (cells.width - 1) * Spacing,
                cells.height * CellSize + (cells.height - 1) * Spacing
            );

            GameObject label = new GameObject("Label", typeof(TextMeshProUGUI));
            label.transform.SetParent(tile.transform, false);

            TextMeshProUGUI text = label.GetComponent<TextMeshProUGUI>();
            text.text = placement.item.itemName;
            text.fontSize = 13;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }
    }

    internal static void AlignItemLayer(RectTransform layer)
    {
        // 物品和拖放都以空格背景的左上角为准
        RectTransform grid = layer.parent.Find("SlotGrid") as RectTransform;
        if (grid == null)
        {
            return;
        }

        layer.anchorMin = grid.anchorMin;
        layer.anchorMax = grid.anchorMax;
        layer.pivot = grid.pivot;
        layer.sizeDelta = grid.sizeDelta;
        layer.anchoredPosition3D = grid.anchoredPosition3D;
        layer.localRotation = grid.localRotation;
        layer.localScale = grid.localScale;
    }

    private void RefreshTotalValue()
    {
        if (totalValueText == null)
        {
            return;
        }

        // 每条放置记录是一件物品，多格物品也只算一次
        long total = 0;

        foreach (PlayerInventory.Placement placement in playerInventory.Placements)
        {
            total += placement.item.value;
        }

        totalValueText.text = $"Total Value: {total:N0}";
    }
}