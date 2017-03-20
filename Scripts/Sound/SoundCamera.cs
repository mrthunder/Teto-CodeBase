using UnityEngine;
using System.Collections;

public class SoundCamera : MonoBehaviour
{

    string m_sEventName = "";

    // Use this for initialization
    void Start()
    {
#if AUDIO
        SoundManager.Instance.PlayEvent("", this.gameObject);

#endif
    }

    public void OnDestroy()
    {

    }
}
