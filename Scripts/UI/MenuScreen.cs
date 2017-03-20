using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class MenuScreen : UIScreen
{

    public override void ShowScreen()
    {
        SoundManager.Instance.SetGameParameter(GameParameters.BG_Type, 1);
    }

    public void OnStartGameButton()
    {
        ScreenManager.Instance.Show<StartGame>();
        //ScreenManager.Instance.GoToNextLevel(ProjectNames.LevelOne);
        //ScreenManager.Instance.Show<GameScreen>();
    }
    

public void OnCreditsButton()
    {
        ScreenManager.Instance.Show<CreditsScreen>();
        
    }

    public void OnSettingsButton()
    {
        ScreenManager.Instance.Show<SettingsScreen>();
    }

    public void OnQuitButton()
    {
        #if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
        #else
         Application.Quit();
        #endif
    }

}
