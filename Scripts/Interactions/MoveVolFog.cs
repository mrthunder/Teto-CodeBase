using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveVolFog : MonoBehaviour
{
    private VolumetricLight m_VolFog;
    private Unit m_Player;
    private float m_fDefualtVolFogHeight;

    public float m_fUpdatedFogHeight = 0.09f;
    public float m_fFobBugFixHeight = -15;


    private Coroutine m_VolFogBugFix;
    private Coroutine m_LerpVolClouds;

    // Use this for initialization
    void Start ()
    {
        m_VolFog = GetComponentInChildren<VolumetricLight>();
        m_fDefualtVolFogHeight = m_VolFog.HeightScale;
        m_Player = GameManager.Instance.m_Player;
        m_VolFogBugFix = StartCoroutine(VolFogBugFix());
    }

    public void UpdateFogHeight()
    {
        StopAllCoroutines();
        m_LerpVolClouds = StartCoroutine(LerpVolClouds(0.5f, 1.5f));
    }

    private IEnumerator VolFogBugFix()
    {
        int LastPlayerDeathCount;

        while (Application.isPlaying)
        {
            LastPlayerDeathCount = GameManager.Instance.m_iPlayerDeathCount;
            yield return new WaitUntil(() => m_Player.transform.position.y <= m_fFobBugFixHeight);
            m_LerpVolClouds = StartCoroutine(LerpVolClouds(1, 99));
            yield return new WaitUntil(() => GameManager.Instance.m_iPlayerDeathCount > LastPlayerDeathCount);
            StopCoroutine(m_LerpVolClouds);
            m_VolFog.HeightScale = m_fDefualtVolFogHeight;
        }
    }

    private IEnumerator LerpVolClouds(float speed, float runForN)
    {
        float time = 0;
        while (time < runForN)
        {
            time += Time.deltaTime;
            m_VolFog.HeightScale = Mathf.Lerp(m_fDefualtVolFogHeight, m_fUpdatedFogHeight, time * speed);
            yield return null;
        }
    }
}
