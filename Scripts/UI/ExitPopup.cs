using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ExitPopup : UIScreen
{
    public void OnConfirmButton()
    {
        SceneManager.UnloadScene("GameScenes");
        ScreenManager.Instance.Show<MenuScreen>();
    }

    public void OnCancelButton()
    {
        ScreenManager.Instance.Show<GameScreen>();
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }
}
