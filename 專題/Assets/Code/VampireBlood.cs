using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VampireBlood : MonoBehaviour
{
    [Header("--- 🩸 吸血鬼血液存量設定 ---")]
    public float maxBlood = 100f;
    public float startingBlood = 50f;
    public float currentBlood;

    [Tooltip("每秒自然流失的血液量 (設為 0 關閉自然扣血)")]
    public float bloodDecayRate = 0f;

    [Header("--- 🩸 主動吸血技能設定 ---")]
    public KeyCode drainKey = KeyCode.R;
    public float drainCooldown = 2.0f;
    private float nextDrainTime = 0f;
    
    public Transform attackPoint;
    public float drainRange = 2.5f;
    public int drainDamageToEnemy = 1;
    public LayerMask enemyLayer;

    [Header("--- ⚠️ 血液狀態機制參數 ---")]
    [Tooltip("血量 0% 時每秒扣除的生命值 (DOT)")]
    public float zeroBloodDotDamage = 5f; 
    
    [Tooltip("100% 爆走狀態下的攻擊力加成倍率 (例：1.5 代表 150% 傷害)")]
    public float berserkDamageMultiplier = 1.5f;

    // 當前血液狀態紀錄
    public enum BloodState { Low, Normal, High }
    [Header("--- 📊 當前狀態顯示 ---")]
    public BloodState currentState = BloodState.Normal;
    private BloodState lastState = BloodState.Normal; // 紀錄上一次狀態，避免 Update 觸發重複刷新

    [Header("--- 📊 UI 與動畫 ---")]
    public Slider bloodSlider;
    public TextMeshProUGUI bloodText;
    private Animator anim;
    private PlayerHealth playerHealth; // 連動玩家 HP 系統以執行 0% DOT 扣血
    private float dotTimer = 0f;       // 0% 扣血 1 秒計時器

    void Start()
    {
        currentBlood = Mathf.Clamp(startingBlood, 0, maxBlood);
        anim = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
        UpdateBloodUI();
    }

    void Update()
    {
        // 1. 自然流失機制
        if (bloodDecayRate > 0 && currentBlood > 0)
        {
            currentBlood -= bloodDecayRate * Time.deltaTime;
            if (currentBlood < 0) currentBlood = 0;
        }

        // 2. 檢測並更新血液狀態
        EvaluateBloodState();

        // 3. 0% 乾涸狀態下的每秒扣血 (DOT)
        if (currentState == BloodState.Low && currentBlood <= 0.01f)
        {
            ApplyZeroBloodDotDamage();
        }

        // 4. 主動技能按鍵
        if (Input.GetKeyDown(drainKey))
        {
            TryCastDrainBlood();
        }

        UpdateBloodUI();
    }

    /// <summary>
    /// 評估血液百分比並觸發對應效果
    /// </summary>
    private void EvaluateBloodState()
    {
        float ratio = currentBlood / maxBlood;

        // 【值 100% / 高血量區間】：爆走 / 攻擊力提升 / 防禦下降
        if (ratio >= 0.99f)
        {
            currentState = BloodState.High;
        }
        // 【值 0% / 乾涸狀態】：極度虛弱 / 持續扣血
        else if (ratio <= 0.01f)
        {
            currentState = BloodState.Low;
        }
        // 【值 50% 附近 / 平衡狀態】：無加成無懲罰
        else
        {
            currentState = BloodState.Normal;
        }

        // 狀態改變時才觸發實質數值切換
        if (currentState != lastState)
        {
            OnStateChanged(currentState);
            lastState = currentState;
        }
    }

    private void OnStateChanged(BloodState newState)
    {
        switch (newState)
        {
            case BloodState.High:
                ApplyBerserkBuff();
                break;
            case BloodState.Low:
                ApplyZeroBloodPenalty();
                break;
            case BloodState.Normal:
                ResetBloodStateBuffs();
                break;
        }
    }

    private void ApplyBerserkBuff()
    {
        Debug.LogWarning("🔥🩸【進入爆走狀態！】攻擊力大幅提升 (x" + berserkDamageMultiplier + ")！");
    }

    private void ApplyZeroBloodPenalty()
    {
        Debug.LogError("💀🩸【血液乾涸！】進入極度虛弱狀態，開始每秒扣血！");
        dotTimer = 0f; // 進入 0% 時重置計時器，準備開始扣血
    }

    private void ApplyZeroBloodDotDamage()
    {
        if (playerHealth != null)
        {
            dotTimer += Time.deltaTime;

            // 每累積滿 1 秒扣一次血
            if (dotTimer >= 1.0f)
            {
                // 💡 呼叫 TakeEnvironmentDamage，防止進入無敵時間與閃爍！
                playerHealth.TakeEnvironmentDamage(Mathf.RoundToInt(zeroBloodDotDamage));
                dotTimer = 0f; // 重置計時器
            }
        }
    }

    private void ResetBloodStateBuffs()
    {
        Debug.Log("⚖️🩸【恢復平衡狀態】無額外加成或懲罰。");
        dotTimer = 0f;
    }

    // 取得當前傷害加成倍率 (供 Player 攻擊時呼叫)
    public float GetCurrentDamageMultiplier()
    {
        if (currentState == BloodState.High)
        {
            return berserkDamageMultiplier;
        }
        return 1.0f; // 正常倍率
    }

    // ----------------------------------------------------
    // 🩸【新增功能】百分比血液檢測與消耗 API (供劍氣/技能呼叫)
    // ----------------------------------------------------

    /// <summary>
    /// 檢查當前血液是否足夠支付指定百分比 (例如：0.1f 代表 10%)
    /// </summary>
    public bool HasEnoughBloodPercent(float percent)
    {
        float requiredBlood = maxBlood * percent;
        return currentBlood >= requiredBlood;
    }

    /// <summary>
    /// 嘗試消耗指定百分比的血液 (例如：0.1f 代表 10%)。若成功扣除回傳 true，血液不足回傳 false。
    /// </summary>
    public bool ConsumeBloodPercent(float percent)
    {
        float cost = maxBlood * percent;

        if (currentBlood >= cost)
        {
            currentBlood -= cost;
            currentBlood = Mathf.Clamp(currentBlood, 0f, maxBlood);
            
            EvaluateBloodState();
            UpdateBloodUI();
            
            Debug.Log($"🩸【技能消耗】成功消耗 {percent * 100}% 血液 ({cost} 點)，剩餘血液：{currentBlood}");
            return true;
        }

        Debug.LogWarning($"❌【血液不足】釋放技能需要 {percent * 100}% 血液 ({cost} 點)，當前僅有 {currentBlood} 點！");
        return false;
    }

    // ----------------------------------------------------

    public void TryCastDrainBlood()
    {
        if (Time.time < nextDrainTime)
        {
            Debug.LogWarning($"⏳ 吸血技能冷卻中！剩餘 {(nextDrainTime - Time.time):F1} 秒");
            return;
        }

        CastDrainBlood();
    }

    private void CastDrainBlood()
    {
        nextDrainTime = Time.time + drainCooldown;

        if (anim != null) anim.SetTrigger("isDraining");

        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector3 origin = (attackPoint != null) ? attackPoint.position : transform.position;

        RaycastHit2D hit = Physics2D.Raycast(origin, attackDirection, drainRange, enemyLayer);

        if (hit.collider != null)
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // 吸血固定獲得 2.5% 最大血量
                float bloodToGain = maxBlood * 0.025f;
                float hpPercent = enemy.currentHealth / enemy.maxHealth;

                if (enemy.isStunned && hpPercent <= 0.25f)
                {
                    enemy.TakeDamage(enemy.currentHealth);
                    AddBlood(bloodToGain);
                    Debug.LogWarning($"🩸💀【處決成功！】絕殺 {hit.collider.name}，獲得 {bloodToGain} 點血液 (2.5%)！");
                }
                else
                {
                    enemy.TakeDamage(drainDamageToEnemy);
                    AddBlood(bloodToGain);
                    Debug.LogWarning($"🩸⚡【吸血成功！】榨取了 {hit.collider.name}，獲得 {bloodToGain} 點血液 (2.5%)！");
                }
                return;
            }
        }

        Debug.Log("🦇【吸血揮空】前方沒有可吸血的目標！");
    }

    public void DrainBloodOnParry()
    {
        float bloodToGain = maxBlood * 0.025f;
        AddBlood(bloodToGain);
        Debug.LogWarning($"⚔️🩸【彈反吸血！】獲得 {bloodToGain} 點血液 (2.5%)！");
    }

    public void AddBlood(float amount)
    {
        currentBlood += amount;
        if (currentBlood > maxBlood) currentBlood = maxBlood;
        UpdateBloodUI();
    }

    public void SetBlood(float targetValue)
    {
        currentBlood = Mathf.Clamp(targetValue, 0f, maxBlood);
        UpdateBloodUI();
    }

    public bool ConsumeBlood(float amount)
    {
        if (currentBlood >= amount)
        {
            currentBlood -= amount;
            UpdateBloodUI();
            return true;
        }
        return false;
    }

    public void UpdateBloodUI()
    {
        if (bloodSlider != null)
        {
            bloodSlider.maxValue = maxBlood;
            bloodSlider.value = currentBlood;

            if (bloodSlider.fillRect != null)
            {
                bloodSlider.fillRect.gameObject.SetActive(currentBlood > 0);
            }
        }

        if (bloodText != null)
        {
            bloodText.text = $"{Mathf.RoundToInt(currentBlood)} / {Mathf.RoundToInt(maxBlood)}";
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = (attackPoint != null) ? attackPoint.position : transform.position;
        Vector3 dir = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawRay(origin, dir * drainRange);
    }
}