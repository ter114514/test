/// <summary>
/// 可存檔的系統。實作此介面的元件會被 SaveManager 自動納入存讀檔流程。
/// </summary>
public interface ISaveable
{
    /// <summary>把自己的狀態寫入 SaveData</summary>
    void SaveState(SaveData data);

    /// <summary>從 SaveData 還原自己的狀態</summary>
    void LoadState(SaveData data);
}