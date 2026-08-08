using System;
using UnityEngine;

/// <summary>
/// 基礎屬性數值庫。
/// 核心原則：只存「資料本體（數值）」，不含任何遊戲邏輯。
/// 不寫受擊無敵、不寫動畫切換 —— 那些是各控制器的責任。
/// 主角、怪物、未來的裝備系統都統一從這裡查詢/修改數值。
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("生命")]
    [SerializeField] float maxHealth = 100f;

    [Header("攻擊")]
    [SerializeField] float attackPower = 20f;
    [Tooltip("攻擊後產生的擊退力道")]
    [SerializeField] float knockbackForce = 10f;

    [Header("防禦")]
    [Tooltip("基礎減傷值（防禦架勢時的額外減免另計）")]
    [SerializeField] float defense = 0f;

    [Header("移動")]
    [SerializeField] float moveSpeed = 8f;

    // ---- 數值變更事件（裝備、buff 系統可訂閱）----
    public event Action OnStatsChanged;

    // ---- 唯讀存取（外部查詢用）----
    public float MaxHealth => maxHealth;
    public float AttackPower => attackPower;
    public float KnockbackForce => knockbackForce;
    public float Defense => defense;
    public float MoveSpeed => moveSpeed;

    // ---- 修改介面（裝備、升級、buff 呼叫）----

    public void SetAttackPower(float value)
    {
        attackPower = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    public void AddAttackPower(float delta)
    {
        attackPower = Mathf.Max(0, attackPower + delta);
        OnStatsChanged?.Invoke();
    }

    public void SetDefense(float value)
    {
        defense = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    public void AddDefense(float delta)
    {
        defense = Mathf.Max(0, defense + delta);
        OnStatsChanged?.Invoke();
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed = Mathf.Max(0, value);
        OnStatsChanged?.Invoke();
    }

    /// <summary>計算實際承受傷害（套用防禦）。純計算，不改任何狀態。</summary>
    public float CalculateDamageTaken(float rawDamage, float extraDefense = 0)
    {
        return Mathf.Max(1, rawDamage - defense - extraDefense);
    }
}