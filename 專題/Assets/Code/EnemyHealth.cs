using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("--- 血量設定 ---")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("--- 🛡️ 被彈反與破防設定 ---")]
    [Tooltip("被彈反幾次會觸發破防眩暈")]
    public int maxParryHitsToStun = 2;

    [Tooltip("破防眩暈持續時間（秒）")]
    public float stunDuration = 2.0f;

    [Tooltip("破防眩暈狀態下的受傷加成（0.5 代表額外承受 50% 傷害）")]
    public float stunDamageBonus = 0.5f;

    [Header("--- 狀態標記 ---")]
    public bool isStunned = false; // 解決 EnemyAI 的 isStunned 報錯
    private int currentParryHits = 0; // 當前被彈反的次數

    private Animator anim;
    private Coroutine stunCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    // 🔴 核心受傷入口 (玩家一般攻擊或武器碰撞呼叫這裡)
    public void TakeDamage(int damage)
    {
        int finalDamage = damage;

        // 💥 如果怪物處於破防眩暈狀態，受到額外 50% 傷害加成！
        if (isStunned)
        {
            finalDamage = Mathf.RoundToInt(damage * (1f + stunDamageBonus));
            Debug.LogWarning($"💥【破防追擊！】{gameObject.name} 處於眩暈中，承受 150% 傷害：{finalDamage} 點！");
        }

        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name} 受到了 {finalDamage} 點傷害，剩餘血量: {currentHealth}/{maxHealth}");

        // 播放受傷動畫 (保持使用你原本的 isHurt)
        if (anim != null && !isStunned)
        {
            anim.SetTrigger("isHurt");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // ⚡ 解決 PlayerHealth 的 OnParriedByPlayer 報錯
    // 當這隻怪物「攻擊玩家被彈反成功」時由 PlayerHealth 呼叫
    public void OnParriedByPlayer()
    {
        // 1. 扣除最大血量的 10% (最低至少扣 1 點)
        int parryDamage = Mathf.Max(1, Mathf.RoundToInt(maxHealth * 0.1f));
        currentHealth -= parryDamage;
        if (currentHealth < 0) currentHealth = 0;

        currentParryHits++;
        Debug.LogWarning($"⚔️【{gameObject.name} 被彈反！】受到 10% 最大血量傷害 ({parryDamage} 點)！當前被彈反次數：{currentParryHits}/{maxParryHitsToStun}");

        // 2. 檢查血量是否歸零
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 3. 檢查是否達到破防次數 (2 次)
        if (currentParryHits >= maxParryHitsToStun)
        {
            TriggerGuardBreak();
        }
        else
        {
            if (anim != null) anim.SetTrigger("isHurt"); // 被彈開時播放受傷/硬直動畫
        }
    }

    // 💥 觸發怪物破防眩暈
    private void TriggerGuardBreak()
    {
        Debug.LogError($"💥💥【{gameObject.name} 破防！】姿態被打破，進入 {stunDuration} 秒眩暈狀態！");
        currentParryHits = 0; // 重置計數

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
        }
        stunCoroutine = StartCoroutine(StunRoutine());
    }

    // 💥 怪物眩暈協程 (2 秒)
    private IEnumerator StunRoutine()
    {
        isStunned = true;

        if (anim != null)
        {
            anim.SetBool("isStunned", true); // 可選：Animator 裡若有布林值 isStunned 可切換眩暈動作
        }

        // 2 秒眩暈黃金輸出時間
        yield return new WaitForSeconds(stunDuration);

        isStunned = false;

        if (anim != null)
        {
            anim.SetBool("isStunned", false);
        }

        Debug.Log($"【{gameObject.name} 狀態】眩暈結束，恢復意識！");
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 死了！");
        if (anim != null) anim.SetTrigger("isDead"); // 保持使用你原本的 isDead
        
        // 停用 Collider 防止死後還能被打或擋路
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 0.5f); // 延遲 0.5 秒讓死亡動畫播完
    }

    // 💥 保留原本的功能：只要主角的武器 (Tag: Weapon) 揮到就自動扣血
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            TakeDamage(1); // 每次被打到就扣 1 點血 (若在眩暈中會自動經由 TakeDamage 計算成 150% 傷害)
        }
    }
}