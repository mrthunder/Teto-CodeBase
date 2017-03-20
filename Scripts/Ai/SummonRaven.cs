using UnityEngine;
using System.Collections;

/*
 * Summons Ravens
 * By Gordon Niemann
 * Final Build - Nov 30th 2016
 */

public class SummonRaven : MonoBehaviour
{
    private Unit        m_Player;
    public GameObject   m_RavenToSummon;
    public GameObject   m_StartParticle;
    public float        m_fAlertRange = 40;

    public bool m_bWillAddCollider = false;

    // Use this for initialization
    void Start ()
    {
        m_Player = GameManager.Instance.m_Player;
        StartCoroutine(WaitUntilPlayerInRange());
    }

    IEnumerator WaitUntilPlayerInRange()
    {
        yield return null;
        while (Vector3.Distance(transform.position, m_Player.transform.position) > m_fAlertRange)
        {
            yield return new WaitForSeconds(1);
        }
        yield return null;

        StartCoroutine(TheBigReveal());
    }

    IEnumerator TheBigReveal()
    {
        Instantiate(m_StartParticle, transform.position, transform.rotation);
        yield return null;

        GameObject raven = ((GameObject)Instantiate(m_RavenToSummon, transform.position, transform.rotation));

        if(m_bWillAddCollider)
        {
            raven.AddComponent<BoxCollider>().isTrigger = true;
        }
    }
}
