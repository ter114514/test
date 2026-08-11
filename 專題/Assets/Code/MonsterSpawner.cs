using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("--- 召喚設定 ---")]
    public GameObject monsterPrefab; // 拖入史萊姆 Prefab
    public Transform spawnPoint;     // 拖入生成位置 (Transform)
    
    [Header("--- 按鍵與冷卻設定 ---")]
    [Tooltip("召喚怪物的觸發按鍵，預設為 E")]
    public KeyCode spawnKey = KeyCode.E; 

    [Tooltip("召喚冷卻時間（秒），防止連按生成太多隻")]
    public float spawnCooldown = 1.0f; 

    [Header("--- 玩家參考 ---")]
    public Transform playerTransform; // 可手動拖入玩家，或留空自動尋找

    private bool isPlayerInRange = false; // 玩家是否在觸發區域內
    private float nextSpawnTime = 0f;     // 下一次可以召喚的時間

    private void Start()
    {
        // 自動尋找 Tag 為 Player 的物件
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        // 當玩家在範圍內、按下指定按鍵（E 鍵）、且冷卻時間已過
        if (isPlayerInRange && Input.GetKeyDown(spawnKey) && Time.time >= nextSpawnTime)
        {
            SpawnMonster();
            nextSpawnTime = Time.time + spawnCooldown; // 設定下一次可召喚的時間
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log($"【提示】進入召喚區域，按 [{spawnKey}] 鍵可多次召喚怪物！");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefab != null && spawnPoint != null)
        {
            // 生成史萊姆
            GameObject newMonster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // 強制將玩家 Transform 帶給新生成的史萊姆 AI
            EnemyAI ai = newMonster.GetComponent<EnemyAI>();
            if (ai != null)
            {
                if (playerTransform != null)
                {
                    ai.player = playerTransform;
                }
                ai.InitPlayer(); // 觸發初始化
            }

            Debug.Log("【召喚成功】成功生成史萊姆並綁定玩家！");
        }
        else
        {
            Debug.LogError("【召喚失敗】MonsterSpawner 上的 Monster Prefab 或 Spawn Point 未設定！");
        }
    }
}