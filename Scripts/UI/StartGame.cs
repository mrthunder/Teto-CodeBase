using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class StartGame : UIScreen
{


    public GameObject m_Meteor;

    Vector3 m_FinalPosition = new Vector3(0, 0, 0);

    public override void ShowScreen()
    {
        SaveManager.Instance.m_currentGame = new Game();
    }

    public void OnContinueButton()
    {
        //ScreenManager.Instance.Show<GameScreen>();
        //SceneManager.LoadScene(ProjectNames.LevelOne, LoadSceneMode.Single);
        if (SaveManager.Instance.LoadGame())
        {
            SceneLoader.scene = SaveManager.Instance.m_currentGame.m_sLevelName;
            if (string.IsNullOrEmpty(SceneLoader.scene)) return;
            ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);
        }


    }

    public void OnNewGameButton()
    {
        //ScreenManager.Instance.Show<GameScreen>();
        StartCoroutine(FallenMeteor());
    }

    IEnumerator FallenMeteor()
    {
        SoundManager.Instance.SetGameParameter(GameParameters.Distance, 1);
        
        m_Meteor.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width + 10, Screen.height + 10, 10));
        m_FinalPosition = Camera.main.ScreenToWorldPoint(new Vector3(-100, -100, 10));
        yield return m_Meteor.transform.DOMove(m_FinalPosition, 2f).OnStart(()=>SoundManager.Instance.PlayEvent(SoundEvents.Play_Meteor,m_Meteor)).OnComplete(() => SoundManager.Instance.PlayEvent(SoundEvents.Stop_Meteor, m_Meteor)).WaitForCompletion();
        SaveManager.Instance.SaveGame();
        SceneLoader.scene = ProjectNames.LevelTwo;
        ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);
    }

    public void OnLevelSelectButton()
    {
        SceneLoader.scene = ProjectNames.LevelOne;
        ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);


    }

    public void OnBackButton()
    {
        ScreenManager.Instance.Show<MenuScreen>();
    }

   
}
