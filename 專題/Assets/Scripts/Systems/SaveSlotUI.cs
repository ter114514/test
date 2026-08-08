using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 單一存檔欄位的 UI 顯示與互動。
/// </summary>
public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] TMP_Text slotNumberText;
    [SerializeField] TMP_Text infoText;
    [SerializeField] Button selectButton;
    [SerializeField] Button deleteButton;

    int slotIndex;
    Action<int> onSelect;
    Action<int> onDelete;

    public void Setup(int index, SaveData data, bool isNewGameMode,
                      Action<int> selectCallback, Action<int> deleteCallback)
    {
        slotIndex = index;
        onSelect = selectCallback;
        onDelete = deleteCallback;

        slotNumberText.text = $"存檔 {index + 1}";

        bool hasData = data != null;

        if (hasData)
        {
            infoText.text = $"HP: {data.playerCurrentHealth:F0}/{data.playerMaxHealth:F0}\n" +
                            $"{data.saveTime}";
        }
        else
        {
            infoText.text = "— 空欄位 —";
        }

        // 新遊戲模式：所有欄位都可選（有資料的會覆蓋）
        // 繼續模式：只有有資料的欄位可選
        selectButton.interactable = isNewGameMode || hasData;
        deleteButton.gameObject.SetActive(hasData);

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke(slotIndex));

        deleteButton.onClick.RemoveAllListeners();
        deleteButton.onClick.AddListener(() => onDelete?.Invoke(slotIndex));
    }
}