using UnityEngine;
using System.Collections;

public class TitleScreen : UIScreen {



    void Start()
    {
        StartCoroutine(TitleScreenPlay());
    }

    IEnumerator TitleScreenPlay()
    {        
        yield return new WaitForSeconds(2);        
        ScreenManager.Instance.Show<MenuScreen>();
    }

}
