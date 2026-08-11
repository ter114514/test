using UnityEngine;

public class HealthFountain : MonoBehaviour
{
    [Header("--- 回血設定 ---")]
    [Tooltip("按下 E 鍵要恢復多少血量？（設為 100 就等於直接補滿）")]
    public int healAmount = 100;

    [Header("--- UI 提示設定（可選） ---")]
    [Tooltip("靠近時要顯示的提示圖片或文字 UI 物件（例如 [E] 鍵提示）")]
    public GameObject interactPrompt;

    private bool isPlayerInside = false; // 玩家是否在觸發區域內
    private PlayerHealth playerHealth;  // 玩家的血量腳本組件

    void Start()
    {
        // 遊戲開局預設關閉 [E] 鍵提示
        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
        }
    }

    void Update()
    {
        // 💥 當玩家站在觸發區內，且按下鍵盤 E 鍵時發動
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            if (playerHealth != null)
            {
                // 檢查玩家是不是已經滿血，滿血就不浪費或跳提示
                if (playerHealth.currentHealth < playerHealth.maxHealth)
                {
                    playerHealth.Heal(healAmount);
                    Debug.Log("【回血神壇】成功按下 E 鍵補血！");
                }
                else
                {
                    Debug.Log("【回血神壇】Aria 目前已經是滿血狀態，不需要補血！");
                }
            }
        }
    }

    // 💥【2D 觸發偵測】：當 Aria 走進這座神壇的範圍時
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 判斷是不是主角 Aria (標籤為 Player)
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerHealth = collision.GetComponent<PlayerHealth>();

            // 顯示 [E] 鍵互動提示
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(true);
            }

            Debug.Log("【回血神壇】Aria 進入回血區域！請按 E 鍵恢復生命。");
        }
    }

    // 💥【2D 觸發偵測】：當 Aria 離開這座神壇的範圍時
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerHealth = null;

            // 隱藏 [E] 鍵互動提示
            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }

            Debug.Log("【回血神壇】Aria 離開了回血區域。");
        }
    }
}