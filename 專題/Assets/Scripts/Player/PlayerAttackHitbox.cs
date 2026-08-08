using UnityEngine;

/// <summary>
/// 玩家攻擊判定框。
/// 掛在武器/攻擊範圍的子物件（含 BoxCollider2D，設為 Trigger）上。
/// 只做一件事：判定框啟用期間，偵測碰到的敵人並套用傷害。
/// 啟用/關閉時機由 PlayerCombatController 透過動畫事件控制。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Tooltip("可被打到的目標所在 Layer（例如 Enemy）")]
    [SerializeField] LayerMask targetLayer;

    PlayerStats stats;
    BoxCollider2D box;

    // 這一次攻擊已經打過的目標，避免同一擊重複觸發
    readonly System.Collections.Generic.HashSet<Collider2D> hitThisSwing = new();

    // 當前這一擊的傷害倍率（不同連段段數可不同）
    float currentDamageMultiplier = 1f;

    void Awake()
    {
        stats = GetComponentInParent<PlayerStats>();
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.enabled = false;   // 預設關閉，攻擊時才開
    }

    /// <summary>開啟判定（由動畫事件在命中幀呼叫）。</summary>
    public void EnableHitbox(float damageMultiplier = 1f)
    {
        currentDamageMultiplier = damageMultiplier;
        hitThisSwing.Clear();
        box.enabled = true;
    }

    /// <summary>關閉判定（由動畫事件在命中幀結束呼叫）。</summary>
    public void DisableHitbox()
    {
        box.enabled = false;
        hitThisSwing.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 不在目標 Layer 就忽略
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;
        // 這一擊已打過就跳過
        if (hitThisSwing.Contains(other)) return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            hitThisSwing.Add(other);

            float damage = stats.AttackPower * currentDamageMultiplier;

            // 擊退方向依玩家面向
            float facing = Mathf.Sign(transform.root.localScale.x);
            Vector2 knockback = new Vector2(
                facing * stats.KnockbackForce,
                stats.KnockbackForce * 0.3f);

            target.TakeDamage(damage, knockback);
        }
    }

    void OnDrawGizmosSelected()
    {
        var b = GetComponent<BoxCollider2D>();
        if (b == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(b.offset, b.size);
    }
}