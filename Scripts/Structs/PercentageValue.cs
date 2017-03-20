using UnityEngine;
using System.Collections;

/// <summary>
/// Returns the value correspond to the value
/// </summary>
[System.Serializable]
public struct PercentageValue {

    public enum PercentageReturn {Percentage,Value}

    public Percent[] m_fPercentages;
   

    public PercentageValue(params Percent[] percentage)
    {
        this.m_fPercentages = percentage;
        
    }


    public float GetValue(float percentage)
    {
        int index = -1;
        //percentage = (percentage <= 1 ? percentage * 100 : percentage);

        for(int i = 0; i < m_fPercentages.Length;i++)
        {
            if(i == 0)
            {
                if (percentage > 0 && percentage <= m_fPercentages[i].m_fPercent)
                {
                    index = i;
                    break;
                }
                continue;
            }
            if(percentage > m_fPercentages[i-1].m_fPercent && percentage<= m_fPercentages[i].m_fPercent)
            {
                index = i;
                break;
            }
        }
        if(index == -1)
        {
            index = m_fPercentages.Length - 1;
        }

        return this[PercentageReturn.Value, index];
    }

    public float this[PercentageReturn returnType,int key]
    {
        get
        {
            if(returnType == PercentageReturn.Percentage)
            {
                return m_fPercentages[key].m_fPercent;
            }
            return m_fPercentages[key].m_fValue;
        }
        set
        {
            if (returnType == PercentageReturn.Percentage)
            {
                if(value>100 || value < 0)
                {
                    Debug.LogWarning("The Percentage value have to be between 0 to 100");
                }

                float percent = Mathf.Clamp(value, 0, 100);
                
                m_fPercentages[key].m_fPercent = percent;
            }
            else
            {
                m_fPercentages[key].m_fValue = value;
            }
            
        }
    }

}
