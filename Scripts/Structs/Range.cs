using UnityEngine;
using System.Collections;

/// <summary>
/// Range between two values
/// </summary>
[System.Serializable]
public struct Range {

    public float m_fMin;
    public float m_fMax;

   
    public Range (float min, float max)
    {
        this.m_fMin = min;
        this.m_fMax = max;
    }

    public float RandomBetween()
    {
        return Random.Range(m_fMin, m_fMax);
    }

    public float Lerp(float t)
    {
        t = Mathf.Clamp01(t);
        return Mathf.Lerp(m_fMin, m_fMax, t);
    }
	
}
