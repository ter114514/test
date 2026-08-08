using UnityEngine;

/// <summary>
/// 玩家動畫驅動器。讀取移動狀態並更新 Animator 參數，
/// 不處理任何移動或輸入邏輯，只負責讓動畫反映當前狀態。
/// </summary>
[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerAnimator : MonoBehaviour
{
    // Animator 參數的字串預先轉成 hash，效能較好也避免打錯字
    static readonly int SpeedHash = Animator.StringToHash("Speed");
    static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    static readonly int VerticalVelocityHash = Animator.StringToHash("VerticalVelocity");

    [Tooltip("地面偵測點，與 PlayerMovement 共用同一個")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    Animator animator;
    Rigidbody2D rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 水平速度絕對值 → Speed（決定 Idle/Run）
        animator.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));

        // 垂直速度 → 決定 Jump / Fall
        animator.SetFloat(VerticalVelocityHash, rb.linearVelocity.y);

        // 著地判斷
        bool grounded = Physics2D.OverlapCircle(
            groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool(IsGroundedHash, grounded);
    }
}