using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

/// <summary>
/// Trap that absorb wolves
/// </summary>
[AddComponentMenu("Puzzle/Circular Trap Puzzle/Circular Trap")]
public class CircularTrap : Trap
{

    /// <summary>
    /// How many enemies will be trap.
    /// </summary>
    [Tooltip("How many enemies will be trap.")]
    public int m_iMaxEnemyTrap = 0;

    /// <summary>
    /// Current number of enemies that were trapped
    /// </summary>
    private int m_iCurrentEnemiesTrapped = 0;

    /// <summary>
    /// This will be instantiate every time a wolf gets absorb.
    /// </summary>
    [Tooltip("This will be instantiate every time a wolf gets absorb.")]
    public GameObject m_AbsorbEffect;

    /// <summary>
    /// How long the effect will take
    /// </summary>
    [Tooltip("How long the effect will take")]
    public float m_fEffectDuration;

    /// <summary>
    /// Radius of effect
    /// </summary>
    [Tooltip("Radius of effect")]
    public float m_fRadius = 2f;

    /// <summary>
    /// Layer that will be looking for the enemies.
    /// </summary>
    private int m_iLayer;

    /// <summary>
    /// The light beam that indicates the status.
    /// </summary>
    public ParticleSystem m_LightBeam;

    // Use this for initialization
    void Start()
    {

        if (m_iMaxEnemyTrap <= 0)
        {
            throw new NotSupportedException("Max Enemy Trap cannot be less then 0.");
        }

        if (m_fRadius == 0)
        {
            throw new NotSupportedException("Radius cannot be 0");
        }

        m_iLayer = LayerMask.GetMask("Enemies");

    }


    public override void Activate()
    {
        if (!isActivated)
        {
            AbsorbWolves();
            CheckTrapStatus();
        }
    }

    /// <summary>
    /// Check is any of is inside the trap and absorb them
    /// </summary>
    private void AbsorbWolves()
    {
        Collider[] col = Physics.OverlapSphere(transform.position, m_fRadius, m_iLayer);
        foreach(Collider c in col)
        {
            if (m_iCurrentEnemiesTrapped == m_iMaxEnemyTrap) break;
            Wolf wolf = c.GetComponent<Wolf>();
            if (wolf == null) continue;
            ShowEffect(wolf);
            m_iCurrentEnemiesTrapped++;
        }
        
    }

    /// <summary>
    /// Display the effect 
    /// </summary>
    /// <param name="wolf">The target</param>
    private void ShowEffect(Wolf wolf)
    {
        if (wolf == null)
        {
            throw new NullReferenceException("You cannot assing an effect to a null wolf.");
        }
        if (m_AbsorbEffect != null)
        {
            if (m_fEffectDuration == 0)
            {
                Debug.LogWarning("Effect duration is equal to zero.", this.gameObject);
            }
            GameObject obj = Instantiate(m_AbsorbEffect, wolf.transform.position+Vector3.up, wolf.transform.rotation) as GameObject;
            obj.SetActive(true);
            obj.transform.DOMove(transform.position, 2f).OnComplete(() =>Destroy(obj));
            
        }
        Destroy(wolf.gameObject);
    }

    /// <summary>
    /// Status of the Trap
    /// </summary>
    /// <returns>Percentage of completion</returns>
    public float GetPercentage()
    {
        return Mathf.InverseLerp(0, m_iMaxEnemyTrap, m_iCurrentEnemiesTrapped) * 100;
    }

    /// <summary>
    /// Check if the trap still can be activated.
    /// </summary>
    private void CheckTrapStatus()
    {
        float percent = GetPercentage();
        if(m_LightBeam!=null)
        {
            m_LightBeam.startLifetime = (percent / 100) * 40;
        }
        

#if UNITY_EDITOR
        this.Print3D(percent.ToString() + "%");
#endif
        if (percent == 100)
        {
            m_bIsActivated = true;
        }
    }



#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_fRadius);
    }
#endif


}
