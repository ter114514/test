using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// 暫停選單。ESC 開關，開啟時暫停遊戲時間。
/// 提供繼續遊戲、回到主畫面、退出遊戲。
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] GameObject pausePanel;

    [Header("按鈕")]
    [SerializeField] Button resumeButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button quitButton;

    public bool IsPaused { get; private set; }

    void Start()
    {
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // ESC 切換暫停
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }

    void Pause()
    {
        IsPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;   // 暫停遊戲時間（物理、動畫、計時全停）
    }

    void Resume()
    {
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;   // 恢復時間
    }

    void ReturnToMainMenu()
    {
        // SaveManager 的 ReturnToMenu 會把 timeScale 復原，避免主畫面凍結
        Time.timeScale = 1f;
        SaveManager.Instance.ReturnToMenu();
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        SaveManager.Instance.QuitGame();
    }
}