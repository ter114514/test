using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 懸停時寶石平滑變亮到最亮並維持；到最亮後可選加入輕微呼吸明滅。
/// </summary>
public class HoverAnimBlend : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Animator[] targets;
    [SerializeField] float speed = 3f;
    [SerializeField] string stateName = "Glow";

    [Header("最亮時的呼吸效果（可選）")]
    [Tooltip("到最亮後是否加入輕微明滅")]
    [SerializeField] bool breatheAtPeak = false;
    [Tooltip("呼吸幅度（0=不呼吸）")]
    [SerializeField] float breatheAmount = 0.05f;
    [Tooltip("呼吸速度")]
    [SerializeField] float breatheSpeed = 2f;

    bool isHovering;
    float blend;

    void Update()
    {
        float target = isHovering ? 1f : 0f;
        blend = Mathf.MoveTowards(blend, target, speed * Time.unscaledDeltaTime);

        float displayBlend = blend;

        // 到最亮且仍懸停時，加入輕微呼吸
        if (breatheAtPeak && isHovering && blend >= 1f)
        {
            float breathe = Mathf.Sin(Time.unscaledTime * breatheSpeed) * breatheAmount;
            displayBlend = Mathf.Clamp01(1f - breatheAmount + breathe);
        }

        foreach (var a in targets)
        {
            if (a == null) continue;
            a.Play(stateName, 0, displayBlend);
            a.speed = 0;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovering = true;
    public void OnPointerExit(PointerEventData eventData) => isHovering = false;
}