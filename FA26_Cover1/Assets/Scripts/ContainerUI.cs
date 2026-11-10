using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContainerUI : MonoBehaviour
{
    public RectTransform containerItemLayer;
    public ItemButtonUI itemButtonPrefab;

    public PlayerInventory playerInventory;
    public InventoryUI inventoryUI;

    public Sprite searchIcon;

    [Header("Search Duration")]
    [Min(0.1f)] public float commonSearchTime = 1.1f;
    [Min(0.1f)] public float uncommonSearchTime = 1.6f;
    [Min(0.1f)] public float rareSearchTime = 2.4f;

    private const int Columns = 6;
    private const int Rows = 4;
    private const float CellStep = 64f;
    private const float Gap = 4f;

    private LootContainer container;
    private Coroutine searchRoutine;
    private GameObject searchMarker;

    private readonly Dictionary<LootContainer.LootSlot, ItemButtonUI> tiles
        = new Dictionary<LootContainer.LootSlot, ItemButtonUI>();

    public void BeginSearch(LootContainer target)
    {
        EndSearch();

        if (target == null || containerItemLayer == null
            || itemButtonPrefab == null || playerInventory == null
            || inventoryUI == null || inventoryUI.itemLayer == null)
        {
            Debug.LogWarning("请检查 Container UI 的物品层、预制体和背包引用。");
            return;
        }

        container = target;
        InventoryUI.AlignItemLayer(containerItemLayer);

        // 每个箱子只在第一次打开时排列物品
        if (container.slots == null)
        {
            BuildSlots();
        }

        // 清掉上一轮的显示，保留箱子里的位置和搜索状态
        foreach (Transform child in containerItemLayer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        tiles.Clear();

        foreach (LootContainer.LootSlot slot in container.slots)
        {
            CreateTile(slot);
        }




        if (container.slots.Exists(slot => !slot.revealed))
        {
            searchRoutine = StartCoroutine(RevealItems());
        }
    }

    public void EndSearch()
    {
        if (searchRoutine != null)
        {
            StopCoroutine(searchRoutine);
            searchRoutine = null;
        }

        ClearSearchMarker();
    }

    private void OnDisable()
    {
        EndSearch();
    }

    private void BuildSlots()
    {
        container.slots = new List<LootContainer.LootSlot>();

        foreach (ItemData item in container.items)
        {
            if (item == null || item.width < 1 || item.height < 1)
            {
                continue;
            }

            if (TryFindSpace(item, out RectInt area))
            {
                container.slots.Add(new LootContainer.LootSlot
                {
                    item = item,
                    area = area
                });
            }
            else
            {
                Debug.LogWarning($"箱子格子放不下 {item.itemName}。");
            }
        }
    }

    private bool TryFindSpace(ItemData item, out RectInt area)
    {
        // 从左上角逐行找一块不重叠的空位
        for (int y = 0; y <= Rows - item.height; y++)
        {
            for (int x = 0; x <= Columns - item.width; x++)
            {
                RectInt candidate = new RectInt(x, y, item.width, item.height);

                if (!container.slots.Exists(slot => slot.area.Overlaps(candidate)))
                {
                    area = candidate;
                    return true;
                }
            }
        }

        area = default;
        return false;
    }

    private void CreateTile(LootContainer.LootSlot slot)
    {
        ItemButtonUI tile = Instantiate(itemButtonPrefab, containerItemLayer);

        // 绑定这件物品的位置记录，而不是只靠物品名称查找
        tile.Setup(
            slot.item,
            item => TakeSlot(slot, null),
            (item, x, y) => TakeSlot(slot, new Vector2Int(x, y)),
            inventoryUI.itemLayer,
            (x, y) => MoveSlot(slot, x, y)
        );

        RectInt area = slot.area;
        RectTransform rect = (RectTransform)tile.transform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(area.x * CellStep, -area.y * CellStep);
        rect.sizeDelta = new Vector2(
            area.width * CellStep - Gap,
            area.height * CellStep - Gap
        );

        Image background = tile.GetComponent<Image>();
        background.sprite = null;

        tile.GetComponent<Button>().transition = Selectable.Transition.None;

        TMP_Text label = tile.GetComponentInChildren<TMP_Text>(true);
        label.fontSize = 13f;
        label.raycastTarget = false;

        tile.gameObject.AddComponent<ItemHoverInfo>().Setup(slot.item);
        tiles.Add(slot, tile);
        SetRevealed(tile, slot.item, slot.revealed);
    }

    private void SetRevealed(ItemButtonUI tile, ItemData item, bool revealed)
    {
        Image background = tile.GetComponent<Image>();
        TMP_Text label = tile.GetComponentInChildren<TMP_Text>(true);

        // 默认显示未知物品的深灰色
        background.color = new Color(0.25f, 0.27f, 0.30f);

        if (revealed)
        {
            // 搜完才显示稀有度颜色和价值
            switch (item.rarity)
            {
                case "rare":
                    background.color = new Color(0.55f, 0.35f, 0.10f);
                    break;
                case "uncommon":
                    background.color = new Color(0.18f, 0.36f, 0.60f);
                    break;
                default:
                    background.color = new Color(0.28f, 0.34f, 0.40f);
                    break;
            }

label.text = item.itemName;
            label.color = Color.white;
        }

        // 只有搜完的物品才能双击或拖动
        label.gameObject.SetActive(revealed);
        background.raycastTarget = revealed;
        tile.GetComponent<Button>().interactable = revealed;
        tile.enabled = revealed;
    }

    private IEnumerator RevealItems()
    {
        while (true)
        {
            LootContainer.LootSlot slot =
                container.slots.Find(item => !item.revealed);

            if (slot == null)
            {
                break;
            }

            ItemButtonUI tile = tiles[slot];

            ShowSearchMarker(tile.transform);

            float elapsed = 0f;

            // 稀有度只影响等待时长，不改变放大镜的转速
            float duration = commonSearchTime;
            if (slot.item.rarity == "uncommon")
                duration = uncommonSearchTime;
            else if (slot.item.rarity == "rare")
                duration = rareSearchTime;

            while (elapsed < duration)
            {
                if (searchMarker != null)
                {
                    // 放大镜保持朝向，沿着小圆圈移动
                    float angle = -elapsed * 2f * Mathf.PI;
                    RectTransform marker = (RectTransform)searchMarker.transform;
                    marker.anchoredPosition = new Vector2(
                        Mathf.Cos(angle) * 6f,
                        Mathf.Sin(angle) * 6f
                    );
                }

                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }

            ClearSearchMarker();

            // 不重建物品块，只在原位置揭晓
            slot.revealed = true;
            SetRevealed(tile, slot.item, true);
        }

        searchRoutine = null;
    }

    private void ShowSearchMarker(Transform parent)
    {
        if (searchIcon == null)
        {
            return;
        }

        searchMarker = new GameObject("SearchMarker", typeof(Image));
        searchMarker.transform.SetParent(parent, false);

        Image image = searchMarker.GetComponent<Image>();
        image.sprite = searchIcon;
        image.preserveAspect = true;
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(20f, 20f);
    }

    private void ClearSearchMarker()
    {
        if (searchMarker != null)
        {
            searchMarker.SetActive(false);
            Destroy(searchMarker);
            searchMarker = null;
        }
    }

    private void TakeSlot(LootContainer.LootSlot slot, Vector2Int? target)
    {
        if (!slot.revealed || !tiles.TryGetValue(slot, out ItemButtonUI tile))
        {
            return;
        }

        // 双击自动找位置，拖拽使用玩家选中的格子
        bool placed = target.HasValue
            ? playerInventory.TryAddAt(slot.item, target.Value.x, target.Value.y)
            : playerInventory.TryAdd(slot.item);

        if (!placed)
        {
            return;
        }

        // 背包收下以后，才移除箱子里的物品
        container.slots.Remove(slot);
        container.items.Remove(slot.item);
        tiles.Remove(slot);

        tile.gameObject.SetActive(false);
        Destroy(tile.gameObject);

        inventoryUI.RefreshItems();
    }

    private void MoveSlot(LootContainer.LootSlot slot, int x, int y)
    {
        if (!slot.revealed || !tiles.TryGetValue(slot, out ItemButtonUI tile)
            || !TryGetDropArea(slot.item, x, y, slot, out RectInt area))
        {
            return;
        }

        // 只移动这一件，不重新排列其他物品或打断搜索
        slot.area = area;
        RectTransform rect = (RectTransform)tile.transform;
        rect.anchoredPosition = new Vector2(area.x * CellStep, -area.y * CellStep);
    }

    private bool TryGetDropArea(
        ItemData item, int x, int y, LootContainer.LootSlot ignoredSlot, out RectInt area)
    {
        area = default;
        if (container == null || container.slots == null || item == null
            || item.width < 1 || item.height < 1
            || x < 0 || y < 0 || x > Columns - item.width || y > Rows - item.height)
        {
            return false;
        }

        RectInt candidate = new RectInt(x, y, item.width, item.height);
        if (container.slots.Exists(slot => slot != ignoredSlot && slot.area.Overlaps(candidate)))
        {
            return false;
        }

        area = candidate;
        return true;
    }

    public bool TryReturnItem(
        PlayerInventory.Placement placement,
        Vector2 screenPosition,
        Camera eventCamera,
        Vector2Int grabbedCell)
    {
        // 先验证落点，再从背包移除；失败时保留原位置
        if (placement == null || playerInventory == null
            || !ItemButtonUI.TryGetCell(containerItemLayer, screenPosition, eventCamera,
                out Vector2Int cell)
            || !TryGetDropArea(placement.item, cell.x - grabbedCell.x, cell.y - grabbedCell.y,
                null, out RectInt area)
            || !playerInventory.TryRemove(placement))
        {
            return false;
        }

        ItemData item = placement.item;

        // 玩家已经知道这是什么，所以放回去不用重新搜索
        LootContainer.LootSlot slot = new LootContainer.LootSlot
        {
            item = item,
            area = area,
            revealed = true
        };

        container.items.Add(item);
        container.slots.Add(slot);

        CreateTile(slot);
        inventoryUI.RefreshItems();
        return true;
    }
}
