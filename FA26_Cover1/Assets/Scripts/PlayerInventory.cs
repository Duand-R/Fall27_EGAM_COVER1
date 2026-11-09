using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int Columns = 6;
    public const int Rows = 4;

    public class Placement
    {
        public ItemData item;
        public RectInt cells;
    }

    private readonly List<Placement> placements = new List<Placement>();
    public IReadOnlyList<Placement> Placements => placements;

    public bool TryAdd(ItemData item)
    {
        if (item == null || item.width < 1 || item.height < 1)
        {
            return false;
        }

        // 从左上角开始，逐行寻找能放下整件物品的位置
        for (int y = 0; y <= Rows - item.height; y++)
        {
            for (int x = 0; x <= Columns - item.width; x++)
            {
                RectInt area = new RectInt(x, y, item.width, item.height);
                bool blocked = false;

                foreach (Placement placement in placements)
                {
                    if (area.Overlaps(placement.cells))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                {
                    placements.Add(new Placement { item = item, cells = area });
                    return true;
                }
            }
        }

        return false;
    }
    public bool TryMove(Placement placement, int x, int y)
    {
        if (!placements.Contains(placement))
        {
            return false;
        }

        RectInt area = new RectInt(
            x, y, placement.cells.width, placement.cells.height
        );

        // 整件物品都必须在背包范围内
        if (area.xMin < 0 || area.yMin < 0
            || area.xMax > Columns || area.yMax > Rows)
        {
            return false;
        }

        // 检查其他物品，跳过正在移动的这件
        foreach (Placement other in placements)
        {
            if (other != placement && area.Overlaps(other.cells))
            {
                return false;
            }
        }

        placement.cells = area;
        return true;
    }

    public bool TryRemove(Placement placement)
    {
        return placements.Remove(placement);
    }

    public bool TryAddAt(ItemData item, int x, int y)
    {
        if (item == null || item.width < 1 || item.height < 1)
        {
            return false;
        }

        // 检查整个物品能不能放在指定位置
        if (x < 0 || y < 0
            || x > Columns - item.width || y > Rows - item.height)
        {
            return false;
        }

        RectInt area = new RectInt(x, y, item.width, item.height);

        foreach (Placement other in placements)
        {
            if (area.Overlaps(other.cells))
            {
                return false;
            }
        }

        placements.Add(new Placement { item = item, cells = area });
        return true;
    }
}