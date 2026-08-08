using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 遊戲設定的單一來源。負責讀寫 PlayerPrefs 並套用到系統。
/// </summary>
public static class GameSettings
{
    const string KEY_MASTER = "vol_master";
    const string KEY_MUSIC = "vol_music";
    const string KEY_SFX = "vol_sfx";
    const string KEY_WINDOWMODE = "gfx_windowmode";
    const string KEY_RESOLUTION = "gfx_resolution";
    const string KEY_FRAMERATE = "gfx_framerate";

    // ---- 固定選項清單 ----

    /// <summary>解析度選項（寬, 高）。索引 2 = 1920x1080 為預設</summary>
    public static readonly (int width, int height)[] Resolutions =
    {
        (1280, 720),
        (1600, 900),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
    };
    public const int DefaultResolutionIndex = 2;   // 1920x1080

    /// <summary>幀數選項，-1 代表無上限。索引 1 = 60 為預設</summary>
    public static readonly int[] FrameRates = { 30, 60, 90, 120, 240, -1 };
    public const int DefaultFrameRateIndex = 1;    // 60

    /// <summary>視窗模式選項顯示文字</summary>
    public static readonly string[] WindowModeNames = { "全螢幕", "視窗" };
    public const int DefaultWindowModeIndex = 0;   // 全螢幕

    // ---- 音量（0~1）----

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        set { PlayerPrefs.SetFloat(KEY_MASTER, value); ApplyVolume("MasterVolume", value); }
    }
    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(KEY_MUSIC, 1f);
        set { PlayerPrefs.SetFloat(KEY_MUSIC, value); ApplyVolume("MusicVolume", value); }
    }
    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat(KEY_SFX, 1f);
        set { PlayerPrefs.SetFloat(KEY_SFX, value); ApplyVolume("SFXVolume", value); }
    }

    // ---- 視窗模式 ----

    /// <summary>0 = 全螢幕，1 = 視窗</summary>
    public static int WindowModeIndex
    {
        get => PlayerPrefs.GetInt(KEY_WINDOWMODE, DefaultWindowModeIndex);
        set
        {
            PlayerPrefs.SetInt(KEY_WINDOWMODE, value);
            ApplyResolutionAndWindow();
        }
    }

    // ---- 解析度 ----

    public static int ResolutionIndex
    {
        get => PlayerPrefs.GetInt(KEY_RESOLUTION, DefaultResolutionIndex);
        set
        {
            PlayerPrefs.SetInt(KEY_RESOLUTION, value);
            ApplyResolutionAndWindow();
        }
    }

    // ---- 幀數 ----

    public static int FrameRateIndex
    {
        get => PlayerPrefs.GetInt(KEY_FRAMERATE, DefaultFrameRateIndex);
        set
        {
            PlayerPrefs.SetInt(KEY_FRAMERATE, value);
            ApplyFrameRate();
        }
    }

    // ---- 套用方法 ----

    static void ApplyResolutionAndWindow()
    {
        int resIdx = Mathf.Clamp(ResolutionIndex, 0, Resolutions.Length - 1);
        var (w, h) = Resolutions[resIdx];

        // 視窗模式：0 全螢幕，1 視窗
        var mode = WindowModeIndex == 0
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        Screen.SetResolution(w, h, mode);
    }

    static void ApplyFrameRate()
    {
        int idx = Mathf.Clamp(FrameRateIndex, 0, FrameRates.Length - 1);
        int fps = FrameRates[idx];

        // 幀數上限需要關閉 VSync 才會生效
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;   // -1 = 無上限
    }

    static AudioMixer mixer;
    public static void SetMixer(AudioMixer m) => mixer = m;

    static void ApplyVolume(string param, float linear)
    {
        if (mixer == null) return;
        float dB = linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
        mixer.SetFloat(param, dB);
    }

    /// <summary>遊戲啟動時呼叫，套用所有已儲存設定</summary>
    public static void ApplyAll(AudioMixer m)
    {
        SetMixer(m);
        ApplyVolume("MasterVolume", MasterVolume);
        ApplyVolume("MusicVolume", MusicVolume);
        ApplyVolume("SFXVolume", SFXVolume);
        ApplyResolutionAndWindow();
        ApplyFrameRate();
        PlayerPrefs.Save();
    }
}