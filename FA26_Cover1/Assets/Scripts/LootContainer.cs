using System.Collections.Generic;
using UnityEngine;

public class LootContainer : MonoBehaviour
{
    // 保存这个箱子里包含的物品配置
    public List<ItemData> items = new List<ItemData>();

    // 每件物品有自己的位置和搜索状态
    public class LootSlot
    {
        public ItemData item;
        public RectInt area;
        public bool revealed;
    }

    // 第一次打开时排好位置，之后不因拿取或关闭而重新排列
    [System.NonSerialized]
    public List<LootSlot> slots;
}