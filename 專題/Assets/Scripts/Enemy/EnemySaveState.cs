using UnityEngine;

/// <summary>
/// 敵人存檔狀態。用唯一 ID 記錄是否已被擊敗，
/// 讀檔時已擊敗的敵人不再出現。
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemySaveState : MonoBehaviour, ISaveable
{
    [Tooltip("此敵人的唯一識別碼，場景中不可重複")]
    [SerializeField] string enemyId;

    EnemyHealth health;

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        health.OnDeath += OnEnemyDeath;
    }

    void OnDestroy()
    {
        if (health != null) health.OnDeath -= OnEnemyDeath;
    }

    void OnEnemyDeath()
    {
        // 死亡時記錄到全域擊敗清單（暫存於 DefeatedEnemyTracker）
        DefeatedEnemyTracker.MarkDefeated(enemyId);
    }

    public void SaveState(SaveData data)
    {
        // 由 Tracker 統一寫入，避免每個敵人各寫一次造成重複
        if (DefeatedEnemyTracker.IsDefeated(enemyId)
            && !data.defeatedEnemies.Contains(enemyId))
        {
            data.defeatedEnemies.Add(enemyId);
        }
    }

    public void LoadState(SaveData data)
    {
        // 讀檔時，若此敵人已被擊敗過，直接移除
        if (data.defeatedEnemies.Contains(enemyId))
        {
            DefeatedEnemyTracker.MarkDefeated(enemyId);
            Destroy(gameObject);
        }
    }

    // 方便在編輯器自動產生 ID
    void OnValidate()
    {
        if (string.IsNullOrEmpty(enemyId))
            enemyId = $"{gameObject.scene.name}_{gameObject.name}_{transform.position.x:F0}_{transform.position.y:F0}";
    }
}