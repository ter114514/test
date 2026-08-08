using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 按鍵重綁的存讀管理。
/// 把 Input Actions 的覆寫綁定序列化成 JSON 存進 PlayerPrefs，
/// 遊戲啟動時載入套用。
/// </summary>
public class RebindManager : MonoBehaviour
{
    public static RebindManager Instance { get; private set; }

    const string KEY_REBINDS = "input_rebinds";

    [Tooltip("要管理重綁的 Input Actions 資產")]
    [SerializeField] InputActionAsset inputActions;

    public InputActionAsset InputActions => inputActions;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRebinds();
    }

    /// <summary>把所有覆寫綁定存成 JSON</summary>
    public void SaveRebinds()
    {
        string json = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(KEY_REBINDS, json);
        PlayerPrefs.Save();
    }

    /// <summary>載入並套用已儲存的覆寫綁定</summary>
    public void LoadRebinds()
    {
        string json = PlayerPrefs.GetString(KEY_REBINDS, string.Empty);
        if (!string.IsNullOrEmpty(json))
            inputActions.LoadBindingOverridesFromJson(json);
    }

    /// <summary>重置所有綁定為預設值</summary>
    public void ResetAllRebinds()
    {
        inputActions.RemoveAllBindingOverrides();
        PlayerPrefs.DeleteKey(KEY_REBINDS);
        PlayerPrefs.Save();
    }
}