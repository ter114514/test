using UnityEngine;
using System.Collections;

public class BloodBurst : MonoBehaviour
{
    [Header("--- ⚔️ 技能基礎設定 ---")]
    public KeyCode skillKey = KeyCode.F;        // 技能快捷鍵 (預設 F 鍵)
    public float cooldown = 15.0f;              // 大招冷卻時間 (秒)
    private float nextCastTime = 0f;

    [Header("--- 🖥️ UI 冷卻設定 ---")]
    [Tooltip("將剛剛做好的 SkillCooldownUI 物件拖到這裡")]
    public SkillCooldownUI cooldownUI;          // 冷卻視覺化 UI 組件

    [Header("--- 💥 衝擊波與傷害設定 ---")]
    public int baseDamage = 60;                 // 基礎高額傷害
    public float burstRadius = 4.5f;            // 衝擊波傷害範圍半徑
    public LayerMask enemyLayer;               // 敵人 Layer

    [Header("--- 🛡️ 無敵時間設定 ---")]
    public float invincibleDuration = 5.0f;     // 獲得的無敵時間 (秒)

    [Header("--- ✨ 特效預製體 (選填) ---")]
    public GameObject burstVFXPrefab;           // 衝擊波特效 Prefab

    // 組件參考
    private Animator anim;
    private VampireBlood vampireBlood;
    private PlayerHealth playerHealth;

    void Start()
    {
        anim = GetComponent<Animator>();
        vampireBlood = GetComponent<VampireBlood>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (Input.GetKeyDown(skillKey))
        {
            TryCastBloodBurst();
        }
    }

    /// <summary>
    /// 嘗試發動大招（檢查冷卻與血液存量）
    /// </summary>
    public void TryCastBloodBurst()
    {
        // 1. 檢查冷卻時間
        if (Time.time < nextCastTime)
        {
            float remaining = nextCastTime - Time.time;
            Debug.LogWarning($"⏳【血爆】大招冷卻中！剩餘 {remaining:F1} 秒");
            return;
        }

        // 2. 檢查是否有吸血值 (至少要小於等於 0 就不能放)
        if (vampireBlood == null || vampireBlood.currentBlood <= 0.01f)
        {
            Debug.LogWarning("❌【血爆失敗】當前吸血值為 0，無法發動血爆！");
            return;
        }

        ExecuteBloodBurst();
    }

    private void ExecuteBloodBurst()
    {
        // 進入冷卻
        nextCastTime = Time.time + cooldown;

        // 💥【新增】觸發 UI 轉圈倒數
        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(cooldown);
        }

        // 1. 紀錄發動時的血液量，並「消耗全部吸血值」
        float consumedBlood = vampireBlood.currentBlood;
        
        // 取得當前傷害加成倍率 (若發動前剛好是 100% 爆走，可吃到倍率)
        float damageMultiplier = vampireBlood.GetCurrentDamageMultiplier();
        
        // 歸零血液
        vampireBlood.SetBlood(0f);

        // 2. 播放大招動畫
        if (anim != null) anim.SetTrigger("bloodBurst");

        // 3. 生成衝擊波視覺特效
        if (burstVFXPrefab != null)
        {
            Instantiate(burstVFXPrefab, transform.position, Quaternion.identity);
        }

        // 4. 計算最終傷害並對範圍內敵人造成打擊
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, burstRadius, enemyLayer);
        foreach (var enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponentInParent<EnemyHealth>();
            EnemyAI enemyAI = enemyCollider.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
            }

            // 給予周圍敵人強力的向外彈飛擊退
            if (enemyAI != null)
            {
                Vector2 pushDirection = (enemyCollider.transform.position - transform.position).normalized;
                Vector2 knockbackForce = (pushDirection + Vector2.up * 0.3f).normalized * 25f; // 擊退速度 25
                enemyAI.ApplyKnockback(knockbackForce, 0.4f);
            }
        }

        // 5. 賦予玩家 5 秒無敵時間
        if (playerHealth != null)
        {
            // 呼叫 PlayerHealth 的無敵介面
            playerHealth.TriggerInvincibility(invincibleDuration);
        }

        Debug.LogWarning($"🔥🩸💀【血祭爆發！】消耗了 {consumedBlood:F1} 點血液，造成 {finalDamage} 點大範圍傷害，並獲得 {invincibleDuration} 秒無敵狀態！");
    }

    private void OnDrawGizmosSelected()
    {
        // 繪製衝擊波範圍
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, burstRadius);
    }
}