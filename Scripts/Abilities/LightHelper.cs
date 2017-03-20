using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

public class LightHelper : MonoBehaviour {

    private int m_iHelperIndex = 0;

    public HelperPoints[] m_Helpers;

    private bool m_fBlockAction = false;

    public Transform m_trHelper;

    public float m_fMoveDuration = 2f;

	// Use this for initialization
	void Start () {
        m_Helpers = FindObjectsOfType<HelperPoints>();
        m_Helpers = m_Helpers.OrderBy(x => x.m_fHelperOrder).ToArray();
	}
	
    void Update()
    {
        if(Input.GetButtonDown("Emotions"))
        {
            ShowHelper();
        }
    }

    public void ChangeHelperIndex()
    {
        m_iHelperIndex++;
    }

    private void ShowHelper()
    {
        
        if(m_iHelperIndex < m_Helpers.Length && !m_fBlockAction)
        {
            
            m_fBlockAction = true;
            m_trHelper.SetParent(null);
            m_trHelper.DOMove(m_Helpers[m_iHelperIndex].transform.position, m_fMoveDuration).OnComplete(UnlockAction);
        }
    }

    void UnlockAction()
    {
        m_fBlockAction = false;
       
        StartCoroutine(FollowPlayer());       
    }
    IEnumerator FollowPlayer()
    {
        float distance = Vector3.Distance(m_trHelper.position, GameManager.Instance.m_Player.transform.position + Vector3.up);
        
        while (distance > 1)
        {
            //print(distance);

            Vector3 destination = GameManager.Instance.m_Player.transform.position + Vector3.up;
            Vector3 vector = destination - m_trHelper.position;
            
            
            
            //vector *= m_fMoveDuration;
            m_trHelper.Translate(vector.normalized* 0.2f, Space.World);
            distance = Vector3.Distance(m_trHelper.position, destination);
            yield return null;
        }
        m_trHelper.SetParent(GameManager.Instance.m_Player.transform);
    }

   
}
