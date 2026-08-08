using System.Collections.Generic;

/// <summary>
/// 追蹤本次遊玩中已擊敗的敵人 ID。
/// 因為敵人死亡後物件會被銷毀，無法在存檔時自己回報，
/// 故由此靜態類別暫存，存檔時統一寫入。
/// </summary>
public static class DefeatedEnemyTracker
{
    static readonly HashSet<string> defeated = new();

    public static void MarkDefeated(string id)
    {
        if (!string.IsNullOrEmpty(id)) defeated.Add(id);
    }

    public static bool IsDefeated(string id) => defeated.Contains(id);

    public static IEnumerable<string> All => defeated;

    public static void Clear() => defeated.Clear();

    /// <summary>從存檔資料還原</summary>
    public static void RestoreFrom(SaveData data)
    {
        defeated.Clear();
        foreach (var id in data.defeatedEnemies) defeated.Add(id);
    }

    /// <summary>寫入存檔資料</summary>
    public static void WriteTo(SaveData data)
    {
        data.defeatedEnemies.Clear();
        data.defeatedEnemies.AddRange(defeated);
    }
}