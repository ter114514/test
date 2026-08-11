using UnityEngine;

public class BloodItem : MonoBehaviour
{
    [Header("--- 補血設定 ---")]
    [Tooltip("拾取後要設定的血量")]
    public float targetBloodValue = 50f;

    [Header("--- 互動與冷卻設定 ---")]
    public KeyCode interactKey = KeyCode.E;
    
    [Tooltip("重複使用之間的冷卻時間 (秒)")]
    public float cooldown = 5.0f;
    private float nextInteractTime = 0f;

    private bool isPlayerInRange = false;
    private VampireBlood playerBlood;

    private void Update()
    {
        // 當玩家在範圍內、按下 E 鍵，且過冷卻時間
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (Time.time >= nextInteractTime)
            {
                CollectItem();
            }
            else
            {
                float remaining = nextInteractTime - Time.time;
                Debug.LogWarning($"⏳【血包冷卻中】還需要 {remaining:F1} 秒才能再次使用！");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerBlood = other.GetComponent<VampireBlood>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerBlood = null;
        }
    }

    private void CollectItem()
    {
        if (playerBlood != null)
        {
            // 設定冷卻結束時間點
            nextInteractTime = Time.time + cooldown;

            playerBlood.SetBlood(targetBloodValue);
            Debug.Log($"按 E 觸發了血池/血包！血量已重置為 {targetBloodValue}");
            
            // 💡 移除 Destroy(gameObject)，讓物件留在場景上可重複使用
        }
    }
}