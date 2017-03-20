using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;


public class SceneLoader : MonoBehaviour {

    private bool loadScene = false;

    public Image FadeImg;
    public float fadeSpeed = 1.5f;

    public float loadingProgress = 0;
    int i = 0;
    public static string scene;
    [SerializeField]
    private Text loadingText;
    AsyncOperation async;

    public static Type ui;

    void Start()
    {
        ScreenManager.Instance.Hide();
        SaveManager.Instance.m_currentGame.m_sLevelName = scene;
        async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;
        StartCoroutine(Loading());
        StartCoroutine(LoadingEffect());

    }
	

    IEnumerator Loading()
    {
        yield return new WaitUntil(() => async.progress == 0.9f);
        ScreenManager.Instance.Show<GameScreen>();
        async.allowSceneActivation = true;
        yield return new WaitUntil(() => async.isDone);
        while (FadeImg.color != new Color(0,0,0,0))
        {
            Fade();
            yield return null;
        }
        

    }

    IEnumerator LoadingEffect()
    {
        loadingText.text = "Loading";
        while (!async.isDone)
        {
            loadingText.text+=".";
            i = (i + 1) % 4;
            if(i==0)
            {
                loadingText.text = "Loading";
               
            }
            yield return new WaitForSeconds(.2f);
        }

    }

    void Fade()
    {
        FadeImg.color = Color.Lerp(FadeImg.color, new Color(0,0,0,0), fadeSpeed * Time.deltaTime);
        loadingText.color = Color.Lerp(FadeImg.color, new Color(1, 1, 1, 0), fadeSpeed * Time.deltaTime);


    }
    


    
}
