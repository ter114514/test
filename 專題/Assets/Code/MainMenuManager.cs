using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 開始遊戲按鈕的功能
    public void PlayGame()
    {
        // "GameScene" 請改成你原本那個「有角色、有史萊姆」的關卡場景名稱！
        SceneManager.LoadScene("Game"); 
    }

    // 結束遊戲按鈕的功能
    public void QuitGame()
    {
        Debug.Log("離開遊戲！");
        Application.Quit(); 
    }
}