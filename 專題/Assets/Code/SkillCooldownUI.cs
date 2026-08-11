using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillCooldownUI : MonoBehaviour
{
    [Header("--- UI 組件綁定 ---")]
    [Tooltip("半透明黑色轉圈遮罩 Image (Image Type 必須設為 Filled)")]
    public Image cooldownOverlay;

    [Tooltip("顯示倒數秒數的 TextMeshPro")]
    public TextMeshProUGUI cooldownText;

    // 內部記錄變數
    private float cooldownDuration = 0f;
    private float currentCooldownTimer = 0f;
    private bool isOnCooldown = false;

    void Start()
    {
        // 初始狀態：關閉冷卻遮罩與數字
        ResetUI();
    }

    void Update()
    {
        if (!isOnCooldown) return;

        // 1. 倒數計時
        currentCooldownTimer -= Time.deltaTime;

        if (currentCooldownTimer > 0)
        {
            // 2. 更新遮罩比例 (1 -> 0)
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = currentCooldownTimer / cooldownDuration;
            }

            // 3. 更新倒數數字 (例如 3.5s 或 2s)
            if (cooldownText != null)
            {
                // 大於 1 秒顯示一位小數，小於 1 秒顯示零點幾秒
                cooldownText.text = currentCooldownTimer >= 1.0f 
                    ? Mathf.CeilToInt(currentCooldownTimer).ToString() 
                    : currentCooldownTimer.ToString("F1");
            }
        }
        else
        {
            // 冷卻結束
            ResetUI();
        }
    }

    /// <summary>
    /// 💥 主動觸發冷卻 UI 旋轉與倒數 (供技能發射時呼叫)
    /// </summary>
    /// <param name="duration">該技能的總冷卻秒數</param>
    public void StartCooldown(float duration)
    {
        if (duration <= 0) return;

        cooldownDuration = duration;
        currentCooldownTimer = duration;
        isOnCooldown = true;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f; // 從滿的開始倒數
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 重置 UI (冷卻完畢或手動刷新時)
    /// </summary>
    public void ResetUI()
    {
        isOnCooldown = false;
        currentCooldownTimer = 0f;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.gameObject.SetActive(false);
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
}