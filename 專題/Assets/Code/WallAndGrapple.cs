using UnityEngine;
using System.Collections;

public class WallAndGrapple : MonoBehaviour
{
    [Header("--- 🧱 牆面攀爬與彈跳設定 ---")]
    public float wallClimbSpeed = 5f;            // 向上爬牆速度
    public float wallSlideSpeed = 2f;            // 貼牆下滑速度
    public Vector2 wallJumpForce = new Vector2(10f, 14f); // 牆面彈跳力道 (X:彈開力, Y:向上力)
    public Transform wallCheck;                  // 牆面檢測點
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;                  // 牆面 Layer

    [Header("--- 🕸️ 圓形黑影抓鉤設定 ---")]
    public KeyCode grappleKey = KeyCode.E;       // 抓鉤按鍵 (預設 E)
    public float grappleSearchRadius = 8f;       // 以玩家為中心搜尋機關的「圓形半徑」
    public float pullSpeed = 22f;                // 將自己拉過去的速度
    public LayerMask grappleMechanismLayer;      // 特殊機關的 Layer

    [Header("--- ✨ 特效與渲染 (選填) ---")]
    public LineRenderer grappleLine;             // 抓鉤黑影線條

    // 內部狀態
    private bool isTouchingWall = false;
    private bool isWallClimbing = false;
    private bool isGrappling = false;
    private Rigidbody2D rb;
    private Animator anim;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            originalGravity = rb.gravityScale;
        }

        if (grappleLine != null)
        {
            grappleLine.enabled = false;
        }
    }

    void Update()
    {
        // 抓鉤中停用普通移動與爬牆
        if (isGrappling) return;

        // 1. 檢測是否貼牆
        CheckWallStatus();

        // 2. 處理牆面攀爬與下滑
        HandleWallClimb();

        // 3. 處理牆面彈跳 (Wall Jump)
        if (isTouchingWall && Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteWallJump();
        }

        // 4. 按下抓鉤鍵發射黑影
        if (Input.GetKeyDown(grappleKey))
        {
            TryCastShadowGrapple();
        }
    }

    private void CheckWallStatus()
    {
        if (wallCheck != null)
        {
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        }
    }

    private void HandleWallClimb()
    {
        if (isTouchingWall)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");

            if (verticalInput > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallClimbSpeed);
                isWallClimbing = true;
            }
            else if (verticalInput < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallClimbSpeed);
                isWallClimbing = true;
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
                isWallClimbing = false;
            }

            SetAnimBoolIfExists("isWallClimbing", true);
        }
        else
        {
            isWallClimbing = false;
            SetAnimBoolIfExists("isWallClimbing", false);
        }
    }

    private void ExecuteWallJump()
    {
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 jumpDirection = new Vector2(-facingDir * wallJumpForce.x, wallJumpForce.y);

        rb.linearVelocity = jumpDirection;

        // 反轉角色面向
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        if (anim != null) anim.SetTrigger("wallJump");

        Debug.Log("🧗‍♀️【牆面彈跳！】反向彈開並向上躍起！");
    }

    /// <summary>
    /// 💥【全方位圓形鎖定】搜尋以玩家為中心半徑內的最近機關
    /// </summary>
    private void TryCastShadowGrapple()
    {
        // 1. 使用 OverlapCircleAll 抓取圓形範圍內所有標有 grappleMechanismLayer 的 Collider
        Collider2D[] mechanisms = Physics2D.OverlapCircleAll(transform.position, grappleSearchRadius, grappleMechanismLayer);

        if (mechanisms.Length == 0)
        {
            Debug.Log($"🕸️【黑影抓鉤】周圍 {grappleSearchRadius} 格圓形範圍內沒有可抓取的機關！");
            return;
        }

        // 2. 尋找距離玩家「最近」的那個機關
        Transform closestMechanism = null;
        float minDistance = float.MaxValue;

        foreach (var mechanism in mechanisms)
        {
            float dist = Vector2.Distance(transform.position, mechanism.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestMechanism = mechanism.transform;
            }
        }

        // 3. 發射黑影抓鉤並拉過去
        if (closestMechanism != null)
        {
            StartCoroutine(GrappleRoutine(closestMechanism.position));
        }
    }

    private IEnumerator GrappleRoutine(Vector2 targetPoint)
    {
        isGrappling = true;
        rb.gravityScale = 0f; // 抓拉過程中無視重力

        // 💥【防遮擋關鍵】紀錄玩家原本的 Z 軸深度，避免移動過去後 Z 軸跑掉變到機關/背景後面
        float originalZ = transform.position.z;

        // 1. 轉向面向機關 (讓少女拉過去時視覺朝向機關)
        float dirX = targetPoint.x - transform.position.x;
        if ((dirX > 0 && transform.localScale.x < 0) || (dirX < 0 && transform.localScale.x > 0))
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }

        // 2. 啟動黑影繪製 (若有 LineRenderer)
        if (grappleLine != null)
        {
            grappleLine.enabled = true;
            grappleLine.SetPosition(0, transform.position);
            grappleLine.SetPosition(1, targetPoint);
        }

        if (anim != null) anim.SetTrigger("grapple");

        // 3. 將少女高速拉向機關目標點
        while (Vector2.Distance(transform.position, targetPoint) > 0.8f)
        {
            // 計算下一幀的 2D 位置
            Vector2 nextPos = Vector2.MoveTowards(transform.position, targetPoint, pullSpeed * Time.deltaTime);
            
            // 💥【防遮擋關鍵】強制將 Z 軸鎖定在 originalZ，防止被圖層覆蓋
            transform.position = new Vector3(nextPos.x, nextPos.y, originalZ);
            
            if (grappleLine != null)
            {
                grappleLine.SetPosition(0, transform.position);
            }

            yield return null;
        }

        // 4. 到達目標附近，結束抓鉤狀態
        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; // 停下衝量

        if (grappleLine != null) grappleLine.enabled = false;

        isGrappling = false;
        Debug.Log("🕸️【影鉤到達】已拉至目標機關點！");
    }

    // 防呆輔助方法：無該動畫參數時避開 Warning
    private void SetAnimBoolIfExists(string paramName, bool value)
    {
        if (anim == null) return;

        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName)
            {
                anim.SetBool(paramName, value);
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }

        // 💥【綠色球體】在 Scene 視窗繪製 360 度圓形搜尋範圍
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, grappleSearchRadius);
    }
}