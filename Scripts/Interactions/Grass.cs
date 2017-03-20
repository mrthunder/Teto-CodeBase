using UnityEngine;
using System.Collections;
using System;
using DG.Tweening;

public class Grass : MonoBehaviour, IPulseInteract,IVeinInteract
{

    //Get all the grass objects
    [SerializeField]
    private GameObject m_gGrass;

    

    private bool m_bIsActive = false;

    public bool m_bPersist = false;

    private Renderer[] m_mMaterials;

    // Use this for initialization
    void Start()
    {
        if (m_gGrass == null)
        {
            m_gGrass = transform.Find("GrassGroup").gameObject;
        }

        m_mMaterials = m_gGrass.GetComponentsInChildren<Renderer>();
        

    }

   

    
    private void ToggleGrass(bool active, float distance)
    {
        if (m_gGrass == null && m_mMaterials == null) return;

        StartCoroutine(Grow(active, distance));

    }

    

    private IEnumerator Grow(bool active ,float delay)
    {
        m_bIsActive = active;
        GetComponent<Collider>().enabled = !active;
        yield return new WaitForSeconds(delay*0.1f);
        foreach (Renderer material in m_mMaterials)
        {
            material.material.SetFloat("_ShowObject", (active ? 1 : 0));
        }
    }

   


    private bool FloatEqual(float a, float b)
    {
        return (a- b) < 0;
    }

    public void OnPulseEnter(float pulsePower)
    {
        //If is not activate, I make the grass show and grow.
        if (m_bIsActive == false)
        {
            ToggleGrass(true, pulsePower);
        }

    }

    


    public void OnLightVeinInteract(float distance)
    {
        //If is not activate, I make the grass show and grow.
        if (m_bIsActive == false)
        {
            ToggleGrass(true, distance);
        }
    }

    public void OnPulseExit()
    {
        
    }
}
