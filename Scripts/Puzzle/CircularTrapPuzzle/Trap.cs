using UnityEngine;
using System.Collections;

/// <summary>
/// All traps share a common class that let them be activated
/// </summary>
public abstract class Trap : MonoBehaviour {

    protected bool m_bIsActivated = false;

    public bool isActivated
    {
        get
        {
            return m_bIsActivated;
        }
    }

    /// <summary>
    /// Activate the trap effect
    /// </summary>
    public abstract void Activate();
	
}
