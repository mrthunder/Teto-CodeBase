using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerCinematic : MonoBehaviour {



    NavMeshAgent m_agent;

    Animator m_anim;

    public GameObject m_FinalPoint;

    bool m_bSceneChanged = false;

    public Material m_MountainMaterial;

    public Surfaces m_surface = Surfaces.Grass_Low;

	// Use this for initialization
	void Start () {
       
        SoundManager.Instance.SetGameParameter(GameParameters.Teto_Speed, 0, this.gameObject);
        m_agent = GetComponent<NavMeshAgent>();
        m_anim = GetComponentInChildren<Animator>();
        SceneManager.activeSceneChanged += SceneChanged;
        SoundManager.Instance.SetSwitch(m_surface, this.gameObject);
        m_anim.SetBool("IsGrounded", true);
        StartCoroutine(WalkToPoint());
	}
	
    IEnumerator WalkToPoint()
    {
        yield return new WaitUntil(()=> m_agent.SetDestination(m_FinalPoint.transform.position));
        m_anim.SetFloat("HorizontalSpeed", 0.06f);
        yield return new WaitUntil(() => m_agent.remainingDistance == m_agent.stoppingDistance);
        m_anim.SetFloat("HorizontalSpeed", 0);
       
        while(!m_bSceneChanged)
        {
            m_anim.SetTrigger("IdleAnim1");
            yield return new WaitForSeconds(20f);
        }

    }

    void SceneChanged(Scene a, Scene b)
    {
        m_bSceneChanged = true;
    }
    
    public void FootSteps()
    {
        SoundManager.Instance.PlayFootSteps(gameObject);
    }
	
}
