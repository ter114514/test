using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存檔資料容器。
/// 核心原則：只定義「存什麼」，不含任何邏輯。
/// 新增要存的內容時，在這裡加欄位，並在對應系統實作 ISaveable 讀寫它。
/// </summary>
[Serializable]
public class SaveData
{
    [Header("存檔資訊（顯示於存檔選擇畫面）")]
    public string saveVersion = "1.0";
    public string saveTime;                 // 存檔時間，如 "2026-07-23 14:30"
    public float playTimeSeconds;           // 累計遊玩時間

    [Header("場景")]
    public string currentSceneName;
    public int currentCheckpoint;           // 存檔點編號

    [Header("玩家狀態")]
    public float playerCurrentHealth;
    public float playerMaxHealth;
    public Vector3 playerPosition;

    [Header("玩家屬性")]
    public float attackPower;
    public float defense;
    public float moveSpeed;

    [Header("進度")]
    public List<string> defeatedEnemies = new();    // 已擊敗的敵人/Boss ID
    public List<string> unlockedItems = new();      // 已取得道具
    public List<string> unlockedAbilities = new();  // 已解鎖能力
}