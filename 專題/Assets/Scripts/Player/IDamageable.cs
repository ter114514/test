using UnityEngine;

/// <summary>可受傷的物件。玩家與敵人共用，讓攻擊方不需知道打到的是誰。</summary>
public interface IDamageable
{
    void TakeDamage(float amount, Vector2 knockback);
}