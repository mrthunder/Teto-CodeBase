using UnityEngine;
using System.Collections;
using DG.Tweening;

[SelectionBase, DisallowMultipleComponent, RequireComponent(typeof(BoxCollider)),HelpURL("https://docs.google.com/document/d/1goujeCoQ02fGBuAp7Vxn5M8IF-tWmQDhnydmnZtnr1w/edit#heading=h.gg7httc3ufqu")]
public class RockSlide : MonoBehaviour
{

    [Header("Prefabs")]
    /// <summary>
    /// Dust that will fall before the rocks.
    /// </summary>
    [Tooltip("Dust that will fall before the rocks")]
    public ParticleSystem m_DustParticle;

    /// <summary>
    /// Prefab of the object that will be used as a rock to block.
    /// </summary>
    [Tooltip("Prefab of the object that will be used as a rock to block")]
    public GameObject m_RocksPrefab;

    [Header("Settings:")]
    /// <summary>
    /// The amount of rock to fall.
    /// </summary>
    [Tooltip("The amount of rock to fall"), Delayed]
    public int m_iRockAmount = 10;

    /// <summary>
    /// Where the rocks will come from.
    /// </summary>
    [Tooltip("Where the rocks will come from")]
    public Vector3 m_vHeightPoint = new Vector3();

    /// <summary>
    /// If the rocks will come in an area instead of a point only.
    /// </summary>
    [Tooltip("If the rocks will come in an area instead of a point only.")]
    public bool m_bHasArea = false;

    /// <summary>
    /// When the Z axis is locked, the rock will be able to move on the X.
    /// </summary>
    [Tooltip("When the Z axis is locked, the rock will be able to move on the X")]
    public bool m_bLockZMove = false;

    /// <summary>
    /// Size of the area where the rocks can be instantiated.
    /// </summary>
    [Header("Only if hasArea is checked"), Tooltip("Size of the area where the rocks can be instantiated.")]
    public Vector3 m_vAreaSize = new Vector3();

    /// <summary>
    /// If the rocks already fell
    /// </summary>
    internal bool m_bIsActivated = false;
    // Use this for initialization
    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        CheckPrefabs();
        
    }

    /// <summary>
    /// Check if the prefabs are null or not.
    /// </summary>
    void CheckPrefabs()
    {
        if (!m_DustParticle)
        {
            //throw new System.Exception("Particle prefab not assing (" + name + ")");
        }
        if (!m_RocksPrefab)
        {
            throw new System.Exception("Rock prefab not assing (" + name + ") it will be replace by a sphere.");
        }
    }
    /// <summary>
    /// Instantiate the rocks one by one
    /// </summary>
    IEnumerator InstantiateRocks()
    {
        int i = 0;
        do
        {
            CreateRocks();
            yield return new WaitForSeconds(0.1f);
        } while (++i < m_iRockAmount);
    }

    /// <summary>
    /// Create the rocks with the <see cref="Rigidbody"/>
    /// </summary>
    void CreateRocks()
    {
        //Create
        GameObject rock;
        if (!m_RocksPrefab)
        {
            rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.transform.SetParent(transform, false);
        }
        else
        {
            rock = Instantiate(m_RocksPrefab, transform) as GameObject;
        }
        //Set Position


        Vector3 pos = (transform.position + m_vHeightPoint);

        if (m_bHasArea)
        {
            Vector3 rndPos = new Vector3(Random.Range(-m_vAreaSize.x / 2, m_vAreaSize.x / 2), 0, Random.Range(-m_vAreaSize.z / 2, m_vAreaSize.z / 2));
            pos += rndPos;
        }

        rock.transform.position = pos;

        //Add Rigidbody
        Rigidbody rb = rock.GetComponent<Rigidbody>();
        if(!rb)
        {
            rb = rock.AddComponent<Rigidbody>();
        }
        
        rb.constraints = (m_bLockZMove ? RigidbodyConstraints.FreezePositionZ : RigidbodyConstraints.FreezePositionX);
        rb.mass = 100;
    }

    /// <summary>
    /// Controls what happend
    /// </summary>
    private IEnumerator Slide()
    {
        if (m_DustParticle)
        {
            yield return m_DustParticle.WaitUntilParticleComplete();
        }
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Raising_FP, this.gameObject);
        yield return InstantiateRocks();
    }

    public void OnTriggerEnter(Collider other)
    {
       if(other.GetComponent<PlayerContoller>())
        {
            if(!m_bIsActivated)
            {
                m_bIsActivated = true;
                StartCoroutine(Slide());
            }
        }
    }


#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + m_vHeightPoint;
        Gizmos.DrawSphere(center, 0.2f);
        if (m_bHasArea)
        {
            Gizmos.color = new Color(0, 1, 1, 0.2f);
            Gizmos.DrawCube(center, m_vAreaSize);
        }
    }
#endif
}
