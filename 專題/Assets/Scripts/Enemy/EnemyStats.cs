using UnityEngine;

/// <summary>
/// 敵人基礎屬性數值庫。只存數值，不含邏輯。
/// </summary>
public class EnemyStats : MonoBehaviour
{
    [Header("生命")]
    [SerializeField] float maxHealth = 30f;

    [Header("攻擊")]
    [SerializeField] float attackPower = 10f;
    [SerializeField] float knockbackForce = 8f;

    [Header("移動")]
    [SerializeField] float moveSpeed = 2.5f;

    [Header("AI 範圍")]
    [Tooltip("偵測到玩家的距離")]
    [SerializeField] float detectRange = 6f;
    [Tooltip("進入攻擊的距離")]
    [SerializeField] float attackRange = 1.2f;
    [Tooltip("攻擊冷卻時間")]
    [SerializeField] float attackCooldown = 1.5f;

    public float MaxHealth => maxHealth;
    public float AttackPower => attackPower;
    public float KnockbackForce => knockbackForce;
    public float MoveSpeed => moveSpeed;
    public float DetectRange => detectRange;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
}