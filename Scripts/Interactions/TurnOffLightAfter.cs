using UnityEngine;
using System.Collections;

public class TurnOffLightAfter : MonoBehaviour
{
    public float m_fTimeOn = 0.5f;
    public float m_fTimeOff = 1;

    // Use this for initialization
    void Start ()
    {
        StartCoroutine(TurnOffLight());
	}

    IEnumerator TurnOffLight()
    {
        Light light = GetComponent<Light>();
        yield return new WaitForSeconds(m_fTimeOn);
        light.enabled = true;
        yield return new WaitForSeconds(m_fTimeOff);
        light.enabled = false;
    }
}
