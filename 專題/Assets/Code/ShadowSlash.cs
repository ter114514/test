using UnityEngine;
using System.Collections;

public class ShadowSlash : MonoBehaviour
{
    [Header("--- ⚔️ 技能基礎設定 ---")]
    public KeyCode skillKey = KeyCode.T;       // 技能快捷鍵
    public float cooldown = 6.0f;               // 基礎冷卻時間 (秒)
    private float nextCastTime = 0f;

    [Header("--- 🖥️ UI 冷卻設定 ---")]
    [Tooltip("將影襲技能的 SkillCooldownUI 物件拖到這裡")]
    public SkillCooldownUI cooldownUI;          // 冷卻視覺化 UI 組件

    [Header("--- 🩸 血液消耗設定 ---")]
    [Tooltip("釋放影襲消耗的血液百分比 (0.1 代表 10%)")]
    public float bloodCostPercent = 0.1f;

    [Header("--- 🎯 鎖定與位移設定 ---")]
    public float maxSearchDistance = 8.0f;      // 搜尋前方的最大中距離 (預設 8 格)
    public float teleportOffset = 1.2f;        // 突進到敵人身後幾格的位置
    public LayerMask enemyLayer;               // 敵人 Layer
    public LayerMask groundLayer;              // 地面/牆壁 Layer (避免突進卡進牆壁)

    [Header("--- 💥 傷害與攻擊設定 ---")]
    public int baseDamage = 25;                // 基礎攻擊傷害
    public float attackAreaRadius = 1.8f;      // 交叉斬擊的傷害判定範圍

    // 內部組件參考
    private Animator anim;
    private VampireBlood vampireBlood;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        vampireBlood = GetComponent<VampireBlood>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(skillKey))
        {
            TryCastShadowSlash();
        }
    }

    /// <summary>
    /// 嘗試發動技能（檢查冷卻、搜尋敵人與扣除血液）
    /// </summary>
    public void TryCastShadowSlash()
    {
        // 1. 檢查冷卻時間
        if (Time.time < nextCastTime)
        {
            float remaining = nextCastTime - Time.time;
            Debug.LogWarning($"⏳【影襲】冷卻中！剩餘 {remaining:F1} 秒");
            return;
        }

        // 2. 搜尋前方目標
        Transform targetEnemy = FindFrontEnemy();

        if (targetEnemy == null)
        {
            Debug.Log("🦇【影襲】前方中距離內沒有可鎖定的敵人！");
            return;
        }

        // 3. 檢查並扣除 10% 吸血值 (傳入 bloodCostPercent)
        if (vampireBlood != null && !vampireBlood.ConsumeBloodPercent(bloodCostPercent))
        {
            return;
        }

        // 4. 條件皆滿足，執行影襲突進與斬擊
        ExecuteShadowSlash(targetEnemy);
    }

    /// <summary>
    /// 搜尋玩家前方中距離內最近的敵人
    /// </summary>
    private Transform FindFrontEnemy()
    {
        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        
        // 往前發射圓形雜湊或多重射線鎖定敵人
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 2.0f, facingDir, maxSearchDistance, enemyLayer);

        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            // 確保目標在玩家面對的前方
            float dirToEnemy = hit.transform.position.x - transform.position.x;
            bool isFacing = (facingDir.x > 0 && dirToEnemy > 0) || (facingDir.x < 0 && dirToEnemy < 0);

            if (isFacing)
            {
                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestEnemy = hit.transform;
                }
            }
        }

        return closestEnemy;
    }

    /// <summary>
    /// 執行突進與交叉斬擊
    /// </summary>
    private void ExecuteShadowSlash(Transform target)
    {
        // 進入冷卻
        nextCastTime = Time.time + cooldown;

        // 💥【新增】觸發 UI 轉圈倒數
        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(cooldown);
        }

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth == null) enemyHealth = target.GetComponentInParent<EnemyHealth>();

        // 1. 計算目標身後的位置
        float enemyFacingDir = target.localScale.x > 0 ? 1f : -1f;
        
        float attackDirSign = Mathf.Sign(target.position.x - transform.position.x);
        Vector3 targetBehindPos = target.position + new Vector3(attackDirSign * teleportOffset, 0f, 0f);

        // 牆壁安全檢測：防止閃現進牆壁或地圖外
        RaycastHit2D wallCheck = Physics2D.Raycast(target.position, new Vector2(attackDirSign, 0), teleportOffset, groundLayer);
        if (wallCheck.collider != null)
        {
            targetBehindPos = wallCheck.point - new Vector2(attackDirSign * 0.3f, 0);
        }

        // 2. 突進/瞬間移動至敵人身後
        transform.position = new Vector3(targetBehindPos.x, targetBehindPos.y, transform.position.z);

        // 3. 轉向面對敵人 (身後位移後要轉頭打他)
        float newFacingDir = target.position.x - transform.position.x;
        if ((newFacingDir > 0 && transform.localScale.x < 0) || (newFacingDir < 0 && transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        // 4. 播放交叉斬擊動畫
        if (anim != null) anim.SetTrigger("shadowSlash");

        // 5. 計算傷害（若處於爆走狀態則套用 100% 血液加成倍率）
        int finalDamage = baseDamage;
        if (vampireBlood != null)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * vampireBlood.GetCurrentDamageMultiplier());
        }

        // 6. 造成傷害並檢查擊殺
        if (enemyHealth != null)
        {
            bool wasAlive = enemyHealth.currentHealth > 0;
            enemyHealth.TakeDamage(finalDamage);

            // 🌟 核心機制：擊殺敵人刷新冷卻
            if (wasAlive && enemyHealth.currentHealth <= 0)
            {
                ResetCooldown();
                Debug.LogWarning($"⚔️💀【影襲擊殺！】成功斬殺 {target.name}，技能冷卻已直接重置！");
            }
            else
            {
                Debug.Log($"⚔️【影襲成功】對 {target.name} 造成 {finalDamage} 點傷害。");
            }
        }
    }

    /// <summary>
    /// 重置冷卻時間 (可供外部或擊殺時呼叫)
    /// </summary>
    public void ResetCooldown()
    {
        nextCastTime = Time.time;

        // 💥【新增】擊殺重置時，同步清空 UI 的冷卻遮罩與倒數數字！
        if (cooldownUI != null)
        {
            cooldownUI.ResetUI();
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 繪製搜尋視窗範圍
        Gizmos.color = Color.cyan;
        Vector3 dir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(transform.position, dir * maxSearchDistance);
    }
}