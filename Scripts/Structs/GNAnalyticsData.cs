using UnityEngine;

/// <summary>
/// Collection of Analytics Data
/// </summary>
[System.Serializable]
public struct GNAnalyticsData
{
    public int      m_id;
    public Vector3  m_Location;
    public float    m_fHealth;
    public float    m_fEnergy;
    public float    m_fFps;
    

    public GNAnalyticsData(int id, Vector3 location, float health, float energy, float fps)
    {
        this.m_id = id;
        this.m_Location = location;
        this.m_fHealth  = health;
        this.m_fEnergy  = energy;
        this.m_fFps     = Mathf.Round(fps);
    }

    public bool PlayerDied()
    {
        if (LowHealth(0))
        {
            return true;
        }
        else return false;
    }

    public bool LowHealth(float lowValue)
    {
        if (m_fHealth <= lowValue)
        {
            return true;
        }
        else return false;
    }

    public bool LowEnergy(float lowValue)
    {
        if (m_fEnergy <= lowValue)
        {
            return true;
        }
        else return false;
    }

    public bool LowFPS(float lowValue)
    {
        if (m_fFps <= lowValue)
        {
            return true;
        }
        else return false;
    }
}
