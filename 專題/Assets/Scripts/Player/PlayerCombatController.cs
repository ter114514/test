using UnityEngine;

/// <summary>
/// 玩家戰鬥控制器。
/// 職責：連段(Combo)時機管理、防禦架勢切換、攻擊/防禦硬直狀態控制，
/// 並與 Animator 溝通。判定框的實際傷害由 PlayerAttackHitbox 處理。
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerCombatController : MonoBehaviour
{
    static readonly int AttackTrigger = Animator.StringToHash("Attack");
    static readonly int ComboStepHash = Animator.StringToHash("ComboStep");
    static readonly int IsBlockingHash = Animator.StringToHash("IsBlocking");

    [Header("連段設定")]
    [Tooltip("最大連段數")]
    [SerializeField] int maxCombo = 3;
    [Tooltip("連段輸入窗口：上一擊後多久內按攻擊才接得上")]
    [SerializeField] float comboWindow = 0.5f;

    [Header("引用")]
    [SerializeField] PlayerAttackHitbox hitbox;

    // ---- 對外唯讀狀態（移動系統可讀，硬直時禁止移動）----
    public bool IsAttacking { get; private set; }
    public bool IsBlocking { get; private set; }
    /// <summary>硬直中（攻擊或防禦），移動系統應據此限制行動</summary>
    public bool IsInHardState => IsAttacking || IsBlocking;

    Animator animator;
    PlayerInputHandler input;

    int comboStep = 0;
    float lastAttackTime = -999f;
    bool canQueueNext = false;   // 是否開放接下一段（由動畫事件控制）

    void Awake()
    {
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        input.OnAttackPressed += HandleAttackInput;
        // 假設 InputHandler 有防禦事件；若沒有，見下方說明
        input.OnBlockPressed += StartBlock;
        input.OnBlockReleased += EndBlock;
    }

    void OnDisable()
    {
        input.OnAttackPressed -= HandleAttackInput;
        input.OnBlockPressed -= StartBlock;
        input.OnBlockReleased -= EndBlock;
    }

    void Update()
    {
        // 超過連段窗口就重置段數
        if (Time.time - lastAttackTime > comboWindow && !IsAttacking)
            comboStep = 0;
    }

    // ---- 攻擊連段 ----

    void HandleAttackInput()
    {
        if (IsBlocking) return;   // 防禦中不能攻擊

        // 第一擊，或在開放窗口內接續下一段
        if (!IsAttacking)
        {
            StartAttack();
        }
        else if (canQueueNext)
        {
            StartAttack();
        }
    }

    void StartAttack()
    {
        comboStep = Mathf.Min(comboStep + 1, maxCombo);
        lastAttackTime = Time.time;
        IsAttacking = true;
        canQueueNext = false;

        animator.SetInteger(ComboStepHash, comboStep);
        animator.SetTrigger(AttackTrigger);
    }

    // ---- 以下方法由「動畫事件(Animation Event)」呼叫 ----

    /// <summary>命中幀：開啟判定框。在攻擊動畫的出手幀加 Animation Event。</summary>
    public void AnimEvent_EnableHitbox()
    {
        // 可依段數給不同倍率，這裡示範第三段加重
        float mult = comboStep >= maxCombo ? 1.5f : 1f;
        hitbox.EnableHitbox(mult);
    }

    /// <summary>命中幀結束：關閉判定框。</summary>
    public void AnimEvent_DisableHitbox()
    {
        hitbox.DisableHitbox();
    }

    /// <summary>開放連段輸入窗口：動畫接近尾聲時加此事件，玩家可接下一段。</summary>
    public void AnimEvent_OpenComboWindow()
    {
        canQueueNext = true;
    }

    /// <summary>攻擊動畫結束：解除攻擊硬直。放在每個攻擊 Clip 最後一幀。</summary>
    public void AnimEvent_AttackEnd()
    {
        IsAttacking = false;
        canQueueNext = false;
    }

    // ---- 防禦架勢 ----

    void StartBlock()
    {
        if (IsAttacking) return;   // 攻擊硬直中不能突然舉盾
        IsBlocking = true;
        animator.SetBool(IsBlockingHash, true);
    }

    void EndBlock()
    {
        IsBlocking = false;
        animator.SetBool(IsBlockingHash, false);
    }
}