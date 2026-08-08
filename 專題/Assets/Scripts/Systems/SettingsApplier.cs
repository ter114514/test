using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 遊戲啟動時載入並套用已儲存的設定。放在最先載入的場景。
/// </summary>
public class SettingsApplier : MonoBehaviour
{
    [SerializeField] AudioMixer mainMixer;

    void Awake()
    {
        GameSettings.ApplyAll(mainMixer);
        DontDestroyOnLoad(gameObject);   // 跨場景保留 Mixer 參考
    }
}