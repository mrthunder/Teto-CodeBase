using UnityEngine;
using System.Collections;

/*
 * Ai Spawner
 * By Gordon Niemann
 * Final Build - Nov 30th 2016
 */

public class Spawner : MonoBehaviour
{
    public AiController m_AiPreFab;
    public float        m_fSpawnRate = 5;
    public int          m_iMaxSpawnsAtOnce = 2;
    public int          m_iMaxTotalSpawns = 99;
    public float        m_fMinSpawnRange = 0;
    public float        m_fMaxSpawnRange = 5;
    private int         m_iSpawnedAi;

    // Use this for initialization
    void Start()
    {
        StartCoroutine(SpawnNewChildren());
    }

    private IEnumerator SpawnNewChildren()
    {
        while (m_iMaxTotalSpawns > 0)
        {
            yield return null;
            m_iSpawnedAi = GetComponentsInChildren<AiController>().Length;
            Vector3 spawnLocation;

            if (m_iSpawnedAi < m_iMaxSpawnsAtOnce && RandomPoint(transform.position, m_fMinSpawnRange, m_fMaxSpawnRange, out spawnLocation) && m_AiPreFab != null)
            {
                yield return null;
                GameObject aiGO = (Instantiate(m_AiPreFab, spawnLocation, transform.rotation) as AiController).gameObject;
                yield return null;
                aiGO.transform.SetParent(transform);
                m_iMaxTotalSpawns--;
            }
            yield return new WaitForSeconds(m_fSpawnRate);
        }
    }

    bool RandomPoint(Vector3 center, float minRange, float maxRange, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(minRange, maxRange);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    public void Die()
    {
        StopAllCoroutines();
        Destroy(this, 2);
    }

#if (UNITY_EDITOR)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_fMaxSpawnRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, m_fMinSpawnRange);
    }
#endif
}