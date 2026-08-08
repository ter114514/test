using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主畫面控制。管理主選單與存檔選擇面板的切換。
/// 沒有存檔時，「繼續遊戲」整組（按鈕＋搭載圖片）透過 CanvasGroup 變淡且不可點擊。
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] GameObject mainPanel;
    [SerializeField] GameObject slotPanel;

    [Header("主選單按鈕")]
    [SerializeField] Button newGameButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button quitButton;

    [Header("繼續遊戲整組（按鈕＋搭載圖片）")]
    [Tooltip("包住繼續遊戲按鈕與其搭載圖片的父物件上的 CanvasGroup")]
    [SerializeField] CanvasGroup continueGroup;
    [Tooltip("無存檔時的淡化透明度")]
    [SerializeField] float continueDisabledAlpha = 0.4f;

    [Header("存檔欄位 UI")]
    [SerializeField] SaveSlotUI[] slots = new SaveSlotUI[3];
    [SerializeField] Button backButton;

    // 目前是「新遊戲」模式還是「繼續」模式
    bool isNewGameMode;

    void Start()
    {
        newGameButton.onClick.AddListener(() => OpenSlotPanel(true));
        continueButton.onClick.AddListener(() => OpenSlotPanel(false));
        quitButton.onClick.AddListener(() => SaveManager.Instance.QuitGame());
        backButton.onClick.AddListener(CloseSlotPanel);

        RefreshContinueAvailability();

        ShowMain();
    }

    /// <summary>依是否有存檔，更新「繼續遊戲」整組的可用性與淡化。</summary>
    void RefreshContinueAvailability()
    {
        bool hasSave = AnySaveExists();

        continueButton.interactable = hasSave;

        if (continueGroup != null)
        {
            // 整組（按鈕＋搭載圖片）一起變淡
            continueGroup.alpha = hasSave ? 1f : continueDisabledAlpha;
            // 不可用時整組不接收點擊
            continueGroup.interactable = hasSave;
            continueGroup.blocksRaycasts = hasSave;
        }
    }

    bool AnySaveExists()
    {
        for (int i = 0; i < SaveManager.SlotCount; i++)
            if (SaveManager.Instance.SlotHasData(i)) return true;
        return false;
    }

    void ShowMain()
    {
        mainPanel.SetActive(true);
        slotPanel.SetActive(false);
    }

    void OpenSlotPanel(bool newGame)
    {
        isNewGameMode = newGame;
        mainPanel.SetActive(false);
        slotPanel.SetActive(true);
        RefreshSlots();
    }

    void CloseSlotPanel() => ShowMain();

    void RefreshSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;   // 閉包捕獲，必須用區域變數
            var data = SaveManager.Instance.PeekSlot(i);
            slots[i].Setup(index, data, isNewGameMode, OnSlotSelected, OnSlotDelete);
        }
    }

    void OnSlotSelected(int slot)
    {
        if (isNewGameMode)
            SaveManager.Instance.StartNewGame(slot);
        else
            SaveManager.Instance.ContinueGame(slot);
    }

    void OnSlotDelete(int slot)
    {
        SaveManager.Instance.DeleteSlot(slot);
        RefreshSlots();
        RefreshContinueAvailability();   // 刪檔後可能沒存檔了，更新繼續遊戲狀態
    }
}