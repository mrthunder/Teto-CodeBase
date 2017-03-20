using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Percent {

    public float m_fPercent;
    public float m_fValue;

    public Percent (float per, float val)
    {
        this.m_fPercent = per;
        this.m_fValue = val;
    }
}
