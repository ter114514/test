using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("移動與跳躍設定")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("地面偵測")]
    public Transform groundCheck;
    public float checkRadius = 0.25f;  // 稍微加大檢測半徑，避免踩在邊緣卡住
    public LayerMask groundLayer;
    public bool isGrounded;            // 設為 public 方便在 Inspector 觀察勾選狀態

    [Header("攻擊判定設定")]
    public Transform attackPoint;      // 刀尖/攻擊中心位置
    public float attackRange = 0.6f;   // 攻擊範圍半徑
    public LayerMask enemyLayers;      // 怪物 Layer (請選擇 Enemy)
    public int attackDamage = 1;       // 傷害值

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerHealth playerHealth; // 讀取眩暈/破防狀態
    
    private float horizontalInput;
    private Vector3 originalScale;     // 記錄初始 Scale 防止翻轉變形
    private bool jumpRequested = false;// 跳躍請求標記 (防止 Update 鍵盤輸入被 FixedUpdate 漏掉)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        
        // 記錄角色 Inspector 上的原始 Scale
        originalScale = transform.localScale;
    }

    void Update()
    {
        // 1.【眩暈攔截】：破防/眩暈時，切斷所有輸入與動畫，歸零水平移動
        if (playerHealth != null && playerHealth.isStunned)
        {
            horizontalInput = 0;
            if (anim != null) 
            {
                anim.SetBool("isRunning", false);
            }
            return; // 阻斷跳躍、攻擊與轉向
        }

        // 2. 地面偵測 (加入防呆，沒拉 Transform 時預設用角色腳底)
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        else
        {
            isGrounded = Physics2D.OverlapCircle(transform.position + Vector3.down * 0.5f, checkRadius, groundLayer);
        }

        // 3. 移動輸入
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 4. 跳躍按鍵偵測 (發送請求給 FixedUpdate 執行物理衝量)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
            if (anim != null) anim.SetTrigger("jump");
        }

        // 5. 攻擊輸入 (滑鼠左鍵 或 Fire1)
        if (Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0))
        {
            Attack();
        }

        // 6. 更新 Animator 狀態參數
        if (anim != null)
        {
            anim.SetBool("isRunning", horizontalInput != 0);
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", rb.linearVelocity.y);
        }

        // 7. 轉向邏輯 (使用原始 Scale 比例翻轉)
        Flip();
    }

    void FixedUpdate()
    {
        // 眩暈時阻止物理滑行
        if (playerHealth != null && playerHealth.isStunned)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 執行跳躍衝量
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpRequested = false; // 執行完畢立即清除標記
        }

        // 水平移動物理更新
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // 核心攻擊與傷害檢測功能
    private void Attack()
    {
        // 重置跳躍 Trigger，避免觸發切換 Bug
        if (anim != null)
        {
            anim.ResetTrigger("jump");
            anim.SetTrigger("attack");
        }

        // 判定範圍內的所有敵人並扣血
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    // 轉向處理 (保留比例)
    private void Flip()
    {
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    // 在 Scene 視窗繪製偵測範圍
    private void OnDrawGizmosSelected()
    {
        // 綠色：地面偵測範圍
        Vector3 checkPos = (groundCheck != null) ? groundCheck.position : transform.position + Vector3.down * 0.5f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(checkPos, checkRadius);

        // 紅色：攻擊範圍
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}