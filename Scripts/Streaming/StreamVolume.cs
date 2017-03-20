using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider)),AddComponentMenu("Volume/Stream Volume")]
public class StreamVolume : MonoBehaviour
{
    [SerializeField, Tooltip("Name of the scene to be load")]
    ///<summary>
    /// Name of the scene to be load whe enter the volume
    /// </summary>
    private string m_sSceneName;

    public void OnTriggerEnter(Collider other)
    {
        PlayerContoller player = other.GetComponent<PlayerContoller>();
        if (player == null) return;
        SceneManager.LoadScene(m_sSceneName, LoadSceneMode.Additive);
        
    }

    public void OnTriggerExit(Collider other)
    {
        PlayerContoller player = other.GetComponent<PlayerContoller>();
        if (player == null) return;
        SceneManager.UnloadScene(m_sSceneName);
    }
}
