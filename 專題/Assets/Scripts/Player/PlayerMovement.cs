using UnityEngine;

/// <summary>
/// 玩家移動系統。訂閱 PlayerInputHandler 的輸入事件，
/// 在 FixedUpdate 以 Rigidbody2D 做物理移動。
/// 內建土狼時間（Coyote Time）與跳躍緩衝（Jump Buffer）。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInputHandler))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動")]
    public float moveSpeed = 8f;
    [Tooltip("加速到目標速度的反應快慢，越大越靈敏")]
    public float acceleration = 60f;
    public float deceleration = 70f;

    [Header("跳躍")]
    public float jumpForce = 16f;
    [Tooltip("放開跳躍鍵後，上升速度衰減的倍率（可變跳躍高度）")]
    public float jumpCutMultiplier = 0.5f;
    [Tooltip("下落時額外重力倍率，讓跳躍更俐落")]
    public float fallGravityMultiplier = 2f;

    [Header("手感輔助")]
    [Tooltip("離開地面後仍可跳躍的寬容時間")]
    public float coyoteTime = 0.1f;
    [Tooltip("落地前提早按跳躍會被記住的時間")]
    public float jumpBufferTime = 0.1f;

    [Header("地面偵測")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    Rigidbody2D rb;
    PlayerInputHandler input;

    float moveInput;          // 訂閱事件保存的當前水平輸入
    bool isGrounded;
    float coyoteCounter;      // 土狼時間倒數
    float jumpBufferCounter;  // 跳躍緩衝倒數
    bool jumpHeld;            // 跳躍鍵是否按住
    int facingDir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerInputHandler>();
    }

    void OnEnable()
    {
        input.OnMove += SetMove;
        input.OnJumpPressed += OnJumpPressed;
        input.OnJumpReleased += OnJumpReleased;
    }

    void OnDisable()
    {
        input.OnMove -= SetMove;
        input.OnJumpPressed -= OnJumpPressed;
        input.OnJumpReleased -= OnJumpReleased;
    }

    // ---- 事件回呼：只保存狀態，不做物理 ----

    void SetMove(float value) => moveInput = value;

    void OnJumpPressed()
    {
        // 按下瞬間啟動緩衝計時，實際能不能跳交給 FixedUpdate 判斷
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
    }

    void OnJumpReleased()
    {
        jumpHeld = false;
        // 可變跳躍高度：上升途中放開，立刻砍掉部分上升速度
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier);
    }

    void Update()
    {
        // 計時器放在 Update 用 deltaTime 累減，判定放在 FixedUpdate
        UpdateTimers();
        UpdateFacing();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleHorizontal();
        HandleJump();
        HandleBetterGravity();
    }

    // ---- 計時 ----

    void UpdateTimers()
    {
        if (isGrounded)
            coyoteCounter = coyoteTime;       // 著地時刷新土狼時間
        else
            coyoteCounter -= Time.deltaTime;

        jumpBufferCounter -= Time.deltaTime;  // 緩衝持續倒數
    }

    void UpdateFacing()
    {
        if (moveInput > 0.01f) facingDir = 1;
        else if (moveInput < -0.01f) facingDir = -1;
        transform.localScale = new Vector3(facingDir, 1, 1);
    }

    // ---- 物理 ----

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
    }

    void HandleHorizontal()
    {
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        // 有輸入時用加速度，放開時用減速度，手感更緊實
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float movement = speedDiff * accelRate;

        rb.AddForce(Vector2.right * movement * Time.fixedDeltaTime, ForceMode2D.Impulse);
    }

    void HandleJump()
    {
        // 緩衝與土狼時間都還有效才起跳
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;   // 用掉緩衝
            coyoteCounter = 0;       // 用掉土狼，避免二段跳
        }
    }

    void HandleBetterGravity()
    {
        // 下落階段加重重力，讓滯空感不拖泥帶水
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y
                * (fallGravityMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}