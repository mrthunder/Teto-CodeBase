using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Linq;

[RequireComponent(typeof(Collider))]
public class EventInteration : MonoBehaviour
{

    public UnityEvent m_eOnEnter = new UnityEvent();

    public UnityEvent m_eOnStay = new UnityEvent();

    public UnityEvent m_eOnExit = new UnityEvent();

    bool m_bActive = false;

    public void Start()
    {
        
        
        gameObject.layer = LayerMask.NameToLayer("Interaction");
    }

    public void OnTriggerEnter(Collider other)
    {
        if(m_eOnEnter!= null && PlayerCheck(other) && !m_bActive)
        {

            m_eOnEnter.Invoke();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (m_eOnStay != null && PlayerCheck(other) && !m_bActive)
        {
            m_eOnStay.Invoke();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (m_eOnExit != null && PlayerCheck(other) && !m_bActive)
        {
            m_eOnExit.Invoke();
        }
    }

    bool PlayerCheck(Collider col)
    {
        PlayerContoller player = col.GetComponent<PlayerContoller>();
        return player != null && player.m_bIsPlayer;
    }

    public void DestroyAfter(float sec)
    {
        Destroy(this.gameObject, sec);
    }

    public void DisableAfter(float sec)
    {
        StartCoroutine(Disable(sec));
    }

    IEnumerator Disable(float sec)
    {
        yield return new WaitForSeconds(sec);
        m_bActive = true;
        
    }

    public void DeActivate(float sec)
    {
        StartCoroutine(Hide(sec));
    }

    IEnumerator Hide(float sec)
    {
        yield return new WaitForSeconds(sec);
        this.gameObject.SetActive(false);
    }
}
