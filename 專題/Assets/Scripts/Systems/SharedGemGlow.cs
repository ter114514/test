using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 一組按鈕共用的寶石發光控制。
/// 任何一個註冊的按鈕被懸停，寶石就平滑變亮；全部移開才平滑變暗。
/// 用單一控制器 + hoverCount 計數，避免多個腳本搶控制同一顆寶石造成閃爍。
/// 掛在一個管理用的空物件上，把按鈕和寶石都指定進來。
/// </summary>
public class SharedGemGlow : MonoBehaviour
{
    [Header("觸發來源")]
    [Tooltip("會觸發寶石發光的所有按鈕")]
    [SerializeField] List<RectTransform> buttons = new();

    [Header("發光目標")]
    [Tooltip("要一起發光的寶石 Animator（可多顆）")]
    [SerializeField] Animator[] gems;

    [Header("設定")]
    [Tooltip("變亮/變暗速度，越大越快")]
    [SerializeField] float speed = 3f;
    [Tooltip("寶石動畫的狀態名稱，需與 Animator 裡的狀態一致")]
    [SerializeField] string stateName = "Glow";

    int hoverCount = 0;   // 目前有幾個按鈕正被懸停
    float blend;          // 0 = 暗，1 = 最亮

    void Start()
    {
        // 為每個按鈕自動掛上懸停偵測（進入/離開）
        foreach (var btn in buttons)
        {
            if (btn == null) continue;

            var trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            AddEvent(trigger, EventTriggerType.PointerEnter, OnAnyEnter);
            AddEvent(trigger, EventTriggerType.PointerExit, OnAnyExit);
        }
    }

    void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    void OnAnyEnter() => hoverCount++;
    void OnAnyExit() => hoverCount = Mathf.Max(0, hoverCount - 1);

    void Update()
    {
        // 只要有任何按鈕被懸停就往亮，否則往暗
        float target = hoverCount > 0 ? 1f : 0f;
        blend = Mathf.MoveTowards(blend, target, speed * Time.unscaledDeltaTime);

        foreach (var g in gems)
        {
            if (g == null) continue;
            // 把動畫凍結在 blend 對應的時間點（0=暗，1=亮）
            g.Play(stateName, 0, blend);
            g.speed = 0;
        }
    }
}