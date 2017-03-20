using UnityEngine;
using UnityEngine.UI;
using System;
using Debug = UnityEngine.Debug;
using System.Text.RegularExpressions;

[System.Serializable]
public class ButtonName
{
    [Header("General Name:")]
    public string m_sButtonName = string.Empty;
    [Header("Controller:")]
    public string m_sControllerButtonName;
    public Sprite m_ControllerButton;
    [Header("Keyboard:")]
    public string m_sKeyBoardButtonName;
    public Sprite m_KeyBoardButton;

}

[RequireComponent(typeof(RectTransform)), DisallowMultipleComponent, HelpURL("https://docs.google.com/document/d/1goujeCoQ02fGBuAp7Vxn5M8IF-tWmQDhnydmnZtnr1w/edit#heading=h.4y9mcbpqyhq1")]
public class ButtonsImage : MonoBehaviour
{

    public ButtonName[] m_ButtonList;

    [Tooltip("Component that will use this")]
    public Image m_Img;

    [Tooltip("Component that will use this")]
    public Text m_Text;

    private Sprite GetImage(int index)
    {
        return (IsJoyStickConnected() ? m_ButtonList[index].m_ControllerButton : m_ButtonList[index].m_KeyBoardButton);
    }

    private string GetName(int index)
    {
        return (IsJoyStickConnected() ? m_ButtonList[index].m_sControllerButtonName : m_ButtonList[index].m_sKeyBoardButtonName);
    }

    private bool IsJoyStickConnected()
    {
        string[] names = Input.GetJoystickNames();
        return (names.Length > 0 && !string.IsNullOrEmpty(names[0]));
    }

    private int GetIndexByName(string name)
    {
        int index = -1;

        while (++index < m_ButtonList.Length)
        {
            if (m_ButtonList[index].m_sButtonName == name)
            {
                break;
            }
        }

        return index;
    }
    private void CheckErrors<T>(ref T obj, string msg)
    {
        if (m_ButtonList == null || m_ButtonList.Length == 0)
        {
            throw new System.Exception("The Button List is empty or null" + this.name);
        }

        if (obj == null)
        {
            throw new System.Exception(msg + this.name);
        }
    }

    public void SetText(string msg)
    {
        CheckErrors(ref m_Text, "The text component is not assing in the inspector");

        string pattern = "{[^#]+}";

        Match match = Regex.Match(msg, pattern);
        if (match.Success)
        {
            string matchValue = match.Value;
            string bName = matchValue.Trim('{', '}');

            int index = GetIndexByName(bName);
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(msg, GetName(index));
            m_Text.text = result;
        }
        else
        {
            Debug.LogError("The message don't have the pattern");
        }
    }
    public void SetImage(string bName)
    {
        CheckErrors(ref m_Img, "The Image component is not assing in the inspector");

        int index = GetIndexByName(bName);
        m_Img.sprite = GetImage(index);

    }

}
