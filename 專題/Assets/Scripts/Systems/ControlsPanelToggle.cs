using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 設定面板中「操作設定」子面板的開關管理。
/// </summary>
public class ControlsPanelToggle : MonoBehaviour
{
    [SerializeField] Button openButton;      // 設定裡的「操作設定」按鈕
    [SerializeField] Button backButton;      // 按鍵面板的「返回」按鈕
    [SerializeField] Button resetButton;     // 「重置為預設」按鈕
    [SerializeField] GameObject rebindPanel; // 按鍵設定面板
    [SerializeField] RebindButton[] allRebindButtons; // 所有按鍵列，重置後要刷新

    void Start()
    {
        rebindPanel.SetActive(false);

        openButton.onClick.AddListener(Open);
        backButton.onClick.AddListener(Close);
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetAll);
    }

    void Open() => rebindPanel.SetActive(true);
    void Close() => rebindPanel.SetActive(false);

    void ResetAll()
    {
        RebindManager.Instance.ResetAllRebinds();
        // 重置後刷新所有按鍵顯示
        foreach (var rb in allRebindButtons)
            if (rb != null) rb.RefreshDisplay();
    }
}