using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// 單個按鍵重綁 UI。點擊後進入等待狀態，玩家按下新鍵完成重綁。
/// 衝突時採「交換」策略：新鍵已被占用時，將對方的鍵換成本 Action 原本的鍵。
/// </summary>
public class RebindButton : MonoBehaviour
{
    [Header("要重綁的 Action")]
    [Tooltip("Action 名稱，例如 Jump、Attack、Move")]
    [SerializeField] string actionName;
    [Tooltip("要重綁第幾個 binding（單鍵通常 0；Move 的左=1 右=2）")]
    [SerializeField] int bindingIndex = 0;

    [Header("UI")]
    [SerializeField] TMP_Text bindingLabel;   // 顯示當前按鍵
    [SerializeField] Button rebindButton;      // 點擊開始重綁
    [SerializeField] TMP_Text buttonText;      // 按鈕上的文字

    [Header("交換通知")]
    [Tooltip("發生交換時，用來刷新其他按鍵列的顯示（把所有列都拖進來）")]
    [SerializeField] RebindButton[] allRebindButtons;

    InputAction action;
    InputActionRebindingExtensions.RebindingOperation rebindOperation;

    void Start()
    {
        // ---- null 防護，出問題時明確告知 ----
        if (RebindManager.Instance == null)
        {
            Debug.LogError("【RebindButton】RebindManager.Instance 是 null —— 場景中沒有 RebindManager 物件。");
            return;
        }
        if (RebindManager.Instance.InputActions == null)
        {
            Debug.LogError("【RebindButton】InputActions 是 null —— RebindManager 沒拖入 PlayerControls 資產。");
            return;
        }

        action = RebindManager.Instance.InputActions.FindAction(actionName);
        if (action == null)
        {
            Debug.LogError($"【RebindButton】找不到 Action：'{actionName}' —— 名稱拼錯或資產裡沒這個 Action。");
            return;
        }

        if (rebindButton == null)
        {
            Debug.LogError("【RebindButton】rebindButton 沒連 —— Inspector 拖入更改按鈕。");
            return;
        }

        rebindButton.onClick.AddListener(StartRebind);
        UpdateDisplay();
    }

    void StartRebind()
    {
        action.Disable();
        if (buttonText != null) buttonText.text = "請按鍵...";

        // 記錄重綁前的原路徑，交換時要用
        string oldPath = action.bindings[bindingIndex].effectivePath;

        rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(op => RebindComplete(oldPath))
            .OnCancel(op => RebindComplete(oldPath))
            .Start();
    }

    void RebindComplete(string oldPath)
    {
        rebindOperation?.Dispose();
        rebindOperation = null;

        string newPath = action.bindings[bindingIndex].effectivePath;

        // 找出新鍵是否被其他綁定占用 → 交換
        if (FindConflict(newPath, out InputAction conflictAction, out int conflictIndex))
        {
            // 把衝突對象的鍵換成本 Action 原本的鍵
            conflictAction.ApplyBindingOverride(conflictIndex, oldPath);
        }

        action.Enable();
        RebindManager.Instance.SaveRebinds();

        RefreshAll();
    }

    /// <summary>找出某個按鍵路徑被哪個綁定占用（排除自己與 composite 標頭）</summary>
    bool FindConflict(string path, out InputAction conflictAction, out int conflictIndex)
    {
        conflictAction = null;
        conflictIndex = -1;

        var asset = RebindManager.Instance.InputActions;
        foreach (var map in asset.actionMaps)
        {
            foreach (var act in map.actions)
            {
                for (int i = 0; i < act.bindings.Count; i++)
                {
                    var binding = act.bindings[i];

                    if (binding.isComposite) continue;                 // 跳過 composite 標頭
                    if (act == action && i == bindingIndex) continue;  // 跳過自己

                    if (binding.effectivePath == path)
                    {
                        conflictAction = act;
                        conflictIndex = i;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void RefreshAll()
    {
        UpdateDisplay();
        if (allRebindButtons != null)
            foreach (var rb in allRebindButtons)
                if (rb != null && rb != this) rb.RefreshDisplay();
    }

    void UpdateDisplay()
    {
        if (action == null) return;
        if (bindingLabel != null)
            bindingLabel.text = action.GetBindingDisplayString(bindingIndex);
        if (buttonText != null)
            buttonText.text = "更改";
    }

    /// <summary>供外部（重置按鈕、交換）刷新顯示</summary>
    public void RefreshDisplay() => UpdateDisplay();

    void OnDestroy() => rebindOperation?.Dispose();
}