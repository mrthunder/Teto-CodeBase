using UnityEngine;
using UnityEngine.UI;

public class CollectablesHud : MonoBehaviour
{
    Text            m_CollectableText;
    PlayerContoller m_Player;
    int             totalCollectables;

	// Use this for initialization
	void Awake ()
    {
        m_CollectableText       = GetComponent<Text>();
        m_Player                = FindObjectOfType<PlayerContoller>();
        totalCollectables       = FindObjectsOfType<Collectables>().Length;
        m_CollectableText.text  = m_Player.CollectablesCollected + " of " + totalCollectables + " Collectables Found!";
        print("!!!!!!!!!!!!!!!!!!!!!!!!! " + m_CollectableText.text);
    }

    public void DisplayTextOn()
    {
        m_CollectableText.text      = m_Player.CollectablesCollected + " of " + totalCollectables + " Collectables Found!";
        m_CollectableText.enabled   = true;
    }

    public void DisplayTextOff()
    {
        m_CollectableText.enabled   = false;
    }
}
