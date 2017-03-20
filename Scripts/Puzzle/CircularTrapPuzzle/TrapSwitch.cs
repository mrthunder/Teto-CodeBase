using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Activate traps using the pulse
/// </summary>
[AddComponentMenu("Puzzle/Trap Switch")]
public class TrapSwitch : MonoBehaviour, IPulseInteract
{
    public delegate void ActivateTrapHandler();
    public event ActivateTrapHandler ActivateTrap;

    public Trap[] m_Traps = new Trap[0];

    // Use this for initialization
    void Start()
    {
        foreach (Trap trap in m_Traps)
        {
            ActivateTrap += trap.Activate;
        }
    }


    public void OnPulseEnter(float distance)
    {
        ActivateTrap();
    }

    public void OnPulseExit()
    {

    }

    void OnDestroy()
    {
        foreach (Trap trap in m_Traps)
        {
            ActivateTrap -= trap.Activate;
        }
    }
}
