using UnityEngine;
using System.Collections;
using DG.Tweening;

public class EnvironmentalTree : MonoBehaviour, IPulseInteract, IVeinInteract {

    public Renderer[] render;
    //BoxCollider col;
    public Transform[] Leafes;

    [Tooltip("Show when the light vein activated")]
    public bool m_bLightVein = false;

    [Tooltip("Show and retract with the pulse")]
    public bool m_bPulse = false;

    private bool m_bIsActivated = false;

    Vector3 m_colliderSizeMax;
    Vector3 m_center;

    public bool m_bTurnCollider = false;

    Vector4[] m_ShaderPos;

    public bool DoneGrowing = false;

    [Tooltip("If you use the lightvein to show this vine, and there is a vine that will grow before, add the vine here.")]
    public Vine PreviousVine;

    public float m_fGrowSpeed = 0.05f;

    /// <summary>
    /// Delay in seconds
    /// </summary>
   [Tooltip("Delay in seconds")]
    public float m_fDelay = 0;

    public LightVein m_PrecisionGrow;

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
    }



    public void OnPulseEnter(float distance)
    {
        if (m_bIsActivated || !m_bPulse) return;
        StartCoroutine(PulseEnter(distance));
    }

    IEnumerator PulseEnter(float distance)
    {

        SoundManager.Instance.PlayEvent(SoundEvents.Play_Restoration_Tree, this.gameObject);
        yield return null;
        
        InteractMaterial();
    }

    public void OnPulseExit()
    {
        if (m_bIsActivated || !m_bPulse) return;
        StartCoroutine(PulseExit());
    }

    IEnumerator PulseExit()
    {
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Restoration_Tree, this.gameObject);
        yield return null;
        InteractMaterial(false);
    }


    private void InteractMaterial(bool show = true)
    {

    
        if (show)
        {
            StopCoroutine(RetractLeafs());
            StartCoroutine(GrowLeafs());
            
        }
        else
        {
            StopCoroutine(GrowLeafs());
            StartCoroutine(RetractLeafs());

        }

    }

    private IEnumerator GrowLeafs()
    {
        
        float grow = 0;
        
        if (m_bLightVein && PreviousVine != null)
        {
            yield return new WaitUntil(() => PreviousVine.DoneGrowing);
        }
        for(int i=0;i<Leafes.Length;i++)
        {
            Leafes[i].DOScale(1, 2);
            
        }
        DoneGrowing = true;
    }

    private IEnumerator RetractLeafs()
    {
        if (m_bLightVein && PreviousVine != null)
        {

            yield return new WaitUntil(() => !PreviousVine.DoneGrowing);
        }

        for (int i = 0; i < Leafes.Length; i++)
        {
            Leafes[i].DOScale(0, 2);
            
        }
        DoneGrowing = false;
    }

    public void OnLightVeinInteract(float distance)
    {
        if (m_bIsActivated || !m_bLightVein) return;
        m_bIsActivated = true;
        StartCoroutine(LightVeinActivation(distance));
    }

    private IEnumerator LightVeinActivation(float distance)
    {
        if (m_PrecisionGrow)
        {
            float increment = 0;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, m_PrecisionGrow.transform.position) < (increment += m_PrecisionGrow.m_fIncrement));
        }
        else
        {
            yield return new WaitForSeconds(m_fDelay);
        }
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Restoration_Tree, this.gameObject);
        yield return PulseEnter(distance);

    }
}
