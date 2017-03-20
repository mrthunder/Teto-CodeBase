using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;


public class Vine : MonoBehaviour, IPulseInteract, IVeinInteract
{

    Renderer render;
    //BoxCollider col;
    Collider meshcol;

    [Tooltip("Show when the light vein activated")]
    public bool m_bLightVein = false;

    [Tooltip("Show and retract with the pulse")]
    public bool m_bPulse = false;

    private bool m_bIsActivated = false;

    Vector3 m_colliderSizeMax;
    Vector3 m_center;

    public bool m_bTurnCollider = false;

    Vector4 m_ShaderPos;

    public bool DoneGrowing = false;

    [Tooltip("If you use the lightvein to show this vine, and there is a vine that will grow before, add the vine here.")]
    public Vine PreviousVine;

    public float m_fGrowSpeed = 0.05f;

    public ParticleSystem m_VineParticle;

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
        render = GetComponent<Renderer>();
        if(render == null)
        {
            render = GetComponentInChildren<Renderer>();
        }
        if(render!=null)
        {
            Vector3 size = new Vector3(0, 0, (m_bTurnCollider ? -1 : 1) * render.bounds.size.z / 2);
            m_ShaderPos = transform.position + size;
            m_colliderSizeMax = render.bounds.size;
            m_center = new Vector3(0, 0, (m_bTurnCollider ? -1 : 1) * m_colliderSizeMax.z / 2);
            
        }
        m_VineParticle = GetComponentInChildren<ParticleSystem>();

        meshcol = GetComponent<MeshCollider>();
        if(meshcol==null)
        {
            meshcol = GetComponentInChildren<MeshCollider>();
        }
        if(meshcol!=null)
        {
            meshcol.enabled = false;
        }
       
        render.material.SetVector("_LightVein", Vector4.zero);


    }

   

    public void OnPulseEnter(float distance)
    {
        if (m_bIsActivated || !m_bPulse) return;
        m_bIsActivated = true;
        if(m_VineParticle)
        {
            m_VineParticle.gameObject.SetActive(false);
        }
        StartCoroutine(PulseEnter(distance));
    }

    IEnumerator PulseEnter(float distance)
    {
        yield return null;
        InteractMaterial();
    }

    public void OnPulseExit()
    {
        
    }


    private void InteractMaterial(bool show = true)
    {
        StartCoroutine(GrowVines());
    }

    private IEnumerator GrowVines()
    {
        meshcol.enabled = true;
        float grow = 0;
        float max = render.bounds.size.z;
        if(m_bLightVein && PreviousVine!= null)
        {
            yield return new WaitUntil(() => PreviousVine.DoneGrowing);
        }
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Vine_Bridge, this.gameObject);
        while(grow < max)
        {

            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

            grow+= m_fGrowSpeed;

            propertyBlock.SetVector("_LightVein", m_ShaderPos);

            propertyBlock.SetFloat("_Range", grow);
            render.SetPropertyBlock(propertyBlock);
            yield return null;
        }
        SoundManager.Instance.PlayEvent(SoundEvents.Stop_Vine_Bridge, this.gameObject);

        DoneGrowing = true;
        render.EnviromentTransition(true);
    }

   
    public void OnLightVeinInteract(float distance)
    {
        if (m_bIsActivated || !m_bLightVein) return;
        m_bIsActivated = true;
        StartCoroutine(PulseEnter(distance));
    }
}
