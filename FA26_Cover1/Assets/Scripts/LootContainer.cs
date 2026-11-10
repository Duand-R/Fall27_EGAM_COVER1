using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class LootContainer : MonoBehaviour
{
    public Transform lidPivot;

    private bool lidOpened;

    public void OpenLid()
    {
        // 同一个箱子只播放一次开盖动画
        if (lidPivot == null || lidOpened)
        {
            return;
        }

        lidOpened = true;
        StartCoroutine(AnimateLid());
    }

    private IEnumerator AnimateLid()
    {
        const float duration = 0.35f;
        Quaternion closed = lidPivot.localRotation;
        Quaternion opened = closed * Quaternion.Euler(65f, 0f, 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 逐帧转动箱盖，起步和停下时更平滑
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            lidPivot.localRotation = Quaternion.Slerp(closed, opened, t);
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        lidPivot.localRotation = opened;
    }
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