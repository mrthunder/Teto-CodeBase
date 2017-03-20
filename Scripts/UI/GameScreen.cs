using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : UIScreen
{
    Text m_CollectableText;
    PlayerContoller m_Player = null;
    int totalCollectables = 0;

    public override void ShowScreen()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1;
        StartCoroutine(SetupCollectablesHud());  
    }

    private IEnumerator SetupCollectablesHud()
    {
        while(m_Player == null)
        {
            m_Player = FindObjectOfType<PlayerContoller>();
            yield return new WaitForSeconds(1);
        }
        m_CollectableText = GetComponentInChildren<Text>();
        m_Player = FindObjectOfType<PlayerContoller>();
        totalCollectables = FindObjectsOfType<Collectables>().Length;
        m_CollectableText.text = m_Player.CollectablesCollected + " of " + totalCollectables + " Collectables Found!";
    }

    // Update is called once per frame
    void Update ()
    {
	    if (Input.GetKeyDown(KeyCode.P) || Input.GetButtonDown("Start"))
        {
           ScreenManager.Instance.Show<PauseGame>();
        }
    }

    public void DisplayTextOn()
    {
        m_CollectableText.text = m_Player.CollectablesCollected + " of " + totalCollectables + " Collectables Found!";
        m_CollectableText.enabled = true;
    }

    public void DisplayTextOff()
    {
        m_CollectableText.enabled = false;
    }
}
