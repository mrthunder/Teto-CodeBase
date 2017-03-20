using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseGame : UIScreen
{

    public override void ShowScreen()
    {
        print("pause");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
        FindObjectOfType<CameraFollow>().CameraLock(true);
        GameManager.Instance.m_Player.LockTotalMovement(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("Start"))
        {
            OnResumeButton();
        }
    }
   

    public void OnResumeButton()
    {
        ScreenManager.Instance.Show<GameScreen>();
        if (!GameManager.Instance.m_Player.m_bTwoDMov)
        {
            FindObjectOfType<CameraFollow>().CameraLock(false);
        }
        GameManager.Instance.m_Player.LockTotalMovement(false);
    }



    public void OnQuitButton()
    {

        //ScreenManager.Instance.Show<MenuScreen>();
        //ScreenManager.Instance.GoToNextLevel(ProjectNames.MainMenu);
        ScreenManager.DestroySingleton();
        SceneManager.LoadScene(ProjectNames.MainMenu);
        Time.timeScale = 1;
        //SceneLoader.scene = ProjectNames.MainMenu;
        //ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);

        /*#if UNITY_EDITOR

                UnityEditor.EditorApplication.isPlaying = false;
        #else
                 Application.Quit();
        #endif*/
    }
}


