using UnityEngine;
using System.Collections;

public class Collectables : MonoBehaviour
{
    PlayerContoller m_Player;
    Collider m_CollectablesCollider;
    Renderer m_CollectablesMeshRenderer;
    Renderer[] m_CollectablesOther;
    GameScreen m_CollectablesHud;
    private bool m_bRotate = true;
    public float m_fCollectableEventTime = 5;
    public float m_fRotateX = 0;
    public float m_fRotateY = 0;
    public float m_fRotateZ = 0;
    public GameObject m_DeathParticle;

    /* 
     * Updated Nov 26th 2016
     * This Script should be attached to Collectables, and allows their pickup. Script also updates HUD (must be loaded to work)
     * Note: HUD counts all Collectable items in the world (that have this script)
     */

    void Start ()
    {
        m_Player = GameManager.Instance.m_Player;
        m_CollectablesHud = FindObjectOfType<GameScreen>();
        m_CollectablesCollider = GetComponent<Collider>();
        m_CollectablesMeshRenderer = GetComponent<Renderer>();

        if (m_CollectablesMeshRenderer == null)
        {
            m_CollectablesOther = GetComponentsInChildren <Renderer>();
        }
    }

    void Update()
    {
        if (m_bRotate)
        {
            transform.Rotate(new Vector3(m_fRotateX, m_fRotateY, m_fRotateZ) * Time.deltaTime);
        }   
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject == m_Player.gameObject)
        {
            m_Player.CollectablesCollected++;
            StartCoroutine(BlinkCollectablesHud());
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Shrine_Activation, this.gameObject);
            m_CollectablesCollider.enabled = false;

            // To deal with Collectables with diffrent setups (Because of other groups)
            if (m_CollectablesMeshRenderer == null)
            {
                foreach(Renderer obj in m_CollectablesOther)
                {
                    obj.enabled = false;
                }
            }
            else
            {
                m_CollectablesMeshRenderer.enabled = false;
            }
            m_bRotate = false;
            Instantiate(m_DeathParticle, transform.position, transform.rotation);
            Destroy(gameObject, m_fCollectableEventTime);
        }
    }

    // Shows Collectable Update on Hud for a short while
    IEnumerator BlinkCollectablesHud()
    {
        m_CollectablesHud.DisplayTextOn();
        yield return new WaitForSeconds(m_fCollectableEventTime - 0.1f);
        m_CollectablesHud.DisplayTextOff();
    }
}