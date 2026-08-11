using UnityEngine;

public class DoubleJump : MonoBehaviour
{
    [Header("--- 🦘 二段跳基礎設定 ---")]
    public float doubleJumpForce = 12f;         // 二段跳的向上推動力道
    public KeyCode jumpKey = KeyCode.Space;      // 跳躍按鍵 (預設 Space)

    [Header("--- 💨 魔能氣流傷害設定 ---")]
    public int updraftDamage = 5;                // 下方氣流造成的微量傷害
    public Vector2 attackBoxSize = new Vector2(1.5f, 0.8f); // 腳底氣流傷害區域大小
    public Vector2 attackBoxOffset = new Vector2(0f, -0.6f); // 氣流相對玩家腳底的位置偏移
    public LayerMask enemyLayer;                 // 敵人 Layer
    public LayerMask groundLayer;                // 地面 Layer (用於檢測是否著地)

    [Header("--- ✨ 特效與動畫 (選填) ---")]
    public GameObject updraftVFXPrefab;          // 腳底噴發氣流的視覺特效 Prefab

    // 內部狀態變數
    private bool canDoubleJump = false;          // 是否還可以進行二段跳
    private bool isGrounded = false;             // 當前是否在地面上
    public Transform groundCheck;                // 地面檢測點
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 檢測是否踩在地面上
        CheckGrounded();

        // 2. 當落回地面時，重置二段跳次數
        if (isGrounded)
        {
            canDoubleJump = true;
        }

        // 3. 按下跳躍鍵處理
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                // 第一段跳躍：由原本的 PlayerController 控制，或在這裡執行一段跳
                // 如果你的 PlayerController 已經處理了一段跳，這裡什麼都不用做，直接讓 Update 繼續倒數即可
            }
            else if (canDoubleJump)
            {
                // 在空中且還有二段跳次數：發動魔能二段跳！
                ExecuteDoubleJump();
            }
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }

    /// <summary>
    /// 執行魔能二段跳與腳底氣流傷害
    /// </summary>
    private void ExecuteDoubleJump()
    {
        canDoubleJump = false; // 消耗掉二段跳次數

        // 1. 重置 Y 軸速度，確保無論當時是在下墜還是在上升，二段跳的高度都保持一致
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);

        // 2. 播放二段跳動畫/觸發器
        if (anim != null)
        {
            anim.SetTrigger("doubleJump"); // 或 ResetTrigger("jump") 後再 SetTrigger
        }

        // 3. 生成腳底氣流特效
        Vector3 spawnPos = (groundCheck != null) ? groundCheck.position : transform.position + (Vector3)attackBoxOffset;
        if (updraftVFXPrefab != null)
        {
            Instantiate(updraftVFXPrefab, spawnPos, Quaternion.identity);
        }

        // 4. 💥【核心機制】對腳下敵人造成微量氣流傷害
        ApplyUpdraftDamage(spawnPos);

        Debug.Log("🦘💨【魔能二段跳】踩踏魔能氣流再次跳躍，並對下方敵人造成衝擊！");
    }

    /// <summary>
    /// 檢測並對腳底範圍內的敵人造成傷害與下壓擊退
    /// </summary>
    private void ApplyUpdraftDamage(Vector3 center)
    {
        // 使用 OverlapBox 檢測玩家腳下的矩形範圍
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(center, attackBoxSize, 0f, enemyLayer);

        foreach (var enemyCollider in hitEnemies)
        {
            EnemyHealth enemy = enemyCollider.GetComponentInParent<EnemyHealth>();
            EnemyAI enemyAI = enemyCollider.GetComponentInParent<EnemyAI>();

            // 造成微量傷害
            if (enemy != null)
            {
                enemy.TakeDamage(updraftDamage);
            }

            // 給予下方敵人向下的氣流壓迫擊退 (向下壓/微幅震頓)
            if (enemyAI != null)
            {
                Vector2 pushDownForce = new Vector2(0f, -8f); // 向下壓的力道
                enemyAI.ApplyKnockback(pushDownForce, 0.15f);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 在 Scene 視窗繪製腳底氣流傷害判定範圍
        Gizmos.color = Color.cyan;
        Vector3 center = (groundCheck != null) ? groundCheck.position : transform.position + (Vector3)attackBoxOffset;
        Gizmos.DrawWireCube(center, attackBoxSize);

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}