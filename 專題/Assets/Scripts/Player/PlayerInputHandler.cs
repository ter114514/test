using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家輸入偵測器。
/// 唯一職責：偵測鍵盤／手把訊號，並以事件形式對外發送。
/// 不處理移動、不處理戰鬥、不保存遊戲狀態。
/// 其他系統透過訂閱事件來接收輸入訊號。
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    // ---- 對外發送的輸入事件 ----

    /// <summary>水平移動輸入改變時發送，帶 -1~1 的方向值</summary>
    public event Action<float> OnMove;

    /// <summary>按下跳躍鍵瞬間</summary>
    public event Action OnJumpPressed;
    /// <summary>放開跳躍鍵瞬間</summary>
    public event Action OnJumpReleased;

    /// <summary>按下攻擊鍵瞬間</summary>
    public event Action OnAttackPressed;

    /// <summary>按下衝刺鍵瞬間</summary>
    public event Action OnDashPressed;

    /// <summary>按下防禦鍵瞬間</summary>
    public event Action OnBlockPressed;
    /// <summary>放開防禦鍵瞬間</summary>
    public event Action OnBlockReleased;

    PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += HandleMove;
        controls.Player.Move.canceled += HandleMove;   // 放開時歸零
        controls.Player.Jump.performed += HandleJumpPressed;
        controls.Player.Jump.canceled += HandleJumpReleased;
        controls.Player.Attack.performed += HandleAttack;
        controls.Player.Dash.performed += HandleDash;
        controls.Player.Block.performed += HandleBlockPressed;
        controls.Player.Block.canceled += HandleBlockReleased;
    }

    void OnDisable()
    {
        controls.Player.Move.performed -= HandleMove;
        controls.Player.Move.canceled -= HandleMove;
        controls.Player.Jump.performed -= HandleJumpPressed;
        controls.Player.Jump.canceled -= HandleJumpReleased;
        controls.Player.Attack.performed -= HandleAttack;
        controls.Player.Dash.performed -= HandleDash;
        controls.Player.Block.performed -= HandleBlockPressed;
        controls.Player.Block.canceled -= HandleBlockReleased;

        controls.Player.Disable();
    }

    // ---- 把 Input System 的 callback 轉發成自訂事件 ----

    void HandleMove(InputAction.CallbackContext ctx)
        => OnMove?.Invoke(ctx.ReadValue<float>());

    void HandleJumpPressed(InputAction.CallbackContext ctx)
        => OnJumpPressed?.Invoke();

    void HandleJumpReleased(InputAction.CallbackContext ctx)
        => OnJumpReleased?.Invoke();

    void HandleAttack(InputAction.CallbackContext ctx)
        => OnAttackPressed?.Invoke();

    void HandleDash(InputAction.CallbackContext ctx)
        => OnDashPressed?.Invoke();

    void HandleBlockPressed(InputAction.CallbackContext ctx)
        => OnBlockPressed?.Invoke();

    void HandleBlockReleased(InputAction.CallbackContext ctx)
        => OnBlockReleased?.Invoke();
}