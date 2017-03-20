using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Unit : MonoBehaviour
{
    public int m_iMaxHealth;
    [Range(1, 100)]
    public int m_iHealth            = 100;
    public int m_iLowHealth         = 33;
    public int m_iMaxEnergy         = 100;
    [Range(0,100)]
    public int m_iEnergy          = 100;
    public float m_fUnitSpeed       = 10;
    public float m_fUnitRunningSeed = 20;
    public bool m_bIsPlayer         = false;
    public bool m_bIsVisable        = true;
    public float m_fDeathHeight     = -100f;

    private float m_fGroundRaycastDist = 0.2f;
    public int m_iAttackPower   = 10;

    protected Animator anim;

    public Material m_mBodyMaterial;
    public Color m_cDamageColor = Color.red;
    public Color m_cCurrentBodyColor;

    public Renderer render;

    /// <summary>
    /// Average ground normal
    /// </summary>
    private Vector3 m_vGroundNormal = Vector3.zero;
    /// <summary>
    /// amount of data collected
    /// </summary>
    private int m_iDataCollect = 0;

   

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if(this.gameObject.GetComponent<PlayerContoller>() ==null)
        {
            m_iMaxHealth = m_iHealth;
            m_iEnergy = m_iMaxEnergy;

        }
        SoundManager.Instance.SetSwitch(Surfaces.Rock, this.gameObject);
        StartCoroutine(CheckDeath());
    }

    IEnumerator CheckDeath()
    {
        while(Application.isPlaying)
        {
            yield return new WaitUntil(() => transform.position.y <= m_fDeathHeight);
            Die();
        }
    }

    public void OnHit(int damage)
    {
        m_iHealth -= damage;

        if(this.m_bIsPlayer)
        {
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Hit, this.gameObject);
            HealthSound();
        }
        else
        {
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Wolf_Hit, this.gameObject);
        }


        FlashColorChange(true, true);
#if UNITY_EDITOR
        // *************** Update Required ***************
        print("ouch if have " + m_iHealth + "health left");
#endif
        if (m_iHealth <= 0)
        {
            Die();
        }
        
    }

    protected void HealthSound(bool healing = false)
    {
        float value = Mathf.InverseLerp(0, m_iMaxHealth, m_iHealth);
        if (m_iHealth <= 0)
        {
            value = 1;
        }
        SoundManager.Instance.SetGameParameter(GameParameters.Teto_Health, value);
        if(!healing)
        {
            if(value < 0.5f && SoundManager.Instance.CurrentState != MusicState.Health)
            {
                SoundManager.Instance.PlayTrigger(this.gameObject, "Low_Life");
                SoundManager.Instance.SetState(MusicState.Health);
            }

        }
        if(value > 0.5f)
        {
            SoundManager.Instance.SetState(MusicState.Idle);
        }
    }

    public void Heal(int amount)
    {
        m_iHealth += amount;
        if (this.m_bIsPlayer)
        {
            
            HealthSound(true);
        }
    }

    /// <summary>
    /// Flash the character
    /// </summary>
    /// <param name="state">True = On & False = Off</param>
    /// <param name="auto">True = flash and stop</param>
    protected void FlashColorChange(bool state, bool auto = false)
    {
        if (m_mBodyMaterial != null && render != null)
        {
            if(auto)
            {
                StartCoroutine(ChangeColor());
            }
            else
            {
                if (state)
                {
                    MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
                    materialBlock.SetFloat("_Flash", 1);
                    render.SetPropertyBlock(materialBlock);
                }else
                {
                    MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
                    materialBlock.SetFloat("_Flash", 0);
                    render.SetPropertyBlock(materialBlock);
                }
            }
            
        }
    }

    private IEnumerator ChangeColor()
    {
        MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
        materialBlock.SetFloat("_Flash", 1);
        render.SetPropertyBlock(materialBlock);
        yield return new WaitForSeconds(.5f);
        materialBlock.SetFloat("_Flash", 0);
        render.SetPropertyBlock(materialBlock);
       
        
    }


    public void ConsumeEnergy(int amount)
    {
        m_iEnergy -= amount;
        if(m_iEnergy <0)
        {
            m_iEnergy = 0;
        }
    }

    public float PercentageHealth()
    {
        return Percentage(m_iMaxHealth,m_iHealth);
    }
    public float PercentageEnergy()
    {
        return Percentage(m_iMaxEnergy,m_iEnergy);
    }
    private float Percentage(int max, int value)
    {
        return (float)((float)value / (float)max) ;
    }

    public virtual void Die()
    {
        StopAllCoroutines();

        if (m_bIsPlayer == false)
        {
            AiManager.Instance.RemoveMoster(this);
        }
        Destroy(gameObject);
    }

    protected void CollectNormalData()
    {
        Vector3 normal;
        if(GroundCheck(transform.forward / 2,out normal))
        {
            m_vGroundNormal =normal;
            m_iDataCollect++;
        }
        if (GroundCheck(Vector3.zero, out normal,true))
        {
            m_vGroundNormal = normal;
            m_iDataCollect++;
        }
        if (GroundCheck(-transform.forward / 2, out normal))
        {
            m_vGroundNormal = normal;
            m_iDataCollect++;
        }
    }

    protected void SetRotation()
    {
        Vector3 normal = m_vGroundNormal / m_iDataCollect;
        m_iDataCollect = 1;
        m_vGroundNormal = normal;
        
       Quaternion to =  Quaternion.FromToRotation(Vector3.forward,normal);
        transform.rotation= Quaternion.Slerp(transform.rotation, to, Time.deltaTime);
    }

    protected bool IsGrounded()
    {
        return GroundCheck(transform.forward/2) || GroundCheck(Vector3.zero,true)|| GroundCheck(-transform.forward / 2);
    }

    private bool GroundCheck(Vector3 offset, bool turn = false)
    {
        // Physics.Raycost renturn true if it hit anything, false if it didn't
        Vector3 origin = transform.position + offset;
        origin.y += 0.05f;
        float rayDistance = m_fGroundRaycastDist;
        if (turn)
        {
            rayDistance += (m_fGroundRaycastDist / 2);
        }

        Debug.DrawLine(origin, Vector3.down * rayDistance,Color.magenta);

        return Physics.Raycast(origin, Vector3.down, rayDistance,-1,QueryTriggerInteraction.Ignore);
    }
    private bool GroundCheck(Vector3 offset, out Material mat, bool turn = false)
    {
        mat = null;
        RaycastHit hit;
        // Physics.Raycost renturn true if it hit anything, false if it didn't
        Vector3 origin = transform.position + offset;
        origin.y += 0.05f;
        float rayDistance = m_fGroundRaycastDist;
        if (turn)
        {
            rayDistance += (m_fGroundRaycastDist/2);
        }
        Debug.DrawLine(origin, origin+Vector3.down * rayDistance, Color.magenta);
        bool result = Physics.Raycast(origin, Vector3.down, out hit, rayDistance, -1, QueryTriggerInteraction.Ignore);

        if (result)
        {
            Renderer render = hit.collider.GetComponent<Renderer>();
            if (render != null)
            {
                
                mat = render.sharedMaterial;
            }
        }

        return result;
    }
    private bool GroundCheck(Vector3 offset, out Vector3 normal, bool turn = false)
    {
        normal = Vector3.zero;
        RaycastHit hit;
        // Physics.Raycost renturn true if it hit anything, false if it didn't
        Vector3 origin = transform.position + offset;
        origin.y += 0.05f;
        float rayDistance = m_fGroundRaycastDist;
        if (turn)
        {
            rayDistance += (m_fGroundRaycastDist / 2);
        }
        Debug.DrawLine(origin, origin + Vector3.down * rayDistance, Color.magenta);
        bool result = Physics.Raycast(origin, Vector3.down, out hit, rayDistance, -1, QueryTriggerInteraction.Ignore);

        if (result)
        {
            normal = hit.normal;
        }

        return result;
    }

    protected bool IsGrounded(out Material mat)
    {
        return GroundCheck(transform.forward / 2,out mat) || GroundCheck(Vector3.zero, out mat,true) || GroundCheck(-transform.forward / 2, out mat);
    }
    
}