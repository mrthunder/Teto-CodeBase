using UnityEngine;
using System.Collections;

public class TutorialManager : Singleton<TutorialManager>
{

    /// <summary>
    /// Current button been used
    /// </summary>
    string m_sCurrentButton = string.Empty;

    public Canvas m_canvas;

    /// <summary>
    /// Button A Image
    /// </summary>
    public GameObject m_gButtonA;
    /// <summary>
    /// Button B Image
    /// </summary>
    public GameObject m_gButtonB;
    /// <summary>
    /// Button X Image
    /// </summary>
    public GameObject m_gButtonX;
    /// <summary>
    /// Button Y Image
    /// </summary>
    public GameObject m_gButtonY;


    public void ShowButton(string name)
    {
        m_sCurrentButton = name;
        GameObject button = GetButton(name);
        if(button!=null)
        {
            button.SetActive(true);
        }
        
    }

    public void HideButton(string name)
    {
        if(string.IsNullOrEmpty(name))
        {
            name = m_sCurrentButton;
        }
        m_sCurrentButton = string.Empty;
        GameObject button = GetButton(name);
        if (button != null)
        {
            button.SetActive(false);
        }
    }

    GameObject GetButton(string name)
    {
        switch (name)
        {
            case "A":
            case "a":
                return m_gButtonA;
            case "B":
            case "b":
                return m_gButtonB;
            case "X":
            case "x":
                return m_gButtonX;
            case "Y":
            case "y":
                return m_gButtonY;
            default:
                Debug.LogError("Button not valid");
                break;
        }
        return null;
    }

    public void HideAllButtons(bool hide)
    {
        m_canvas.enabled = !hide;
    }

}
