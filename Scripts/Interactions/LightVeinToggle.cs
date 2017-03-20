using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
using UnityEngine.Events;


public class LightVeinToggle : MonoBehaviour, IVeinInteract
{

    private bool isActivate = false;

    public UnityEvent m_OnLightVein = new UnityEvent();

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
    }

    public void OnLightVeinInteract(float distance)
    {
        if (isActivate) return;
        isActivate = true;
        m_OnLightVein.Invoke();
    }
}
