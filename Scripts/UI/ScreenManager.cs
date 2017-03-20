using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;


public class ScreenManager : Singleton<ScreenManager> {

    private Dictionary<Type, UIScreen> screens;
    private UIScreen currentScreen;

	// Use this for initialization
	void Awake ()
    {
        SoundManager.Instance.SetGameParameter(GameParameters.BG_Type, 1, Camera.main.gameObject);
        DontDestroyOnLoad(this.gameObject);
        screens = new Dictionary<Type, UIScreen>();

        foreach (UIScreen screen in GetComponentsInChildren<UIScreen>())
        {
            screen.gameObject.SetActive(false);
            screens.Add(screen.GetType(), screen);
        }
        // Only line that would need to be changed
        Show<TitleScreen>();
	}

	// Any method with the Generic <> symbols, should be supplied with a certain type!
    public void Show<T> () where T : UIScreen
    {
        // Based on T, we create a Type variabile, this type variable could for exsample be Gamescreen (that dosent refer to the instance in our screen though!)
        Type screenType = typeof(T);

        if (currentScreen != null)
        {
            //print(currentScreen.GetType().Name);
            currentScreen.gameObject.SetActive(false);
        }

        // Based on the GameScreen type, we access the dictionary, to provide us with the instance in our scene, with the type GameScreen.
        UIScreen newScreen = screens[screenType];
        newScreen.gameObject.SetActive(true);
        newScreen.ShowScreen();
        currentScreen = newScreen;
        //print(currentScreen.GetType().Name);
    }

    public void Hide()
    {
        if (currentScreen != null)
        {
            currentScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Standart method to handle level load
    /// </summary>
    /// <param name="name">Name of the next level</param>
    public void GoToNextLevel(string name)
    {
        SceneManager.LoadScene(name);
    }

}
