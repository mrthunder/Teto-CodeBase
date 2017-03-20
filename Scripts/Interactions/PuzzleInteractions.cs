using UnityEngine;
using System.Collections;

public class PuzzleInteractions : MonoBehaviour
{

    protected delegate void EndHelperHandler();
    protected event EndHelperHandler EndHelper;

    [Tooltip("When the scripts uses the light helper")]
    ///<summary>
    ///When the script uses the helper.
    ///</summary>
    public bool m_bUsingHelper = false;

    [Tooltip("Order which the helpers going to be arrange. -1 is default")]
    /// <summary>
    /// Order which the helpers going to be arrange. -1 is default
    /// </summary>
    public float m_fHelperOrder = -1;

    


    void Awake()
    {
        if (m_bUsingHelper)
        {
            EndHelper += GameManager.Instance.m_Player.GetComponent<LightHelper>().ChangeHelperIndex;
            EndHelper += DisableHelper;

        }
        gameObject.layer = LayerMask.NameToLayer("Interaction");

    }

    public void OnDestroy()
    {
        if (EndHelper != null && EndHelper.GetInvocationList().Length > 0)
        {
            EndHelper -= GameManager.Instance.m_Player.GetComponent<LightHelper>().ChangeHelperIndex;
           EndHelper -= DisableHelper;
        }

    }

    private void DisableHelper()
    {
        m_bUsingHelper = false;
    }

    protected void End()
    {
        if(m_bUsingHelper)
        {
            EndHelper();
        }
        
    }

}
