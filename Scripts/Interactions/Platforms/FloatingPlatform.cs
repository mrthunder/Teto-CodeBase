using UnityEngine;
using System.Collections;
using DG.Tweening;
using System;

[AddComponentMenu("Platforms/Floating Platform")]
public class FloatingPlatform : MonoBehaviour, IPulseInteract, IVeinInteract
{

    /// <summary>
    /// If will be affect by the pulse ability
    /// </summary>
    [Header("Interactions:", order = 0)]
    public bool m_bPulse = false;

    /// <summary>
    /// Max distance that platform can go when activated by the pulse.
    /// </summary>
    public Vector3 m_vMaxPulseDistance = Vector3.zero;

    /// <summary>
    /// If will be affect by the Light Vein
    /// </summary>
    public bool m_bLightVein = false;


    /// <summary>
    /// How many interactions is necessary to activate this
    /// </summary>
    [Tooltip("How many interactions is necessary to activate this.")]
    public int m_iLightVeinInteractions = 0;

    /// <summary>
    /// Effect in seconds
    /// </summary>
    [Tooltip("Effect in seconds")]
    public float m_fLightVeinEffectDelay = 0;

    /// <summary>
    /// Actual number of interactions.
    /// </summary>
    /// <example>
    /// When the light vein activate this platform, one interaction is added until is equal to <see cref="m_iLightVeinInteractions"/>
    /// </example>
    private int m_iInteractions = 0;

    /// <summary>
    /// Positions that the platform will go based on <see cref="m_iInteractions"/>
    /// </summary>
    public Vector3[] m_vNextPos;

    /// <summary>
    /// Index of the <see cref="m_vNextPos"/>
    /// </summary>
    private int m_iIncrement = 0;

    /// <summary>
    /// Initial position of the platform
    /// </summary>
    private Vector3 m_vInitialPosition;

    /// <summary>
    /// Duration of the tween move
    /// </summary>
    [Range(1, 10)]
    public float m_fDuration = 1;

    /// <summary>
    /// If the tween will loop
    /// </summary>
    public bool m_bLoop = false;

    /// <summary>
    /// Tweener of the pulse movement
    /// </summary>
    private Tweener m_twPulseMove;

    /// <summary>
    /// If the platform have already been activated by a <see cref="LightVein"/> the pulse will not have any more effect
    /// </summary>
    private bool m_bIsActivated = false;

    /// <summary>
    /// When the movement is finish (<see cref="m_twPulseMove"/> or <seealso cref="m_MoveSequence"/>) this will be true, otherwise will be false.
    /// </summary>
    private bool m_bIsMovementFinish = true;

    /// <summary>
    /// If the Platform will shake while move
    /// </summary>
    public bool m_bShake = false;

    /// <summary>
    /// One of the params of the <see cref="DOTween.Shake(DG.Tweening.Core.DOGetter{Vector3}, DG.Tweening.Core.DOSetter{Vector3}, float, float, int, float, bool, bool)"/>
    /// </summary>
    public float m_fShakeStrenth = 10;

    /// <summary>
    /// Sequence of the movement. Use in the Coroutine
    /// </summary>
    Sequence m_MoveSequence;

    /// <summary>
    /// Renderer that will change the color of the texture
    /// </summary>
    private Renderer m_render;

    //Sounds
    internal SoundEvents m_StartLoop = SoundEvents.Play_Floating_Platform;

    internal SoundEvents m_EndLoop = SoundEvents.Stop_Floating_Platform;

    internal SoundEvents m_RaisePlatform = SoundEvents.Play_Raising_FP;

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
        m_vInitialPosition = transform.position;
    }

    public void Start()
    {
        m_render = GetComponent<Renderer>();
        if (!m_render)
        {
            m_render = GetComponentInChildren<Renderer>();
        }
        SoundManager.Instance.PlayEvent(m_StartLoop, this.gameObject);
        m_MoveSequence = DOTween.Sequence();
    }



    IEnumerator MovePlatform()
    {
        m_MoveSequence.Kill();


        m_bIsMovementFinish = false;

        m_MoveSequence.Append(transform.DOMove(m_vInitialPosition + m_vMaxPulseDistance, m_fDuration).OnStart(() => SoundManager.Instance.PlayEvent(m_RaisePlatform, this.gameObject)));
        if (m_bShake)
        {
            m_MoveSequence.Insert(0, transform.DOShakeRotation(m_fDuration, m_fShakeStrenth));
        }


        yield return null;


    }

    public void OnLightVeinInteract(float distance)
    {
        if (m_iInteractions < m_iLightVeinInteractions)
        {
            m_iInteractions++;
            return;
        }
        if (m_bLightVein == false || m_vNextPos == null) return;
        if (m_bIsActivated == true && m_iIncrement >= m_vNextPos.Length) return;

        if (m_iIncrement >= m_vNextPos.Length)
        {
            return;
        }
        StartCoroutine(LightVeinEffect());

    }

    IEnumerator LightVeinEffect()
    {
        yield return new WaitForSeconds(m_fLightVeinEffectDelay);
        if (m_render)
        {
            MaterialPropertyBlock property = new MaterialPropertyBlock();
            property.SetFloat("_Show", 1);
            m_render.SetPropertyBlock(property);
        }
        if (m_bLoop)
        {

            transform.DOMove(m_vInitialPosition + m_vNextPos[m_iIncrement], m_fDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).OnStart(() => SoundManager.Instance.PlayEvent(m_RaisePlatform, this.gameObject));
        }
        else
        {

            transform.DOMove(m_vInitialPosition + m_vNextPos[m_iIncrement], m_fDuration).OnStart(() => SoundManager.Instance.PlayEvent(m_RaisePlatform, this.gameObject));

        }
        m_iIncrement++;
        m_bIsActivated = true;
    }

    public void OnPulseEnter(float pulsePower)
    {
        if (m_bPulse == false || m_vNextPos == null) return;
        if (m_bIsActivated == true) return;
        StartCoroutine(OnPulse());
    }

    IEnumerator OnPulse()
    {
        if (m_bLoop)
        {
            if (m_twPulseMove != null)
            {
                m_twPulseMove.Kill();
                m_twPulseMove = null;
            }
            else
            {
                m_twPulseMove = transform.DOMove(m_vInitialPosition + m_vMaxPulseDistance, m_fDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            }

        }
        else
        {
            if (m_bIsMovementFinish)
            {
                yield return MovePlatform();
            }
            else
            {
                StopAllCoroutines();
                if (m_MoveSequence != null && m_MoveSequence.IsPlaying())
                {
                    m_twPulseMove.Kill(false);
                    yield return MovePlatform();
                }
            }

        }
    }
    public void OnPulseExit()
    {
        if (m_bIsActivated == true) return;
        StartCoroutine(BackToInit());
    }
    IEnumerator BackToInit()
    {
        m_MoveSequence.Append(transform.DOMove(m_vInitialPosition, m_fDuration));
        yield return m_MoveSequence.WaitForCompletion();
        m_bIsMovementFinish = true;
    }
    public void OnDrawGizmosSelected()
    {

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (mesh != null)
        {
            Gizmos.color = Color.red;

            Vector3 destination = Vector3.zero;
            if (m_vNextPos != null)
            {
                for (int i = 0; i < m_vNextPos.Length; i++)
                {
                    destination = transform.position + m_vNextPos[i];
                    Gizmos.DrawMesh(mesh, 0, destination, transform.rotation, transform.lossyScale);
                }

            }

            if (m_vMaxPulseDistance != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                destination = transform.position + m_vMaxPulseDistance;
                Gizmos.DrawMesh(mesh, 0, destination, transform.rotation, transform.lossyScale);
            }
        }
    }


    public void OnCollisionEnter(Collision collision)
    {
        PlayerContoller pc = collision.gameObject.GetComponent<PlayerContoller>();
        if (pc != null)
        {
            pc.transform.SetParent(transform, true);
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        PlayerContoller pc = collision.gameObject.GetComponent<PlayerContoller>();
        if (pc != null)
        {
            pc.transform.SetParent(null, true);
        }
    }

   
}
