using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("--- AI 狀態與目標 ---")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("--- 史萊姆移動設定 ---")]
    public float patrolSpeed = 6.0f;
    public float chaseSpeed = 10.0f;
    [Tooltip("圍繞出生點隨機巡邏的左右範圍半徑")]
    public float patrolRange = 15.0f; 

    [Header("--- 💥 邊界與索敵設定 ---")]
    [Tooltip("發現玩家並追擊的距離")]
    public float detectRange = 25.0f; 
    [Tooltip("💥 史萊姆離『出生點』的最遠距離！超過這個距離，就算看得到玩家也強制回頭！")]
    public float maxDistanceFromSpawn = 30.0f; 

    [Header("--- 史萊姆攻擊設定 ---")]
    public int damage = 10;             
    public float attackRate = 1.5f;     
    public float attackRange = 12.0f;    
    public float damageDelay = 1.0f;    

    [Header("--- 橢圓傷害偵測設定 ---")]
    public float attackOffset = 15.0f;
    public float ellipseRadiusX = 21.0f;
    public float ellipseRadiusY = 20.0f;

    [Header("--- 💥 受擊擊退設定 ---")]
    private bool isKnockedBack = false; // 是否正處於受擊擊退硬直中

    [Header("--- 手動拖入主角物件 ---")]
    public Transform player;            

    private Animator anim;              
    private Rigidbody2D rb;             
    private float nextAttackTime = 0f;  
    private PlayerHealth playerHealth;  
    private EnemyHealth enemyHealth;    // 對接怪物的血量與眩暈狀態

    private Vector3 originalScale;      
    private Vector2 spawnPoint;         // 固定的出生點位置
    private Vector2 patrolTarget;       
    private bool isAttacking = false;   

    void Start()
    {
        anim = GetComponent<Animator>(); //[cite: 1]
        rb = GetComponent<Rigidbody2D>(); //[cite: 1]
        enemyHealth = GetComponent<EnemyHealth>(); // 自動取得怪物的 Health 元件[cite: 1]

        if (rb == null) //[cite: 1]
        {
            Debug.LogError("【錯誤】史萊姆身上的 Rigidbody2D 遺失了！"); //[cite: 1]
        }

        originalScale = transform.localScale; //[cite: 1]
        spawnPoint = transform.position; // 記錄生成的座標[cite: 1]
        SetNewPatrolTarget(); //[cite: 1]

        InitPlayer(); //[cite: 1]
    }

    public void InitPlayer()
    {
        if (player != null && !player.CompareTag("Player")) //[cite: 1]
        {
            Debug.LogWarning($"【警告】{name} 的 Player 欄位拖到了非 Player 物件 ({player.name})，已自動重置！"); //[cite: 1]
            player = null; //[cite: 1]
        }

        if (player == null) //[cite: 1]
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); //[cite: 1]
            if (playerObj != null)  //[cite: 1]
            {
                player = playerObj.transform; //[cite: 1]
                playerHealth = playerObj.GetComponent<PlayerHealth>(); //[cite: 1]
            }
        }
        else if (playerHealth == null) //[cite: 1]
        {
            playerHealth = player.GetComponent<PlayerHealth>(); //[cite: 1]
        }
    }

    void Update()
    {
        // 💥 如果正在被擊退，暫停所有 AI 狀態切換
        if (isKnockedBack) return;

        // 💥 如果怪物陷入破防眩暈狀態，完全凍結所有 AI 決策！[cite: 1]
        if (enemyHealth != null && enemyHealth.isStunned) //[cite: 1]
        {
            isAttacking = false; //[cite: 1]
            return; //[cite: 1]
        }

        if (player == null)  //[cite: 1]
        {
            InitPlayer();  //[cite: 1]
            return; //[cite: 1]
        }

        if (!isAttacking) //[cite: 1]
        {
            DecideState(); //[cite: 1]
        }
    }

    void FixedUpdate()
    {
        // 💥【核心修正】若處於擊退狀態中，直接 return，讓 AddForce 的物理力道完整發揮，不干涉速度！
        if (isKnockedBack) return;

        // 💥 如果怪物正在眩暈、無玩家或正在攻擊，強制停下物理速度[cite: 1]
        if ((enemyHealth != null && enemyHealth.isStunned) || player == null || isAttacking)  //[cite: 1]
        {
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);  //[cite: 1]
            return; //[cite: 1]
        }

        switch (currentState) //[cite: 1]
        {
            case EnemyState.Patrol: //[cite: 1]
                MoveTowards(patrolTarget, patrolSpeed); //[cite: 1]
                // 抵達巡邏目標點後，在原生出生點周圍重新選一個點[cite: 1]
                if (Mathf.Abs(transform.position.x - patrolTarget.x) < 1.5f) //[cite: 1]
                {
                    SetNewPatrolTarget(); //[cite: 1]
                }
                break;

            case EnemyState.Chase: //[cite: 1]
                MoveTowards(player.position, chaseSpeed); //[cite: 1]
                break;
        }
    }

    // 💥【供遠程武器/劍氣呼叫的擊退介面】
    public void ApplyKnockback(Vector2 force, float duration = 0.25f)
    {
        if (rb != null)
        {
            StopCoroutine(KnockbackRoutine(force, duration));
            StartCoroutine(KnockbackRoutine(force, duration));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isKnockedBack = true;
        isAttacking = false;

        // 1. 觸發動畫（如果 Animator 裡面有受擊動畫）
        if (anim != null)
        {
            anim.SetTrigger("Hit"); // 或 "TakeDamage"
        }

        // 2. 給予瞬間衝量速度
        rb.linearVelocity = force;

        // 3. 讓擊退過程速度自然衰減，而不是一直等速滑行
        float elapsed = 0f;
        Vector2 initialVelocity = force;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 隨時間將速度滑順減速降至 0
            rb.linearVelocity = Vector2.Lerp(initialVelocity, Vector2.zero, elapsed / duration);
            yield return null;
        }

        isKnockedBack = false;
    }

    // 💥 狀態與邊界判定[cite: 1]
    void DecideState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position); //[cite: 1]
        float distanceToSpawn = Vector2.Distance(transform.position, spawnPoint); // 計算與出生點的距離[cite: 1]

        // 💥【最高優先級】如果史萊姆跑太遠（超過 maxDistanceFromSpawn），不管怎樣立刻放棄追擊！[cite: 1]
        if (distanceToSpawn > maxDistanceFromSpawn) //[cite: 1]
        {
            ForceReturnToSpawnArea(); //[cite: 1]
            return; //[cite: 1]
        }

        // 1. 攻擊範圍判定[cite: 1]
        if (distanceToPlayer <= attackRange) //[cite: 1]
        {
            currentState = EnemyState.Attack; //[cite: 1]
            if (Time.time >= nextAttackTime) //[cite: 1]
            {
                StartCoroutine(AttackRoutine()); //[cite: 1]
                nextAttackTime = Time.time + attackRate; //[cite: 1]
            }
            return; //[cite: 1]
        }

        // 2. 索敵範圍判定（在 detectRange 內才追擊）[cite: 1]
        if (distanceToPlayer <= detectRange) //[cite: 1]
        {
            currentState = EnemyState.Chase; //[cite: 1]
            return; //[cite: 1]
        }

        // 3. 超過 detectRange 時，切回 Patrol[cite: 1]
        if (currentState == EnemyState.Chase || currentState == EnemyState.Attack) //[cite: 1]
        {
            ForceReturnToSpawnArea(); //[cite: 1]
        }
    }

    // 強制煞車並轉為回家巡邏[cite: 1]
    void ForceReturnToSpawnArea()
    {
        if (rb != null) //[cite: 1]
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); //[cite: 1]
        }

        currentState = EnemyState.Patrol; //[cite: 1]
        SetNewPatrolTarget(); // 把目標點重新拉回出生點附近[cite: 1]
    }

    void MoveTowards(Vector2 target, float speed)
    {
        if (isAttacking || isKnockedBack || rb == null) return;

        Vector2 direction = (target - (Vector2)transform.position).normalized; //[cite: 1]
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y); //[cite: 1]

        if (rb.linearVelocity.x > 0.1f) //[cite: 1]
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); //[cite: 1]
        }
        else if (rb.linearVelocity.x < -0.1f) //[cite: 1]
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z); //[cite: 1]
        }
    }

    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(-patrolRange, patrolRange); //[cite: 1]
        patrolTarget = new Vector2(spawnPoint.x + randomX, transform.position.y); //[cite: 1]
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true; //[cite: 1]
        if (rb != null) rb.linearVelocity = Vector2.zero; //[cite: 1]

        if (anim != null) //[cite: 1]
        {
            anim.SetTrigger("isAttacking");  //[cite: 1]
        }

        yield return new WaitForSeconds(damageDelay); //[cite: 1]

        // 如果蓄力前搖期間被打飛，取消傷害[cite: 1]
        if (isKnockedBack || (enemyHealth != null && enemyHealth.isStunned))
        {
            isAttacking = false;
            yield break;
        }

        if (player != null && playerHealth != null) //[cite: 1]
        {
            float facingDirection = transform.localScale.x > 0 ? 1 : -1;  //[cite: 1]
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(facingDirection * attackOffset, 0); //[cite: 1]

            float dx = player.position.x - attackCenter.x; //[cite: 1]
            float dy = player.position.y - attackCenter.y; //[cite: 1]

            float rx2 = ellipseRadiusX * ellipseRadiusX; //[cite: 1]
            float ry2 = ellipseRadiusY * ellipseRadiusY; //[cite: 1]

            if (rx2 > 0 && ry2 > 0) //[cite: 1]
            {
                float value = (dx * dx) / rx2 + (dy * dy) / ry2; //[cite: 1]
                if (value <= 1f) //[cite: 1]
                {
                    playerHealth.TakeDamage(damage, this.transform);  //[cite: 1]
                }
            }
        }

        isAttacking = false; //[cite: 1]
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centerPoint = (spawnPoint != Vector2.zero) ? (Vector3)spawnPoint : transform.position; //[cite: 1]

        Gizmos.color = Color.yellow; //[cite: 1]
        Gizmos.DrawWireSphere(transform.position, attackRange); //[cite: 1]

        Gizmos.color = Color.cyan; //[cite: 1]
        Gizmos.DrawWireSphere(transform.position, detectRange); //[cite: 1]

        Gizmos.color = Color.white; //[cite: 1]
        Gizmos.DrawWireSphere(centerPoint, maxDistanceFromSpawn); //[cite: 1]
        Gizmos.DrawSphere(centerPoint, 0.5f); //[cite: 1]

        Gizmos.color = Color.red; //[cite: 1]
        float facingDirection = transform.localScale.x > 0 ? 1 : -1;  //[cite: 1]
        Vector2 attackCenter = (Vector2)transform.position + new Vector2(facingDirection * attackOffset, 0); //[cite: 1]

        Matrix4x4 oldMatrix = Gizmos.matrix; //[cite: 1]
        Matrix4x4 ellipseMatrix = Matrix4x4.TRS(
            new Vector3(attackCenter.x, attackCenter.y, transform.position.z),
            Quaternion.identity,
            new Vector3(ellipseRadiusX, ellipseRadiusY, 1f)
        ); //[cite: 1]
        
        Gizmos.matrix = ellipseMatrix; //[cite: 1]
        Gizmos.DrawWireSphere(Vector3.zero, 1f); //[cite: 1]
        Gizmos.matrix = oldMatrix; //[cite: 1]
    }
}