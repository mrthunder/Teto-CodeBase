using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class UIButtonSounds : MonoBehaviour, ISelectHandler
{
    public bool m_bMuteSelection = false;

    public void OnSelect(BaseEventData eventData)
    {
        if (m_bMuteSelection) return;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_UI_Navigation, this.gameObject);
    }
    public void OnClick()
    {
        SoundManager.Instance.PlayEvent(SoundEvents.Play_UI_Select, this.gameObject);
    }
    public void OnReturn()
    {
        SoundManager.Instance.PlayEvent(SoundEvents.Play_UI_Return, this.gameObject);
    }
}
