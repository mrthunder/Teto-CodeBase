using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnableScripts : MonoBehaviour
{

    public bool onTrigger = true;
    public bool changeScene = true;
    public string sceneName;
    public GameObject script;

    public void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Interaction");
    }

    void OnTriggerEnter(Collider col)
    {
        if (!onTrigger) return;

        EnableScript(col.gameObject);

    }

    void OnCollisionEnter(Collision col)
    {
        if (onTrigger) return;
        EnableScript(col.gameObject);
    }

    void EnableScript(GameObject obj)
    {
        PlayerContoller player = obj.GetComponent<PlayerContoller>();

        if (player != null)
        {
            if (changeScene)
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                if (script != null)
                {
                    script.SetActive(true);
                }
            }

        }

    }

}
