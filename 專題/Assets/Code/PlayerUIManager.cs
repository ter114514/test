using UnityEngine;
using UnityEngine.UI;
using TMPro; // 【新加的】這樣才能控制 TextMeshPro 文字

public class PlayerUIManager : MonoBehaviour
{
    [Header("對接的組件")]
    public PlayerHealth playerHealth; 

    [Header("UI 元素")]
    public Slider healthSlider;       
    public TextMeshProUGUI healthText; // 【新加的】拖入剛剛建立的 HealthText
    public Image[] shieldIcons;       

    void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (healthSlider != null && playerHealth != null)
        {
            healthSlider.maxValue = playerHealth.maxHealth;
            healthSlider.value = playerHealth.currentHealth;
        }
        
        // 初始更新一次文字
        UpdateHealthText();
    }

    void Update()
    {
        if (playerHealth == null) return;

        // 1. 即時更新血條
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, playerHealth.currentHealth, Time.deltaTime * 10f);
        }

        // 2. 即時更新血量文字 【新加的】
        UpdateHealthText();

        // 3. 即時更新 5 格防禦耐力
        int currentShield = GetCurrentShieldValue();
        for (int i = 0; i < shieldIcons.Length; i++)
        {
            if (shieldIcons[i] != null)
            {
                if (i < currentShield) shieldIcons[i].enabled = true; 
                else shieldIcons[i].enabled = false; 
            }
        }
    }

    // 更新血量文字的功能 【新加的】
    void UpdateHealthText()
    {
        if (healthText != null && playerHealth != null)
        {
            // 確保血量不會變成負數
            int displayHealth = Mathf.Max(0, playerHealth.currentHealth);
            
            // 把文字設定成 "目前血量 / 最大血量" (例如: 80 / 100)
            healthText.text = displayHealth + " / " + playerHealth.maxHealth;
        }
    }

    private int GetCurrentShieldValue()
    {
        if (playerHealth == null) return 0;
        var field = typeof(PlayerHealth).GetField("currentShieldValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (int)field.GetValue(playerHealth) : 0;
    }
}