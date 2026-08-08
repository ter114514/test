using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 篝火存檔點。玩家進入範圍後按互動鍵存檔。
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SaveCampfire : MonoBehaviour
{
    [SerializeField] GameObject promptUI;      // 「按 E 存檔」提示
    [SerializeField] GameObject savedFeedback; // 「已存檔」提示

    bool playerInRange;

    void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (promptUI != null) promptUI.SetActive(false);
        if (savedFeedback != null) savedFeedback.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        // 按 E 存檔（用新版 Input System 直接讀鍵盤）
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            DoSave();
    }

    void DoSave()
    {
        SaveManager.Instance.SaveToCurrentSlot();
        if (savedFeedback != null)
            StartCoroutine(ShowFeedback());
    }

    System.Collections.IEnumerator ShowFeedback()
    {
        savedFeedback.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        savedFeedback.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (promptUI != null) promptUI.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (promptUI != null) promptUI.SetActive(false);
    }
}