using UnityEngine;

public class SwordWaveProjectile : MonoBehaviour
{
    [Header("--- 🗡️ 劍氣基礎參數 ---")]
    public float speed = 12f;                 // 劍氣飛行速度
    public float lifetime = 1.5f;             // 存在時間 (秒)
    public int damage = 20;                   // 基礎傷害
    public float knockbackForce = 25f;        // 擊退速度 (建議 15~25 之間打擊感最佳)
    public float stunDuration = 0.25f;        // 擊退硬直時間

    [Header("--- 🎯 判定設定 ---")]
    public LayerMask enemyLayer;
    public LayerMask groundLayer;            // 撞到牆壁/地面銷毀

    private Vector2 flyDirection;

    public void Setup(Vector2 direction, int finalDamage)
    {
        flyDirection = direction.normalized;
        damage = finalDamage;

        // 旋轉預製體以匹配飛行的方向
        float angle = Mathf.Atan2(flyDirection.y, flyDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 設置 lifetime 秒後自動銷毀
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 保持勻速直線飛行
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 防誤判：忽略玩家自身與其他 Trigger 觸發區域
        if (other.CompareTag("Player") || other.isTrigger)
        {
            return;
        }

        // 2. 擊中敵人判定
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // 優先從本體或父物件取得 EnemyHealth、EnemyAI 與 Rigidbody2D
            EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
            EnemyAI enemyAI = other.GetComponentInParent<EnemyAI>();
            Rigidbody2D enemyRb = other.GetComponentInParent<Rigidbody2D>();

            // 扣血處理
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 💥【核心修正】優先呼叫 EnemyAI 的 ApplyKnockback 介面！
            if (enemyAI != null)
            {
                // 計算擊退向量 (水平擊退 + 微幅向上拋)
                Vector2 knockbackVector = (flyDirection + Vector2.up * 0.2f).normalized * knockbackForce;
                
                // 呼叫 EnemyAI 鎖定 AI 並給予擊退力道/速度
                enemyAI.ApplyKnockback(knockbackVector, stunDuration);
                
                Debug.Log($"🗡️【劍氣命中】對 {other.name} 造成 {damage} 點傷害並成功發動擊退！力道：{knockbackVector}");
            }
            // 保險備用：若目標沒有 EnemyAI 腳本 (例如普通障礙物)，直接施加 AddForce
            else if (enemyRb != null)
            {
                enemyRb.linearVelocity = new Vector2(0f, enemyRb.linearVelocity.y);
                Vector2 force = (flyDirection + Vector2.up * 0.2f).normalized * knockbackForce;
                enemyRb.AddForce(force, ForceMode2D.Impulse);
            }

            // 💡 如果你希望劍氣「貫穿」敵人繼續飛，請保持原樣；
            // 💡 如果希望「撞到第一個敵人就消失」，請取消下方註解：
            // Destroy(gameObject);
            return;
        }

        // 3. 撞到牆壁/地面銷毀
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            Debug.Log($"🧱 劍氣撞擊牆壁/地面 [{other.name}] 銷毀");
            Destroy(gameObject);
        }
    }
}