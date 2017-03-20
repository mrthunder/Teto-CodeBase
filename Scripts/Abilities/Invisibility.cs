using UnityEngine;


public class Invisibility : Abilities
{



    
    ///<summary>
    ///Tells if the player is invisible or not.
    ///</summary>
    internal bool isInvisible = false;

    [Header("Doppel"), SerializeField]
    ///<summary>
    ///Distance that the charcter will go back
    ///</summary>
    private float m_fBackStepDistance = 5f;

    [SerializeField]
    ///<summary>
    ///Prefab that will be instantiate in the same position of the player
    ///</summary>
    private GameObject m_gDoppelPrefab;

    [SerializeField]
    ///<summary>
    ///Durantion of the moviment in the DoTween
    ///</summary>
    private float m_fBackStepDuration = 2f;

    [SerializeField,Tooltip("Prefab/Particle/Particle_Doppel")]
    /// <summary>
    /// Particle Effect that is instantiate when the character moves.
    ///</summary>
    private ParticleSystem m_psSmokeScreen;

    SkinnedMeshRenderer m_Renderer;

    internal SoundEvents m_DoppleSound = SoundEvents.Play_Teto_Dopple;

    public override void UseAbility(Animator anim, GameObject unit, Rigidbody rb)
    {
        if (m_bOnCoolDown) return;
        if (!HasEnergy())
        {
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Low_Energy, this.gameObject);
            return;
        }
        base.UseAbility(anim, unit, rb);

#if UNITY_EDITOR
        //if (AnalyticsManager.Instance != null)
        //{
        //    AnalyticsManager.Instance.SetDopplerData();
        //}
#endif

        SoundManager.Instance.PlayEvent(m_DoppleSound, this.gameObject);


        if (!isInvisible)
        {
            if(m_Renderer==null)
            {
                m_Renderer = transform.Find("Teto_GameReady/Teto1").GetComponent<SkinnedMeshRenderer>();
            }

            m_Renderer.materials[0].shader = Shader.Find("Shader Forge/TetoBodyTransparent");

            



            CreateDoppel(unit);

            JumpBack(unit);

            isInvisible = true;
        }
        else
        {
           

           
        }

    }

    protected override void CoolDownFinish()
    {

        m_Renderer.materials[0].shader = Shader.Find("Shader Forge/TetoBody");

       
        isInvisible = false;
    }

    private void CreateDoppel(GameObject unit)
    {
        if (m_gDoppelPrefab == null)
        {
            GameObject doppelObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            doppelObject.name = "Teto(Doppel)";
            doppelObject.transform.position = unit.transform.position + Vector3.up * 2f;
           
            Destroy(doppelObject, m_fCoolDownTime);
        }
        else
        {
            
            GameObject doppelObject = Instantiate(m_gDoppelPrefab, unit.transform.position+Vector3.up, unit.transform.rotation) as GameObject;
            doppelObject.name = "Teto(Doppel)";
            doppelObject.transform.localScale = new Vector3(1, 1, 1);
            GameManager.Instance.m_gPlayerDoppel = doppelObject.GetComponent<Unit>();
            Destroy(doppelObject, m_fCoolDownTime);
        }
    }

    private void JumpBack(GameObject unit)
    {
        Vector3 destination = -unit.transform.forward * m_fBackStepDistance;
        bool cannotMove = true;//IsSomethingBlocking(ref destination, unit, m_fBackStepDistance, -unit.transform.forward, true);

        if (!cannotMove)
        {

            
        }
        m_psSmokeScreen.gameObject.SetActive(true);
        RelativeQuickMove(unit, destination, 5f, m_fBackStepDuration, EndEffect);

    }

    private void EndEffect()
    {
        m_psSmokeScreen.gameObject.SetActive(false);
    }

}
