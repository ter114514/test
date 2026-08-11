using UnityEngine;
using UnityEngine.UI; // 控制 UI 血條[cite: 2]
using System.Collections; // 協程閃爍效果[cite: 2]

public class PlayerHealth : MonoBehaviour
{
    [Header("--- Aria 血量數值 ---")]
    [Tooltip("Aria 的最大生命值上限")]
    public int maxHealth = 100; //[cite: 2]
    public int currentHealth { get; private set; } //[cite: 2]

    [Header("--- UI 血條對接 ---")]
    [Tooltip("請把 Canvas 底下的綠色血條 Slider 拖到這裡")]
    public Slider healthSlider; //[cite: 2]

    [Header("--- ⚡ 彈反 (Parry) 設定 ---")]
    [Tooltip("按下右鍵剛開始的彈反黃金視窗（單位：秒，建議 0.15~0.25 秒）")]
    [SerializeField] private float parryWindow = 0.2f; //[cite: 2]

    [Tooltip("彈反冷卻時間（秒）")]
    [SerializeField] private float parryCooldown = 0.8f; //[cite: 2]

    [Header("--- 🛡️ 防禦與破防設定 ---")]
    [Tooltip("防禦狀態下的減傷比例（0.5 代表減傷 50%）")]
    [SerializeField] private float blockDamageReduction = 0.5f; //[cite: 2]

    [Tooltip("連續被攻擊幾次會觸發破防")]
    [SerializeField] private int maxBlockHits = 3; //[cite: 2]

    [Tooltip("破防後受到的額外傷害加成（0.25 代表額外增加 25% 原始傷害）")]
    [SerializeField] private float guardBreakDamageBonus = 0.25f; //[cite: 2]

    [Tooltip("破防眩暈持續時間（單位：秒，建議 1~2 秒）")]
    [SerializeField] private float stunDuration = 1.5f; //[cite: 2]

    [Header("--- 狀態標記 ---")]
    [Tooltip("目前是否正在防禦（按住滑鼠右鍵）")]
    public bool isBlocking = false; //[cite: 2]

    [Tooltip("目前是否處於彈反黃金視窗內")]
    public bool isParrying = false; //[cite: 2]

    [Tooltip("目前是否處於破防眩暈狀態（無法被玩家控制）")]
    public bool isStunned = false; //[cite: 2]

    [Header("--- 💥 無敵時間設定 💥 ---")]
    [Tooltip("受傷後的無敵時間長度（單位：秒）")]
    [SerializeField] private float invincibilityDuration = 1.5f; //[cite: 2]

    [Tooltip("無敵時身體閃爍的頻率（數值越小閃越快）")]
    [SerializeField] private float flickerInterval = 0.15f; //[cite: 2]

    // 內部計時與狀態變數
    private bool isInvincible = false; //[cite: 2]
    private int currentBlockHits = 0; // 當前防禦狀態下連續被攻擊的次數[cite: 2]
    private float parryTimer = 0f; //[cite: 2]
    private float parryCooldownTimer = 0f; //[cite: 2]

    private Animator anim; //[cite: 2]
    private SpriteRenderer spriteRenderer; //[cite: 2]
    private Coroutine stunCoroutine; //[cite: 2]
    private Coroutine customInvincibleCoroutine; // 專供大招技能使用的無敵協程參考

    void Start()
    {
        currentHealth = maxHealth; //[cite: 2]

        if (healthSlider != null) //[cite: 2]
        {
            healthSlider.maxValue = maxHealth; //[cite: 2]
            healthSlider.value = maxHealth;    //[cite: 2]
        }

        anim = GetComponent<Animator>(); //[cite: 2]
        
        spriteRenderer = GetComponent<SpriteRenderer>(); //[cite: 2]
        if (spriteRenderer == null) //[cite: 2]
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(); //[cite: 2]
        }
    }

    void Update()
    {
        // 1. 倒數彈反冷卻時間
        if (parryCooldownTimer > 0) //[cite: 2]
        {
            parryCooldownTimer -= Time.deltaTime; //[cite: 2]
        }

        // 2. 倒數彈反黃金視窗時間
        if (isParrying) //[cite: 2]
        {
            parryTimer -= Time.deltaTime; //[cite: 2]
            if (parryTimer <= 0) //[cite: 2]
            {
                isParrying = false; // 彈反視窗結束，若繼續按著右鍵會自動轉為一般防禦[cite: 2]
            }
        }

        // 3. 破防眩暈狀態判定 (眩暈中無法進行防禦或彈反)
        if (isStunned) //[cite: 2]
        {
            if (isBlocking || isParrying) //[cite: 2]
            {
                isBlocking = false; //[cite: 2]
                isParrying = false; //[cite: 2]
                if (anim != null) anim.SetBool("isBlocking", false); //[cite: 2]
            }
            return; //[cite: 2]
        }

        // 4. 防禦 / 彈反輸入邏輯 (滑鼠右鍵)
        if (Input.GetMouseButtonDown(1)) // 剛按下的瞬間：觸發彈反[cite: 2]
        {
            if (parryCooldownTimer <= 0) //[cite: 2]
            {
                StartParry(); //[cite: 2]
            }
        }

        if (Input.GetMouseButton(1)) // 按住右鍵：保持防禦狀態[cite: 2]
        {
            if (!isBlocking) //[cite: 2]
            {
                isBlocking = true; //[cite: 2]
                if (anim != null) anim.SetBool("isBlocking", true); //[cite: 2]
            }
        }
        else // 鬆開右鍵：取消防禦並重置連續擋格次數[cite: 2]
        {
            if (isBlocking) //[cite: 2]
            {
                isBlocking = false; //[cite: 2]
                currentBlockHits = 0; //[cite: 2]
                if (anim != null) anim.SetBool("isBlocking", false); //[cite: 2]
            }
        }
    }

    // 發起彈反判定
    private void StartParry()
    {
        isParrying = true; //[cite: 2]
        parryTimer = parryWindow; //[cite: 2]
        parryCooldownTimer = parryCooldown; //[cite: 2]

        if (anim != null) anim.SetTrigger("parry"); //[cite: 2]
        Debug.Log("⚡【Aria 彈反】架刀開啟 0.2 秒彈反黃金視窗！"); //[cite: 2]
    }

    // 🔴 核心受傷入口 (支援 彈反 ➔ 防禦 ➔ 破防 ➔ 一般受擊 判定鏈)[cite: 2]
    public void TakeDamage(int damage, Transform attacker = null) //[cite: 2]
    {
        // 💥 1. 無敵狀態，無視傷害
        if (isInvincible) //[cite: 2]
        {
            Debug.Log("【玩家狀態】🛡️ 處於無敵時間，無視傷害！"); //[cite: 2]
            return; //[cite: 2]
        }

        // 💥 2.【完美彈反成功！】
        if (isParrying) //[cite: 2]
        {
            OnParrySuccess(attacker); //[cite: 2]
            return; // 瞬間攔截，不扣血、不計入破防次數！[cite: 2]
        }

        int finalDamage = damage; //[cite: 2]

        // 💥 3. 處於破防眩暈狀態受擊：原始傷害 + 25% 加成
        if (isStunned) //[cite: 2]
        {
            finalDamage = Mathf.RoundToInt(damage * (1f + guardBreakDamageBonus)); //[cite: 2]
            Debug.LogWarning($"【眩暈受擊！】受到破防增幅傷害：{finalDamage} 點（原始傷害 {damage} + 25%）"); //[cite: 2]
        }
        // 💥 4. 處於一般防禦狀態受擊
        else if (isBlocking) //[cite: 2]
        {
            currentBlockHits++; //[cite: 2]
            Debug.Log($"【玩家防禦】🛡️ 格擋成功！當前連續防禦次數：{currentBlockHits} / {maxBlockHits}"); //[cite: 2]

            // 檢查是否達到破防上限
            if (currentBlockHits >= maxBlockHits) //[cite: 2]
            {
                // 破防這一下：無視防禦減傷，並加上 25% 破防傷害加成
                finalDamage = Mathf.RoundToInt(damage * (1f + guardBreakDamageBonus)); //[cite: 2]
                Debug.LogWarning($"💥【防禦被打破！】這一下直接承受破防加成傷害：{finalDamage} 點！"); //[cite: 2]

                TriggerGuardBreak(); //[cite: 2]
            }
            else
            {
                // 未破防：正常減傷 50%
                finalDamage = Mathf.RoundToInt(damage * (1f - blockDamageReduction)); //[cite: 2]
            }
        }
        // 💥 5. 一般無防禦受擊
        else
        {
            if (anim != null) anim.SetTrigger("hurt");  //[cite: 2]
        }

        // 扣除血量
        currentHealth -= finalDamage; //[cite: 2]
        if (currentHealth < 0) currentHealth = 0; //[cite: 2]

        if (healthSlider != null) healthSlider.value = currentHealth; //[cite: 2]

        Debug.Log($"【玩家血量】Aria 受到 {finalDamage} 點實際傷害！目前剩餘血量: {currentHealth}/{maxHealth}"); //[cite: 2]

        // 如果沒死，啟動無敵閃爍時間
        if (currentHealth > 0)  //[cite: 2]
        {
            StartCoroutine(BecomeInvincibleRoutine()); //[cite: 2]
        }
        else
        {
            Die(); //[cite: 2]
        }
    }

    // 重載 TakeDamage，保持向後相容 (若舊腳本呼叫未帶 attacker)[cite: 2]
    public void TakeDamage(int damage) //[cite: 2]
    {
        TakeDamage(damage, null); //[cite: 2]
    }

    // 🩸【新增】環境 / 狀態扣血 (專供血液 0% 乾涸、中毒等，不觸發無敵時間與無敵閃爍)[cite: 2]
    public void TakeEnvironmentDamage(int damage) //[cite: 2]
    {
        // 死亡判定
        if (currentHealth <= 0) return; //[cite: 2]

        currentHealth -= damage; //[cite: 2]
        if (currentHealth < 0) currentHealth = 0; //[cite: 2]

        if (healthSlider != null) healthSlider.value = currentHealth; //[cite: 2]

        Debug.LogWarning($"🩸【血液乾涸扣血】Aria 受到 {damage} 點環境持續傷害！剩餘血量: {currentHealth}/{maxHealth}"); //[cite: 2]

        if (currentHealth <= 0) //[cite: 2]
        {
            Die(); //[cite: 2]
        }
    }

    // ----------------------------------------------------
    // 🛡️【新增功能】手動觸發指定秒數無敵時間 API (供 BloodBurst 呼叫)
    // ----------------------------------------------------

    /// <summary>
    /// 手動觸發指定秒數的無敵時間 (帶閃爍效果)
    /// </summary>
    public void TriggerInvincibility(float duration)
    {
        if (customInvincibleCoroutine != null)
        {
            StopCoroutine(customInvincibleCoroutine);
        }
        customInvincibleCoroutine = StartCoroutine(CustomInvincibleRoutine(duration));
    }

    private IEnumerator CustomInvincibleRoutine(float duration)
    {
        isInvincible = true;
        Debug.Log($"🛡️【大招無敵】Aria 進入 {duration} 秒絕對無敵狀態！");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.3f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1.0f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(flickerInterval);
            elapsed += flickerInterval;
        }

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 1.0f;
            spriteRenderer.color = color;
        }

        isInvincible = false;
        Debug.Log("🛡️【大招無敵】無敵狀態結束！");
    }

    // ----------------------------------------------------

    // ⚡ 完美彈反成功處理[cite: 2]
    private void OnParrySuccess(Transform attacker) //[cite: 2]
    {
        isParrying = false; // 彈反成功後關閉視窗[cite: 2]
        Debug.Log("✨【完美彈反！】叮！成功格擋攻擊並免疫傷害！"); //[cite: 2]

        if (anim != null) anim.SetTrigger("parrySuccess"); // 播放彈反成功/反擊動畫[cite: 2]

        // 💥 主動通知攻擊者：觸發怪物的 10% 血量傷害與彈反次數累加[cite: 2]
        if (attacker != null) //[cite: 2]
        {
            EnemyHealth enemy = attacker.GetComponent<EnemyHealth>(); //[cite: 2]
            if (enemy == null) //[cite: 2]
            {
                // 預防 Hitbox 位於子物件的情況，向上抓取父物件的 EnemyHealth[cite: 2]
                enemy = attacker.GetComponentInParent<EnemyHealth>(); //[cite: 2]
            }

            if (enemy != null) //[cite: 2]
            {
                enemy.OnParriedByPlayer(); // 呼叫怪物的被彈反機制[cite: 2]
            }
        }
    }

    // 💥 觸發玩家破防眩暈處理[cite: 2]
    private void TriggerGuardBreak() //[cite: 2]
    {
        Debug.LogWarning("💥【破防觸發！】Aria 陷入眩暈，進入無法控制狀態！"); //[cite: 2]
        
        isBlocking = false; //[cite: 2]
        isParrying = false; //[cite: 2]
        currentBlockHits = 0; //[cite: 2]
        if (anim != null) anim.SetBool("isBlocking", false); //[cite: 2]

        if (stunCoroutine != null) //[cite: 2]
        {
            StopCoroutine(stunCoroutine); //[cite: 2]
        }
        stunCoroutine = StartCoroutine(StunRoutine()); //[cite: 2]
    }

    // 💥 玩家眩暈協程[cite: 2]
    private IEnumerator StunRoutine() //[cite: 2]
    {
        isStunned = true; //[cite: 2]

        yield return new WaitForSeconds(stunDuration); //[cite: 2]

        isStunned = false; //[cite: 2]
        Debug.Log("【眩暈解除】Aria 恢復控制！"); //[cite: 2]
    }

    // 處理受傷後的無敵時間與角色透明度閃爍效果[cite: 2]
    private IEnumerator BecomeInvincibleRoutine() //[cite: 2]
    {
        isInvincible = true; //[cite: 2]
        Debug.Log("【玩家狀態】✨ Aria 進入無敵狀態！"); //[cite: 2]

        float elapsed = 0f; //[cite: 2]
        while (elapsed < invincibilityDuration) //[cite: 2]
        {
            if (spriteRenderer != null) //[cite: 2]
            {
                Color color = spriteRenderer.color; //[cite: 2]
                color.a = 0.3f; //[cite: 2]
                spriteRenderer.color = color; //[cite: 2]
            }

            yield return new WaitForSeconds(flickerInterval); //[cite: 2]
            elapsed += flickerInterval; //[cite: 2]

            if (spriteRenderer != null) //[cite: 2]
            {
                Color color = spriteRenderer.color; //[cite: 2]
                color.a = 1.0f; //[cite: 2]
                spriteRenderer.color = color; //[cite: 2]
            }

            yield return new WaitForSeconds(flickerInterval); //[cite: 2]
            elapsed += flickerInterval; //[cite: 2]
        }

        if (spriteRenderer != null) //[cite: 2]
        {
            Color color = spriteRenderer.color; //[cite: 2]
            color.a = 1.0f; //[cite: 2]
            spriteRenderer.color = color; //[cite: 2]
        }

        isInvincible = false; //[cite: 2]
        Debug.Log("【玩家狀態】❌ Aria 無敵時間結束！"); //[cite: 2]
    }

    void Die() //[cite: 2]
    {
        if (anim != null) anim.SetTrigger("death");  //[cite: 2]
        Debug.Log("【遊戲結束】Aria 已經倒下了..."); //[cite: 2]
        this.enabled = false;  //[cite: 2]
    }

    // 補血功能[cite: 2]
    public void Heal(int amount) //[cite: 2]
    {
        currentHealth += amount; //[cite: 2]

        if (currentHealth > maxHealth) //[cite: 2]
        {
            currentHealth = maxHealth; //[cite: 2]
        }

        if (healthSlider != null) //[cite: 2]
        {
            healthSlider.value = currentHealth; //[cite: 2]
        }

        Debug.Log($"【玩家血量】✨ Aria 恢復了生命！目前血量: {currentHealth}/{maxHealth}"); //[cite: 2]
    }
}