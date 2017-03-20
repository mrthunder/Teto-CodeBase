using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public  class UIScreen : MonoBehaviour
{

    public GameObject m_gFirstButtonSelect;
    private EventSystem m_EventSystem;

    public void OnEnable()
    {
        if(m_EventSystem == null)
        {
            m_EventSystem = FindObjectOfType<EventSystem>();
        }
 
        m_EventSystem.SetSelectedGameObject(m_gFirstButtonSelect);
    }

    public virtual void ShowScreen()
    {

    }
    
}
