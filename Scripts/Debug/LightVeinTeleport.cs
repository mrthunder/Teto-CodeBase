using UnityEngine;
using System.Collections;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class LightVeinTeleport : Singleton<LightVeinTeleport> {



    public LightVein[] m_allVeins;

    int index = 0;

    bool m_bCanPress = true;
    Stopwatch sw;

    // Use this for initialization
    void Start () {
        m_allVeins = FindObjectsOfType<LightVein>();
        sw = new Stopwatch();

	}
	
	// Update is called once per frame
	void Update () {

	    
        if(GetButton())
        {
            m_bCanPress = false;
            sw.Start();
            int direction = (int)Input.GetAxisRaw("D-PADHorizontal");
            int prevIndex = index;
            index = Mathf.Abs((index + direction) % m_allVeins.Length);

            if(index == prevIndex)
            {
                index = m_allVeins.Length - 1;
            }

            Vector3 nextlocation = m_allVeins[index].transform.position + Vector3.up;
            GameManager.Instance.m_Player.transform.position = nextlocation ;
            GameManager.Instance.m_Player.ResetRotation(m_allVeins[index].transform);
        }

        if(sw.ElapsedMilliseconds>1000 && !m_bCanPress)
        {
            sw.Stop();
            sw.Reset();
            m_bCanPress = true;
        }
	}

    bool GetButton()
    {
        return Input.GetAxisRaw("D-PADHorizontal")!= 0 && m_bCanPress;
    }

}
