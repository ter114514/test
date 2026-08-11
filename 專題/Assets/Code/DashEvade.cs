using UnityEngine;
using System.Collections;

public class TeleportEvade : MonoBehaviour
{
    [Header("--- ⚡ 順移閃避設定 ---")]
    public KeyCode teleportKey = KeyCode.LeftShift; // 順移按鍵 (預設 Shift)
    public float teleportDistance = 5f;             // 瞬移距離 (格/單位)
    public float cooldown = 1.2f;                    // 冷卻時間 (秒)
    public float invincibilityTime = 0.3f;          // 順移後的無敵時間 (秒)
    public LayerMask groundLayer;                   // 地面/牆壁 Layer (防止順移進牆壁)

    private float nextTeleportTime = 0f;
    private PlayerHealth playerHealth;
    private Animator anim;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teleportKey) && Time.time >= nextTeleportTime)
        {
            ExecuteTeleport();
        }
    }

    private void ExecuteTeleport()
    {
        nextTeleportTime = Time.time + cooldown;

        // 1. 計算方向 (玩家目前面對的方向)
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        Vector2 direction = new Vector2(facingDir, 0f);

        // 2. 牆壁檢測：從玩家位置往前發射射線，確認前方有沒有牆壁阻擋
        float actualDistance = teleportDistance;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, teleportDistance, groundLayer);

        if (hit.collider != null)
        {
            // 如果前方有牆壁，瞬移距離縮短至牆壁前 (保留 0.3 格安全距離)
            actualDistance = Mathf.Max(0f, hit.distance - 0.3f);
            Debug.Log($"🧱 前方有牆壁 [{hit.collider.name}]，縮短順移距離至: {actualDistance}");
        }

        // 3. 💥 瞬間移動座標
        transform.position += new Vector3(direction.x * actualDistance, 0f, 0f);

        // 4. 觸發無敵時間與動畫
        if (playerHealth != null)
        {
            playerHealth.TriggerInvincibility(invincibilityTime);
        }

        if (anim != null)
        {
            anim.SetTrigger("teleport"); // 可以在 Animator 裡設定殘影或閃爍動畫
        }

        Debug.Log($"⚡【瞬間移動】向前方順移了 {actualDistance} 格！");
    }
}