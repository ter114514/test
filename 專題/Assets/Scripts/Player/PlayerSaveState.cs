using UnityEngine;

/// <summary>
/// 玩家場景狀態存檔。
/// 負責記錄與還原玩家在世界中的位置與面向。
/// 生命值由 PlayerHealthSystem 自行存檔，屬性由 PlayerStats 自行存檔，
/// 各系統只管自己的資料，互不干涉。
/// </summary>
public class PlayerSaveState : MonoBehaviour, ISaveable
{
    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void SaveState(SaveData data)
    {
        data.playerPosition = transform.position;
        data.currentSceneName = UnityEngine.SceneManagement
            .SceneManager.GetActiveScene().name;
    }

    public void LoadState(SaveData data)
    {
        // 還原位置前先歸零速度，避免帶著舊的動量繼續移動
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        transform.position = data.playerPosition;
    }
}