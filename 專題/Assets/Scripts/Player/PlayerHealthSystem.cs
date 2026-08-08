using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家生命狀態系統。
/// 職責：管理當前 HP、處理受擊扣血與無敵時間、發出生命相關事件。
/// HP 上限由 PlayerStats 提供（唯一數值來源），此處只管當前值與受擊邏輯。
/// 不處理移動、不處理攻擊判定。
/// </summary>
[RequireComponent(typeof(PlayerStats))]
public class PlayerHealthSystem : MonoBehaviour, IDamageable, ISaveable
{
    [Header("受擊無敵")]
    [Tooltip("受擊後無敵持續時間")]
    [SerializeField] float invincibleTime = 1f;
    [Tooltip("無敵期間閃爍間隔")]
    [SerializeField] float blinkInterval = 0.1f;

    // ---- 對外事件（UI、音效、動畫等訂閱）----

    /// <summary>生命值變化時發送：(當前值, 最大值)</summary>
    public event Action<float, float> OnHealthChanged;
    /// <summary>受到傷害時發送：傷害量</summary>
    public event Action<float> OnDamaged;
    /// <summary>治療時發送：治療量</summary>
    public event Action<float> OnHealed;
    /// <summary>死亡時發送</summary>
    public event Action OnDeath;
    /// <summary>無敵狀態切換時發送：是否無敵</summary>
    public event Action<bool> OnInvincibleChanged;

    // ---- 對外唯讀狀態 ----
    public float CurrentHealth { get; private set; }
    /// <summary>HP 上限轉發自 PlayerStats，維持單一數值來源</summary>
    public float MaxHealth => stats.MaxHealth;
    public bool IsInvincible { get; private set; }
    public bool IsDead { get; private set; }
    public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0;

    PlayerStats stats;
    SpriteRenderer sr;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        sr = GetComponentInChildren<SpriteRenderer>();
        CurrentHealth = stats.MaxHealth;
    }

    void Start()
    {
        // 讓 UI 在開場拿到初始值
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // ---- IDamageable 實作 ----

    /// <summary>受到傷害。無敵中或已死亡則忽略。傷害會套用 PlayerStats 的防禦計算。</summary>
    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsInvincible || IsDead || amount <= 0) return;

        // 套用防禦減傷（純計算由 PlayerStats 負責）
        float finalDamage = stats.CalculateDamageTaken(amount);

        CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);

        OnDamaged?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(InvincibilityRoutine());
    }

    // ---- 治療 ----

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

        OnHealed?.Invoke(amount);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    /// <summary>完全恢復（篝火休息用）</summary>
    public void FullHeal()
    {
        if (IsDead) return;
        CurrentHealth = MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // ---- 無敵時間 ----

    IEnumerator InvincibilityRoutine()
    {
        SetInvincible(true);

        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (sr != null) sr.enabled = true;
        SetInvincible(false);
    }

    void SetInvincible(bool value)
    {
        IsInvincible = value;
        OnInvincibleChanged?.Invoke(value);
    }

    // ---- 死亡 ----

    void Die()
    {
        IsDead = true;
        StopAllCoroutines();
        SetInvincible(false);
        if (sr != null) sr.enabled = true;
        OnDeath?.Invoke();
    }

    // ---- 重生 / 重置 ----

    public void ResetHealth()
    {
        StopAllCoroutines();
        CurrentHealth = MaxHealth;
        IsDead = false;
        IsInvincible = false;
        if (sr != null) sr.enabled = true;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    // ---- ISaveable 實作 ----

    public void SaveState(SaveData data)
    {
        // 只存當前值；上限由 PlayerStats 負責存檔，避免重複寫入互相覆蓋
        data.playerCurrentHealth = CurrentHealth;
    }

    public void LoadState(SaveData data)
    {
        CurrentHealth = Mathf.Clamp(data.playerCurrentHealth, 0, MaxHealth);
        IsDead = CurrentHealth <= 0;
        IsInvincible = false;
        if (sr != null) sr.enabled = true;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }
}