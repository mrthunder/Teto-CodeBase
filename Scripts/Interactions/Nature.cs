using UnityEngine;
using DG.Tweening;
using System.Collections;
using System;

public class Nature : MonoBehaviour, IVeinInteract, IPulseInteract
{

    #region Status
    /// <summary>
    /// If the object already grow
    /// </summary>
    private bool _isActivate = false;
    /// <summary>
    /// Tells if the object already grow
    /// </summary>
    public bool m_bIsActivate
    {
        get
        {
            return _isActivate;
        }
    }
    #endregion

    #region Tweener
    /// <summary>
    /// A sequence of action that include change scale and change position.
    /// </summary>
    private Sequence m_Action;

    /// <summary>
    /// Maximum size
    /// </summary>
    private Vector3 m_vFinalScale;

    /// <summary>
    /// Initial position of the Object
    /// </summary>
    private Vector3 m_vInitialPosition;

    /// <summary>
    /// When the object is srink
    /// </summary>
    private Vector3 m_vFinalPosition;

    [Header("Grow")]
    [Range(1f, 10f), Tooltip("How long it will take to the object grow.")]
    public float m_fDuration = 1f;

    #endregion

    #region Effect
    private ParticleSystem m_Effect;
    private const string EFFECT_PATH = "Effect/NatureEffect";
    private Vector3 m_vEffectFinalPos;
    #endregion

    /// <summary>
    /// Delay in Seconds
    /// </summary>
    [Tooltip("Delay in Seconds")]
    public float m_fDelay = 0;

    public LightVein m_PrecisionGrow;

    // Use this for initialization
    void Start()
    {

        //gameObject.layer = LayerMask.GetMask("Interaction");

        m_Action = DOTween.Sequence();
        m_vFinalScale = transform.localScale;
        m_vInitialPosition = transform.position;

        BoxCollider col = GetComponent<BoxCollider>();
        Vector3 height = Vector3.up;
        if (col != null)
        {
            height = (Vector3.up * col.bounds.size.y / 2);
        }
        else
        {
            height = (Vector3.up * m_vFinalScale.y / 2);
        }
        m_vFinalPosition = m_vInitialPosition - height;
        try
        {
           //ParticleSystem effectPrefab = Resources.Load<ParticleSystem>(EFFECT_PATH);
           // m_Effect = Instantiate(effectPrefab, this.transform, false) as ParticleSystem;
           // m_vEffectFinalPos = m_vInitialPosition+ height;
        }
        catch
        {
            Debug.LogError("Resource Load Failed");
        }

        //Setting the object to the srink state

        transform.position = m_vFinalPosition;
        transform.localScale = Vector3.zero;


    }

    /// <summary>
    /// Stops the sequence and starts grow from the actual state
    /// </summary>
    void Grow()
    {
        m_Action.Kill();
        m_Action.Append(transform.DOMove(m_vInitialPosition, m_fDuration));
        m_Action.Insert(0, transform.DOScale(m_vFinalScale, m_fDuration));
        if (m_Effect)
            m_Action.Insert(0, m_Effect.transform.DOMove(m_vEffectFinalPos, m_fDuration));
    }

    /// <summary>
    /// Stops the sequence and starts srink from the actual state
    /// </summary>
    void Srink()
    {
        m_Action.Kill();
        m_Action.Append(transform.DOMove(m_vFinalPosition, m_fDuration));
        m_Action.Insert(0, transform.DOScale(Vector3.zero, m_fDuration));
        if (m_Effect)
            m_Action.Insert(0, m_Effect.transform.DOMove(m_vFinalPosition, m_fDuration));
    }

    public void OnLightVeinInteract(float distance)
    {
        if (m_bIsActivate) return;
        _isActivate = true;
        StartCoroutine(LightVeinActivation());
       // Grow();
    }

    IEnumerator LightVeinActivation()
    {
        if(m_PrecisionGrow)
        {
            float increment = 0;
            yield return new WaitUntil(() => Vector3.Distance(transform.position, m_PrecisionGrow.transform.position) < (increment += m_PrecisionGrow.m_fIncrement));
        }
        else
        {
            yield return new WaitForSeconds(m_fDelay);
        }
       
        Grow();
    }


    public void OnPulseEnter(float distance)
    {
        if (m_bIsActivate) return;
        Grow();
    }

    public void OnPulseExit()
    {
        if (m_bIsActivate) return;
        Srink();
    }


}
