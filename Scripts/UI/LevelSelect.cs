using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelSelect : UIScreen {

	public void OnBackButton()
    {
        Debug.Log("Hello", gameObject);
        ScreenManager.Instance.Show<StartGame>();
    }

    public void OnLevel1Button()
    {
        //ScreenManager.Instance.Show<GameScreen>();
        //SceneManager.LoadScene("LV1", LoadSceneMode.Single);
        SceneLoader.scene = ProjectNames.LevelOne;
        ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);
    }

    public void OnLevel2Button()
    {
        //ScreenManager.Instance.Show<GameScreen>();
        // SceneManager.LoadScene("Level 2 testing", LoadSceneMode.Single);
        SceneLoader.scene = ProjectNames.LevelTwo;
        ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);
    }

    public void OnLevel3Button()
    {
        //ScreenManager.Instance.Show<GameScreen>();
        //SceneManager.LoadScene("Level 3", LoadSceneMode.Single);

        SceneLoader.scene = ProjectNames.LevelThree;
        ScreenManager.Instance.GoToNextLevel(ProjectNames.LoadingScene);
    }
}
