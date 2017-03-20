using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour {

    public float m_fSeconds = 0.5f;

    public GameObject m_Container;

	// Use this for initialization
	void Start () {
       
        StartCoroutine(End());
	}

    IEnumerator End()
    {
        yield return new WaitForSeconds(m_fSeconds);

        m_Container.SetActive(true);

        yield return new WaitUntil(()=>Input.GetButtonDown("Jump"));

        SceneManager.LoadScene(ProjectNames.MainMenu);

    }

	
}
