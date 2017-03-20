using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class PulseToggle : MonoBehaviour, IPulseInteract
{




    private bool isActivate = false;

    public UnityEvent m_OnPulse = new UnityEvent();
    public UnityEvent m_OnPulseExit = new UnityEvent();

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
    }

    public void OnPulseEnter(float pulsePower)
    {
        if (isActivate) return;
        isActivate = true;
        m_OnPulse.Invoke();
    }


    public void OnPulseExit()
    {
        if (isActivate) return;
        m_OnPulseExit.Invoke();
    }
}
