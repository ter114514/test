using System.Collections;
using UnityEngine;

/// <summary>
/// 站定間歇攻擊的敵人，攻擊時向面向方向撲擊位移。
/// </summary>
[RequireComponent(typeof(EnemyStats), typeof(Rigidbody2D))]
public class EnemyAttacker : MonoBehaviour
{
    [Header("攻擊判定範圍")]
    [SerializeField] Vector2 attackBoxSize = new Vector2(1.2f, 1f);
    [SerializeField] float attackReach = 0.8f;
    [SerializeField] LayerMask targetLayer;   // 設成 Player

    [Header("攻擊位移（撲擊）")]
    [Tooltip("攻擊時向前衝的力道")]
    [SerializeField] float lungeForce = 8f;
    [Tooltip("從觸發攻擊到實際位移+判定的延遲（配合動畫出手時機）")]
    [SerializeField] float lungeDelay = 0.2f;
    [Tooltip("撲擊持續時間，之後煞停")]
    [SerializeField] float lungeDuration = 0.15f;

    static readonly int AttackHash = Animator.StringToHash("Attack");

    EnemyStats stats;
    EnemyHealth health;
    Rigidbody2D rb;
    Animator animator;
    float lastAttackTime;
    bool isAttacking;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;
        if (isAttacking) return;   // 攻擊中不重複觸發

        if (Time.time >= lastAttackTime + stats.AttackCooldown)
        {
            lastAttackTime = Time.time;
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // 1. 播攻擊動畫
        if (animator != null)
            animator.SetTrigger(AttackHash);

        // 2. 等到出手時機（配合動畫）
        yield return new WaitForSeconds(lungeDelay);

        if (health != null && health.IsDead) { isAttacking = false; yield break; }

        // 3. 向前撲擊位移
        float facing = Mathf.Sign(transform.localScale.x);
        if (facing == 0) facing = 1;
        rb.linearVelocity = new Vector2(-facing * lungeForce, rb.linearVelocity.y);

        // 4. 撲擊瞬間做傷害判定
        DoAttackHit(facing);

        // 5. 撲擊持續一小段後煞停
        yield return new WaitForSeconds(lungeDuration);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        isAttacking = false;
    }

    void DoAttackHit(float facing)
    {
        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attackReach;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0, targetLayer);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var target))
            {
                Vector2 knockback = new Vector2(
                    facing * stats.KnockbackForce,
                    stats.KnockbackForce * 0.3f);
                target.TakeDamage(stats.AttackPower, knockback);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        if (facing == 0) facing = 1;
        Vector2 center = (Vector2)transform.position + Vector2.right * facing * attackReach;
        Gizmos.color = new Color(1, 0.4f, 0, 0.6f);
        Gizmos.DrawWireCube(center, attackBoxSize);
    }
}