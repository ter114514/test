using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 設定面板 UI。音量用滑桿，畫面設定用固定選項的下拉選單。
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("音量滑桿")]
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [Header("畫面下拉選單")]
    [SerializeField] TMP_Dropdown windowModeDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown frameRateDropdown;

    [Header("控制")]
    [SerializeField] Button closeButton;

    void Start()
    {
        SetupDropdowns();
        LoadCurrentValues();
        HookEvents();
    }

    void SetupDropdowns()
    {
        // 視窗模式：全螢幕 / 視窗
        windowModeDropdown.ClearOptions();
        windowModeDropdown.AddOptions(new List<string>(GameSettings.WindowModeNames));

        // 解析度：依固定清單
        resolutionDropdown.ClearOptions();
        var resOptions = new List<string>();
        foreach (var (w, h) in GameSettings.Resolutions)
            resOptions.Add($"{w} x {h}");
        resolutionDropdown.AddOptions(resOptions);

        // 幀數：依固定清單，-1 顯示「無上限」
        frameRateDropdown.ClearOptions();
        var fpsOptions = new List<string>();
        foreach (var fps in GameSettings.FrameRates)
            fpsOptions.Add(fps < 0 ? "無上限" : fps.ToString());
        frameRateDropdown.AddOptions(fpsOptions);
    }

    void LoadCurrentValues()
    {
        masterSlider.value = GameSettings.MasterVolume;
        musicSlider.value = GameSettings.MusicVolume;
        sfxSlider.value = GameSettings.SFXVolume;

        // 下拉選單設成當前值（不觸發事件）
        windowModeDropdown.SetValueWithoutNotify(GameSettings.WindowModeIndex);
        resolutionDropdown.SetValueWithoutNotify(GameSettings.ResolutionIndex);
        frameRateDropdown.SetValueWithoutNotify(GameSettings.FrameRateIndex);

        windowModeDropdown.RefreshShownValue();
        resolutionDropdown.RefreshShownValue();
        frameRateDropdown.RefreshShownValue();
    }

    void HookEvents()
    {
        masterSlider.onValueChanged.AddListener(v => GameSettings.MasterVolume = v);
        musicSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
        sfxSlider.onValueChanged.AddListener(v => GameSettings.SFXVolume = v);

        windowModeDropdown.onValueChanged.AddListener(v => GameSettings.WindowModeIndex = v);
        resolutionDropdown.onValueChanged.AddListener(v => GameSettings.ResolutionIndex = v);
        frameRateDropdown.onValueChanged.AddListener(v => GameSettings.FrameRateIndex = v);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    void Close()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    void OnDisable() => PlayerPrefs.Save();
}