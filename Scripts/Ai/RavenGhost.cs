using UnityEngine;
using System.Collections;

public class RavenGhost : MonoBehaviour
{
    public float m_fSpeed;
    private Vector3 m_Target;
    //private int m_iDamage;

    private Rigidbody m_RavenGhostRB;
    private MeshRenderer m_RavenGhostRenderer;
    private SphereCollider m_RavenCollider;


    private GameObject m_GhostExplosion;
    //private SphereCollider m_GhostExplosioCollider;
    private Renderer m_GhostExplostionRenderer;

    public float m_fGhostExplosionDuration = 5;
    

    public void Init(Vector3 _target, int _damage)
    {
        this.m_Target = _target;
        //this.m_iDamage = _damage;
    }

    // Use this for initialization
    void Start()
    {
        transform.LookAt(m_Target + Vector3.up * 0.5f);

        m_RavenGhostRB = GetComponent<Rigidbody>();
        m_RavenGhostRB.AddRelativeForce(Vector3.forward * m_fSpeed);
        m_RavenGhostRenderer = GetComponent<MeshRenderer>();
        m_RavenCollider = GetComponent<SphereCollider>();

        m_GhostExplosion = transform.Find("Explosion").gameObject;
        //m_GhostExplosioCollider = m_GhostExplosion.GetComponent<SphereCollider>();
        m_GhostExplostionRenderer = m_GhostExplosion.GetComponent<Renderer>();

        Invoke("EnableCollider", 0.15f);
    }

    private void EnableCollider()
    {
        m_RavenCollider.enabled = true;
    }

    public void OnTriggerEnter(Collider collision)
    {
        if(collision.GetComponent<LightBox>() == null)
        {
            m_RavenGhostRB.velocity = Vector3.zero;
            m_RavenGhostRB.angularVelocity = Vector3.zero;
            m_RavenGhostRB.isKinematic = true;
            m_RavenGhostRenderer.enabled = false;
            m_GhostExplosion.SetActive(true);

            StartCoroutine(GhostExplosion());
        }
    }

    IEnumerator GhostExplosion()
    {
        float explosionSize = 0;
        MaterialPropertyBlock blocker = new MaterialPropertyBlock();
        m_GhostExplostionRenderer.GetPropertyBlock(blocker);
        Color color = blocker.GetVector("_Color");
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Rvn_Explosion, gameObject);

        while (explosionSize < m_fGhostExplosionDuration)
        {
            explosionSize = Mathf.Lerp(explosionSize, m_fGhostExplosionDuration, Time.deltaTime);
            explosionSize = explosionSize + 0.1f;

            color.a = Mathf.Lerp(color.a, 0, Time.deltaTime);
            blocker.SetColor("_Color", color);
            m_GhostExplostionRenderer.SetPropertyBlock(blocker);

            m_GhostExplosion.transform.localScale = m_GhostExplosion.transform.localScale = new Vector3(explosionSize, explosionSize, explosionSize);

            yield return null;
        }
        Destroy(gameObject);
    }
}
