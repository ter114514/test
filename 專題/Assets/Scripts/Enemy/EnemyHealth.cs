using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] float hurtFlashTime = 0.1f;
    [Tooltip("死亡動畫播放時間，播完才銷毀")]
    [SerializeField] float deathAnimTime = 1f;

    static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    EnemyStats stats;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator animator;
    Color originalColor;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        if (sr != null) originalColor = sr.color;
    }

    void Start()
    {
        CurrentHealth = stats.MaxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, stats.MaxHealth);
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsDead || amount <= 0) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, stats.MaxHealth);

        if (rb != null)
            rb.linearVelocity = knockback;

        if (sr != null)
            StartCoroutine(HurtFlash());

        if (CurrentHealth <= 0)
            Die();
    }

    IEnumerator HurtFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(hurtFlashTime);
        if (!IsDead) sr.color = originalColor;   // 死亡時不還原，避免蓋掉死亡表現
    }

    void Die()
    {
        IsDead = true;
        OnDeath?.Invoke();

        // 觸發死亡動畫
        if (animator != null)
            animator.SetBool(IsDeadHash, true);

        // 停止移動、關掉碰撞避免死屍還擋路或被再次攻擊
        if (rb != null) rb.linearVelocity = Vector2.zero;
        foreach (var col in GetComponents<Collider2D>())
            col.enabled = false;

        // 等死亡動畫播完再銷毀
        Destroy(gameObject, deathAnimTime);
    }
}