using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("攻擊設定")]
    public int damage = 1;          // 攻擊力
    public float attackCooldown = 1.5f; // 每幾秒打一次主角
    private float attackTimer;

    void Update()
    {
        // 計時器持續倒數
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    // 碰著主角時（適用於有勾選 Is Trigger 的 Collider）
    // 如果你們的物理是用硬碰撞，請把 OnTriggerStay2D 改成 OnCollisionStay2D
    private void OnTriggerStay2D(Collider2D collision)
    {
        // 檢查碰到的物件是不是主角（主角的 Tag 必須設為 "Player"）
        if (collision.CompareTag("Player"))
        {
            // 如果冷卻時間到了，就發動攻擊
            if (attackTimer <= 0)
            {
                AttackPlayer(collision.gameObject);
                attackTimer = attackCooldown; // 重設冷卻計時器
            }
        }
    }

    void AttackPlayer(GameObject playerObj)
    {
        Debug.Log("史萊姆咬了主角一口！");

        // 如果你有做攻擊動畫，可以在這裡觸發
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("isAttacking");

        // 【對接提示】這裡去呼叫你主角身上的受傷邏輯，例如：
        // PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
        // if (playerHealth != null) playerHealth.TakeDamage(damage);
    }
}