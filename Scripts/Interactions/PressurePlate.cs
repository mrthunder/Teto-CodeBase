using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;
using UnityEngine.Events;

public class PressurePlate : MonoBehaviour
{
    Tweener tween;

   [Tooltip("Duration of the movement")]
    public float m_fDuration;

    public Transform m_gDoor;

    public Vector3 m_vMoveTo;

    public Vector3 m_vSize;

    public Vector3 m_vColPos = new Vector3(0, 1, 0);

    private Vector3 m_vPlateInitial;

    private Vector3 m_vDoorInitial;

    public bool m_bUseTrigger = false;

    public UnityEvent m_OnEnter = new UnityEvent();
    public UnityEvent m_OnExit = new UnityEvent();


    internal bool m_bActivate = false;

    private Collider[] colliders;
    private Collider[] prevColliders;


    [Tooltip("When the pressure plate is use with another script to open the door.")]
    /// <summary>
    /// When the pressure plate is use with another script to open the door.
    /// </summary>
    public bool m_bDontMove = false;

    [Tooltip("When this plate is the source it can control the door with other pressures plates.")]
    /// <summary>
    /// When this plate is the source it can control the door with other pressures plates.
    /// </summary>
    public bool m_bSource = false;

    public PressurePlate[] m_Plates;

    private bool m_bDoorOpen = false;

    public bool m_bDontAnimate = false;

    public bool m_bPermanent = false;

    [Tooltip("Cinematic camera, the same with light vein")]
    public GameObject m_gCamContainer;

    private SoundEvents m_Activation = SoundEvents.Play_PP_Activation;
    private SoundEvents m_Deactivation = SoundEvents.Play_PP_Deactivation;

    private SoundEvents m_StartRaiseObject = SoundEvents.Play_Small_Drag;
    private SoundEvents m_StopRaiseObject = SoundEvents.Stop_Small_Drag;


    // Use this for initialization
    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
        m_vPlateInitial = transform.position;
        if (m_gDoor != null)
        {
            m_vDoorInitial = m_gDoor.position;
        }
        prevColliders = new Collider[0];
        Pressure();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_bPermanent && m_bActivate) return;
        Pressure();
        if (m_bSource)
        {
            if (m_Plates != null)
            {
                var result = CheckAllPlates();
                if (result && !m_bDoorOpen)
                {
                    DoorMovement(true);
                    m_bDoorOpen = true;
                }
                else if (!result && m_bDoorOpen)
                {
                    DoorMovement(false);
                    m_bDoorOpen = false;
                }
            }
        }
    }

    bool CheckAllPlates()
    {
        if (!m_bActivate) return false;

        foreach (PressurePlate plate in m_Plates)
        {
            if (!plate.m_bActivate)
            {
                return false;
            }
        }

        return true;
    }

    void Pressure()
    {
        colliders = Physics.OverlapBox(transform.position + m_vColPos, m_vSize/2,Quaternion.identity,LayerMask.GetMask("Player"));
       
        bool hasObjects = HasObjects();
        if (hasObjects && !m_bActivate)
        {
            
            OnEnter();
        }
        if (!hasObjects && m_bActivate)
        {
            OnExit();
        }

        foreach (Collider col in colliders.Except(prevColliders).ToArray())
        {
            col.transform.SetParent(transform, true);
        }

        foreach (Collider col in prevColliders.Except(colliders).ToArray())
        {
            if (col == null)
                continue;
            col.transform.SetParent(null, true);
        }

        prevColliders = colliders;
    }

    bool HasObjects()
    {
        foreach (Collider c in colliders)
        {
            if (c.gameObject == gameObject)
                continue;
            return true;
        }
        if(colliders.Length >1)
        {
            print("hey" + colliders[1].name);
        }
        
        return false;
    }

    public void OnEnter()
    {


        SoundManager.Instance.PlayEvent(m_Activation, this.gameObject);
        if(m_bPermanent && !m_bActivate)
        {
            if(m_gCamContainer!=null)
            {
                if (!m_gCamContainer.activeInHierarchy)
                {
                    m_gCamContainer.SetActive(true);
                }
            }
        }
        m_bActivate = true;
        if(!m_bDontAnimate)
        {
            Vector3 posDown = m_vPlateInitial - Vector3.up * 0.1f;
            tween = transform.DOMove(posDown, m_fDuration);
        }
       
       
        if (!m_bDontMove)
        {
            if (!m_bSource)
            {
                if (m_bUseTrigger)
                {
                    m_OnEnter.Invoke();
                }
                else
                {
                    DoorMovement(true);
                }
                
            }

        }
        
       
    }

    private void DoorMovement(bool open)
    {
        if (open)
        {
           
            m_gDoor.DOMove(m_gDoor.position + m_vMoveTo, m_fDuration).OnStart(()=> 
            {
                SoundManager.Instance.PlayEvent(m_StopRaiseObject, m_gDoor.gameObject);
                SoundManager.Instance.PlayEvent(m_StartRaiseObject, m_gDoor.gameObject);
            }).OnComplete(()=>SoundManager.Instance.PlayEvent(m_StopRaiseObject, m_gDoor.gameObject));
        }
        else
        {
            
            m_gDoor.DOMove(m_vDoorInitial, m_fDuration).OnStart(() =>
            {
                SoundManager.Instance.PlayEvent(m_StopRaiseObject, m_gDoor.gameObject);
                SoundManager.Instance.PlayEvent(m_StartRaiseObject, m_gDoor.gameObject);
            }).OnComplete(() => SoundManager.Instance.PlayEvent(m_StopRaiseObject, m_gDoor.gameObject));
        }

    }

    public void OnExit()
    {
        if (m_bPermanent && m_bActivate) return;
#if AUDIO
        SoundManager.Instance.PlayEvent(m_Deactivation, this.gameObject);
#endif
        m_bActivate = false;
        if (!m_bDontAnimate)
        {
            tween = transform.DOMove(m_vPlateInitial, m_fDuration);
        }

        if (!m_bDontMove)
        {
            if (!m_bSource)
            {
                if (m_bUseTrigger)
                {
                    m_OnExit.Invoke();
                }
                else
                {
                    DoorMovement(false);
                }
                
            }
        }

        
    }

#if CODEDEBUG
    
    public void OnDrawGizmos()
    {
        Vector3 center = transform.position + m_vColPos;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, m_vSize);
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(center, m_vSize);

        
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (!m_bDontMove || m_bSource)
        {
            if (m_gDoor != null)
            {

                Vector3 destination = m_gDoor.position + m_vMoveTo;
                Gizmos.DrawLine(m_gDoor.transform.position, destination);
                Mesh mesh = m_gDoor.GetComponent<MeshFilter>().sharedMesh;
                if (mesh != null)
                {
                    Gizmos.DrawMesh(mesh, 0, destination, m_gDoor.rotation, m_gDoor.lossyScale);
                }

            }
        }
    }
#endif
}
