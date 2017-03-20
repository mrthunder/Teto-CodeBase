using UnityEngine;
using System.Collections;

public class CameraVein : MonoBehaviour
{

    Animator m_Anim;

    public string m_sAnimationName = string.Empty;

    

    public void Start()
    {
        m_Anim = GetComponent<Animator>();
        GameManager.Instance.m_Player.LockTotalMovement(true);
        if(TutorialManager.IsInstantiated())
        {
            TutorialManager.Instance.HideButton(string.Empty);
        }
        
        StartCoroutine(AutoDestroy());
    }

    IEnumerator AutoDestroy()
    {
        GameObject cam = Camera.main.transform.parent.parent.gameObject;
        cam.SetActive(false);
        yield return new WaitUntil(() => !m_Anim.GetCurrentAnimatorStateInfo(0).IsName(m_sAnimationName));
        GameManager.Instance.m_Player.LockTotalMovement(false);
        if (TutorialManager.IsInstantiated())
        {
            TutorialManager.Instance.HideAllButtons(false);
        }
        cam.SetActive(true);
        this.transform.parent.gameObject.SetActive(false);
        //Destroy(this.gameObject);
    }
}
