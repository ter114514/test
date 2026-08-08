using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 標題選單控制。標題是可互動 Button：
/// - 懸停時播放一次性動畫（Animator）
/// - 點擊後標題上升並縮小，下方選單淡入出現
/// - 展開後閒置一段時間，自動回復到初始狀態
/// 標題上浮後維持原樣，不再觸發懸停動畫。
/// </summary>
public class TitleMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("標題")]
    [Tooltip("標題的 RectTransform（上浮與縮小的對象）")]
    [SerializeField] RectTransform titleRect;
    [Tooltip("標題的 Animator，用於懸停動畫（沒有可留空）")]
    [SerializeField] Animator titleAnimator;

    [Header("上浮設定")]
    [Tooltip("點擊後標題往上移動的距離")]
    [SerializeField] float floatUpDistance = 200f;
    [Tooltip("上浮/回復的移動與縮放速度")]
    [SerializeField] float moveSpeed = 4f;
    [Tooltip("上浮後標題縮小到的比例（1=原大小，0.6=縮到六成）")]
    [SerializeField] float shrinkScale = 0.6f;

    [Header("選單")]
    [Tooltip("點擊後出現的選單面板（需有 CanvasGroup）")]
    [SerializeField] CanvasGroup menuGroup;
    [Tooltip("選單淡入淡出速度")]
    [SerializeField] float fadeSpeed = 4f;

    [Header("閒置回復")]
    [Tooltip("展開後無操作多久回復初始狀態（秒）")]
    [SerializeField] float idleTimeout = 8f;

    static readonly int HoverHash = Animator.StringToHash("Hover");

    Vector2 titleBasePos;      // 標題原始位置
    Vector2 titleUpPos;        // 上浮後位置
    Vector3 titleBaseScale;    // 標題原始縮放
    bool isExpanded;           // 是否已展開選單
    float idleTimer;           // 閒置計時

    void Awake()
    {
        titleBasePos = titleRect.anchoredPosition;
        titleUpPos = titleBasePos + Vector2.up * floatUpDistance;
        titleBaseScale = titleRect.localScale;
    }

    void Start()
    {
        // 初始：選單隱藏、不可互動
        SetMenuVisible(false, instant: true);
    }

    void Update()
    {
        // 位置補間（上浮 / 回復）
        Vector2 targetPos = isExpanded ? titleUpPos : titleBasePos;
        titleRect.anchoredPosition = Vector2.Lerp(
            titleRect.anchoredPosition, targetPos, moveSpeed * Time.unscaledDeltaTime);

        // 縮放補間（上浮縮小 / 回復原大小）
        Vector3 targetScale = isExpanded ? titleBaseScale * shrinkScale : titleBaseScale;
        titleRect.localScale = Vector3.Lerp(
            titleRect.localScale, targetScale, moveSpeed * Time.unscaledDeltaTime);

        // 選單淡入 / 淡出
        float targetAlpha = isExpanded ? 1f : 0f;
        menuGroup.alpha = Mathf.MoveTowards(
            menuGroup.alpha, targetAlpha, fadeSpeed * Time.unscaledDeltaTime);

        // 展開狀態下偵測閒置
        if (isExpanded)
        {
            if (AnyInput())
                idleTimer = 0f;
            else
                idleTimer += Time.unscaledDeltaTime;

            if (idleTimer >= idleTimeout)
                Collapse();
        }
    }

    bool AnyInput()
    {
        var mouse = Mouse.current;
        var kb = Keyboard.current;
        bool mouseMoved = mouse != null && mouse.delta.ReadValue().sqrMagnitude > 0.01f;
        bool mouseClick = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool keyPressed = kb != null && kb.anyKey.wasPressedThisFrame;
        return mouseMoved || mouseClick || keyPressed;
    }

    // ---- 懸停（IPointerEnter/Exit）----

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isExpanded) return;   // 已展開就不再播懸停動畫，維持原樣
        if (titleAnimator != null)
            titleAnimator.SetBool(HoverHash, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (titleAnimator != null)
            titleAnimator.SetBool(HoverHash, false);
    }

    // ---- 點擊（由標題 Button 的 onClick 呼叫 OnTitleClicked）----

    public void OnTitleClicked()
    {
        if (!isExpanded)
            Expand();
    }

    void Expand()
    {
        isExpanded = true;
        idleTimer = 0f;
        // 解除懸停動畫，避免 Animator 和程式搶 scale 控制
        if (titleAnimator != null)
            titleAnimator.SetBool(HoverHash, false);
        SetMenuVisible(true);
    }

    void Collapse()
    {
        isExpanded = false;
        SetMenuVisible(false);
        if (titleAnimator != null)
            titleAnimator.SetBool(HoverHash, false);
    }

    void SetMenuVisible(bool visible, bool instant = false)
    {
        menuGroup.interactable = visible;
        menuGroup.blocksRaycasts = visible;
        if (instant)
            menuGroup.alpha = visible ? 1f : 0f;
    }
}