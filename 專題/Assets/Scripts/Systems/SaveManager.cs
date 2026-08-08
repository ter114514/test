using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 存檔管理器（多欄位版）。
/// 管理 3 個存檔欄位、記住當前使用中的欄位（跨場景保留）。
/// 負責序列化、寫入檔案、讀取還原，以及新遊戲／繼續遊戲的流程控制。
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public const int SlotCount = 3;

    [Header("設定")]
    [Tooltip("是否加密（簡易混淆，非高強度加密）")]
    [SerializeField] bool useEncryption = false;

    [Header("場景名稱")]
    [Tooltip("遊戲主場景名稱")]
    [SerializeField] string gameSceneName = "Game";
    [Tooltip("主畫面場景名稱")]
    [SerializeField] string menuSceneName = "MainMenu";

    /// <summary>當前使用中的存檔欄位（0~2），-1 表示未選</summary>
    public int CurrentSlot { get; private set; } = -1;

    public event Action OnSaved;
    public event Action OnLoaded;

    // 讀檔後待套用的資料（等場景載入完成才套用）
    SaveData pendingLoadData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    string GetPath(int slot)
        => Path.Combine(Application.persistentDataPath, $"save_slot{slot}.json");

    // ---- 欄位查詢（給存檔選擇 UI 用）----

    public bool SlotHasData(int slot) => File.Exists(GetPath(slot));

    /// <summary>讀取欄位摘要資訊（顯示在選擇畫面，不套用到遊戲）</summary>
    public SaveData PeekSlot(int slot)
    {
        if (!SlotHasData(slot)) return null;
        try
        {
            string json = File.ReadAllText(GetPath(slot));
            if (useEncryption) json = XorObfuscate(json);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"讀取欄位 {slot} 失敗：{e.Message}");
            return null;
        }
    }

    // ---- 新遊戲 ----

    public void StartNewGame(int slot)
    {
        CurrentSlot = slot;
        pendingLoadData = null;              // 不套用任何舊資料
        DefeatedEnemyTracker.Clear();        // 新遊戲，所有敵人都要在
        DeleteSlot(slot);                    // 清掉該欄位舊檔
        SceneManager.LoadScene(gameSceneName);
    }

    // ---- 繼續遊戲 ----

    public bool ContinueGame(int slot)
    {
        var data = PeekSlot(slot);
        if (data == null) return false;

        CurrentSlot = slot;
        pendingLoadData = data;              // 存起來，等場景載完再套用
        SceneManager.LoadScene(gameSceneName);
        return true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoadData == null) return;
        if (scene.name != gameSceneName) return;

        // 必須在各 LoadState 之前還原，敵人才知道自己該不該存在
        DefeatedEnemyTracker.RestoreFrom(pendingLoadData);

        // 場景物件都就緒了，此時才套用存檔資料
        foreach (var s in FindSaveables())
            s.LoadState(pendingLoadData);

        OnLoaded?.Invoke();
        pendingLoadData = null;
        Debug.Log($"已載入欄位 {CurrentSlot + 1} 的存檔");
    }

    // ---- 遊戲內存檔（篝火呼叫）----

    public void SaveToCurrentSlot()
    {
        if (CurrentSlot < 0)
        {
            Debug.LogWarning("尚未選擇存檔欄位");
            return;
        }

        var data = new SaveData
        {
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            currentSceneName = SceneManager.GetActiveScene().name
        };

        // 讓所有實作 ISaveable 的系統寫入自己的資料
        foreach (var s in FindSaveables())
            s.SaveState(data);

        // 寫入已擊敗敵人清單（敵人死後物件已銷毀，需由 Tracker 統一寫入）
        DefeatedEnemyTracker.WriteTo(data);

        try
        {
            string json = JsonUtility.ToJson(data, true);
            if (useEncryption) json = XorObfuscate(json);
            File.WriteAllText(GetPath(CurrentSlot), json);
            OnSaved?.Invoke();
            Debug.Log($"已存檔至欄位 {CurrentSlot + 1}");
        }
        catch (Exception e)
        {
            Debug.LogError($"存檔失敗：{e.Message}");
        }
    }

    // ---- 其他 ----

    public void DeleteSlot(int slot)
    {
        if (SlotHasData(slot))
        {
            File.Delete(GetPath(slot));
            Debug.Log($"欄位 {slot + 1} 存檔已刪除");
        }
    }

    public void ReturnToMenu()
    {
        CurrentSlot = -1;
        pendingLoadData = null;
        DefeatedEnemyTracker.Clear();
        Time.timeScale = 1f;                 // 避免從暫停狀態退出後時間仍停止
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    ISaveable[] FindSaveables()
    {
        var monos = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        var list = new List<ISaveable>();
        foreach (var m in monos)
            if (m is ISaveable s) list.Add(s);
        return list.ToArray();
    }

    /// <summary>簡易 XOR 混淆，防止玩家直接用記事本改存檔。非高強度加密。</summary>
    string XorObfuscate(string input)
    {
        const string key = "MyGameKey2026";
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
            sb.Append((char)(input[i] ^ key[i % key.Length]));
        return sb.ToString();
    }
}