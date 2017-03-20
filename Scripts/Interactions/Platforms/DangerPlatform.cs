using UnityEngine;
using System.Collections;
using DG.Tweening;

public enum PlatformState { Stationary, Shaking, Falling, Spawning };

[AddComponentMenu("Platforms/Danger Platform")]
public class DangerPlatform : MonoBehaviour
{

    [Header("Color Variant:")]
    [Tooltip("Color usage to show player when is safe and when is dangerous")]
    public Gradient m_GradientColor = new Gradient();

    [Tooltip("How long it will be take to change from a color to another"), Range(1f, 10f)]
    public float m_fColorChangeDuration = 2f;

    public bool m_bWillLerp = false;
    /// <summary>
    /// Index usage to show the colors
    /// </summary>
    internal float m_fColorChangeIndex = 0;

    private bool m_bHasCollide = false;

    private Renderer m_Renderer;

    [Header("Shake:")]
    [Tooltip("How strong is going to be the shake"), Range(0f, 10f)]
    public float m_fShakeStrength = 2f;

    [Tooltip("How long will be the shake movement"), Range(0f,10f)]
    public float m_fShakeDuration = 2f;

    

    [Header("Fall Movement:")]
    [Tooltip("Where the platform will fall")]
    public Vector3 m_vEndPosition = Vector3.zero;
   
    [Range(0, 10), Tooltip("Duration of the fall movement")]
    public float m_fFallDuration = 1f;

    /// <summary>
    /// The platform initial position
    /// </summary>
    private Vector3 _initialPosition = Vector3.zero;

    /// <summary>
    /// The property that only exposes the initial position
    /// </summary>
    internal Vector3 m_vInitialPosition
    {
        get
        {
            return _initialPosition;
        }
    }

    [Header("Spawn:")]
    [Tooltip("How long will take in seconds for the platform reappear"), Range(0f, 10f)]
    public float m_fSpawnDuration = 2f;

    /// <summary>
    /// State of the platform
    /// </summary>
    private PlatformState _state = PlatformState.Stationary;

    /// <summary>
    /// What the platform is doing
    /// </summary>
    internal PlatformState m_State
    {
        get
        {
            return _state;
        }
    }

    /// <summary>
    /// The collider will disappear when the plaform vanish.
    /// </summary>
    private Collider col;

    MeshFilter mesh;

    // Use this for initialization
    void Start()
    {
        m_Renderer = GetComponent<Renderer>();
        col = GetComponent<Collider>();
        _initialPosition = transform.position;
        mesh = GetComponent<MeshFilter>();
        MaterialPropertyBlock property = new MaterialPropertyBlock();
        property.SetColor("_EmissionColor", m_GradientColor.Evaluate(m_fColorChangeIndex));
        m_Renderer.SetPropertyBlock(property);
        StartCoroutine(Action());
    }

    /// <summary>
    /// Color will change until player step on the platform and the platform is in any dangerous color
    /// </summary>
    IEnumerator ChangeColors()
    {
        _state = PlatformState.Stationary;

        m_fColorChangeIndex = 0;
        Tween t = DOTween.To(() => m_fColorChangeIndex, x => m_fColorChangeIndex = x, 1, m_fColorChangeDuration).SetLoops(-1, LoopType.Yoyo).ChangeStartValue(0).ChangeEndValue(1);
        


        while (true)
        {
            if (m_bHasCollide)
            {
                if(m_bWillLerp && m_fColorChangeIndex > 0.5)
                {
                    break;
                }
                else if(!m_bWillLerp)
                {
                    break;
                }
                
            }
            if(m_bWillLerp)
            {
                MaterialPropertyBlock property = new MaterialPropertyBlock();
                property.SetColor("_EmissionColor", m_GradientColor.Evaluate(m_fColorChangeIndex));
                m_Renderer.SetPropertyBlock(property);
            }
           
            yield return null;
        }
        t.Kill();
    }
    /// <summary>
    /// Platform will shake to warn player
    /// </summary>
    IEnumerator WarningShake()
    {
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Weak_Platform, this.gameObject);
        _state = PlatformState.Shaking;
        yield return transform.DOShakePosition(m_fShakeDuration, m_fShakeStrength).WaitForCompletion();
    }

    /// <summary>
    /// The platform will fall to a set position
    /// </summary>
    IEnumerator FallPlatform()
    {
        _state = PlatformState.Falling;
        yield return transform.DOMove(m_vEndPosition, m_fFallDuration).SetRelative(true).WaitForCompletion();
    }

    /// <summary>
    /// After a fill seconds the platform will respawn.
    /// </summary>
    IEnumerator Spawn()
    {
        _state = PlatformState.Spawning;
        m_Renderer.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(m_fSpawnDuration);
        transform.position = m_vInitialPosition;
        m_Renderer.enabled = true;
        col.enabled = true;
    }

    /// <summary>
    /// All the action in a loop
    /// </summary>
    IEnumerator Action()
    {
        while (Application.isPlaying)
        {
            yield return ChangeColors();
            yield return WarningShake();
            yield return FallPlatform();
            yield return Spawn();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        m_bHasCollide = true;
    }

    public void OnTriggerExit(Collider other)
    {
        m_bHasCollide = false;
    }





#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (m_vEndPosition != Vector3.zero)
        {
            
            if (mesh != null)
            {

                Gizmos.DrawMesh(mesh.sharedMesh, 0, m_vEndPosition + transform.position, transform.rotation, transform.lossyScale);
            }
            else
            {
                mesh = GetComponent<MeshFilter>();
            }

        }

    }
#endif

    void OnDestoy()
    {
        StopAllCoroutines();
    }
}
