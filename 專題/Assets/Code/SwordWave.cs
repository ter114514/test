using UnityEngine;

public class SwordWave : MonoBehaviour
{
    [Header("--- ⚔️ 技能設定 ---")]
    public KeyCode skillKey = KeyCode.Q;        // 按鍵 Q 觸發
    public float cooldown = 4.0f;               // 冷卻時間 (秒)
    private float nextCastTime = 0f;
    public int baseDamage = 20;                 // 基礎傷害

    [Header("--- 🖥️ UI 冷卻設定 ---")]
    [Tooltip("將劍氣技能的 SkillCooldownUI 物件拖到這裡")]
    public SkillCooldownUI cooldownUI;          // 冷卻視覺化 UI 組件

    [Header("--- 🩸 血液消耗設定 ---")]
    [Tooltip("釋放劍氣消耗的血液百分比 (0.1 代表 10%)")]
    public float bloodCostPercent = 0.1f;

    [Header("--- 🗡️ 劍氣生成設定 ---")]
    public GameObject swordWavePrefab;          // 劍氣 Prefab
    public Transform spawnPoint;                // 劍氣發射點 (如武器前端)

    // 組件參考
    private Animator anim;
    private VampireBlood vampireBlood;

    void Start()
    {
        anim = GetComponent<Animator>();
        vampireBlood = GetComponent<VampireBlood>();
    }

    void Update()
    {
        if (Input.GetKeyDown(skillKey))
        {
            TryCastSwordWave();
        }
    }

    public void TryCastSwordWave()
    {
        // 1. 檢查冷卻時間
        if (Time.time < nextCastTime)
        {
            float remaining = nextCastTime - Time.time;
            Debug.LogWarning($"⏳【劍氣】冷卻中！剩餘 {remaining:F1} 秒");
            return;
        }

        // 2. 檢查 Prefab
        if (swordWavePrefab == null)
        {
            Debug.LogError("❌【劍氣】尚未設定 SwordWavePrefab！");
            return;
        }

        // 3. 檢查並扣除 10% 血液 (如果血液不足直接退出)
        if (vampireBlood != null && !vampireBlood.ConsumeBloodPercent(bloodCostPercent))
        {
            return;
        }

        // 4. 條件皆滿足，執行技能釋放
        ExecuteSwordWave();
    }

    private void ExecuteSwordWave()
    {
        nextCastTime = Time.time + cooldown;

        // 💥【新增】觸發 UI 轉圈倒數
        if (cooldownUI != null)
        {
            cooldownUI.StartCooldown(cooldown);
        }

        // 播放揮劍動畫
        if (anim != null) anim.SetTrigger("swordWave");

        // 計算面向方向
        Vector2 facingDir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : transform.position + (Vector3)(facingDir * 0.8f);

        // 計算最終傷害（結合 100% 血液爆走加成）
        int finalDamage = baseDamage;
        if (vampireBlood != null)
        {
            finalDamage = Mathf.RoundToInt(baseDamage * vampireBlood.GetCurrentDamageMultiplier());
        }

        // 生成劍氣預製體
        GameObject waveObj = Instantiate(swordWavePrefab, spawnPos, Quaternion.identity);
        
        // 修正 Sprite 翻轉（如果角色朝左）
        if (facingDir.x < 0)
        {
            Vector3 localScale = waveObj.transform.localScale;
            localScale.x *= -1f;
            waveObj.transform.localScale = localScale;
        }

        // 傳遞參數給 SwordWaveProjectile
        SwordWaveProjectile waveScript = waveObj.GetComponent<SwordWaveProjectile>();
        if (waveScript != null)
        {
            waveScript.Setup(facingDir, finalDamage);
        }

        Debug.Log($"🗡️🔥【血色劍氣】揮出劍氣！傷害: {finalDamage}，方向: {facingDir}");
    }
}