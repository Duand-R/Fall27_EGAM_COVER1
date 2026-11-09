using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LootContainer))]
public class LootGenerator : MonoBehaviour
{
    public TextAsset lootTableJson;

    [System.Serializable]
    private class LootTable
    {
        public int minItems;
        public int maxItems;
        public RarityWeight[] rarityWeights;
        public LootRow[] items;
    }

    [System.Serializable]
    private class RarityWeight
    {
        public string rarity;
        public int weight;
    }

    [System.Serializable]
    private class LootRow
    {
        public string id;
        public string name;
        public int value;
        public int width;
        public int height;
        public string rarity;
    }

    private void Awake()
    {
        if (lootTableJson == null)
        {
            Debug.LogWarning("请把 LootTable.json 拖进 Loot Table Json。", this);
            return;
        }

        LootTable table = JsonUtility.FromJson<LootTable>(lootTableJson.text);

        if (table == null || table.items == null || table.rarityWeights == null)
        {
            Debug.LogWarning("掉落表缺少 items 或 rarityWeights。", this);
            return;
        }

        LootContainer container = GetComponent<LootContainer>();
        List<LootRow> available = new List<LootRow>(table.items);

        int min = Mathf.Clamp(table.minItems, 0, available.Count);
        int max = Mathf.Clamp(table.maxItems, min, available.Count);
        int count = Random.Range(min, max + 1);

        container.items.Clear();

        for (int i = 0; i < count; i++)
        {
            LootRow row = RollItem(table.rarityWeights, available);
            if (row == null)
            {
                break;
            }

            // 把 JSON 的一行变成现有箱子和背包都认识的 ItemData
            ItemData item = ScriptableObject.CreateInstance<ItemData>();
            item.id = row.id;
            item.itemName = row.name;
            item.value = row.value;
            item.width = row.width;
            item.height = row.height;
            item.rarity = row.rarity;

            container.items.Add(item);
            available.Remove(row);
        }
    }

    private LootRow RollItem(RarityWeight[] weights, List<LootRow> available)
    {
        int totalWeight = 0;

        // 没有剩余物品的稀有度不参加抽取
        foreach (RarityWeight tier in weights)
        {
            if (tier.weight > 0 && available.Exists(item => item.rarity == tier.rarity))
            {
                totalWeight += tier.weight;
            }
        }

        if (totalWeight == 0)
        {
            return null;
        }

        int roll = Random.Range(0, totalWeight);

        foreach (RarityWeight tier in weights)
        {
            if (tier.weight <= 0)
            {
                continue;
            }

            List<LootRow> candidates =
                available.FindAll(item => item.rarity == tier.rarity);

            if (candidates.Count == 0)
            {
                continue;
            }

            // 先抽中稀有度，再在这一档里随机选一件
            if (roll < tier.weight)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }

            roll -= tier.weight;
        }

        return null;
    }
}