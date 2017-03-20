using UnityEngine;
using System.Collections;

public class GlowingCrystals : MonoBehaviour
{
    
    private Behaviour halo;

    public GameObject crystal;

    // Use this for initialization
    void Start()
    {
        halo = (Behaviour)GetComponent("Halo");
        if(!halo)
        {
            
            if(crystal)
            {
                
                halo = (Behaviour)crystal.GetComponent("Halo");
            }
            
        }
        if(halo)
        {
            halo.enabled = false;
        }

        
        
       
    }

    public void OnTriggerEnter(Collider other)
    {
        if(halo)
        {
            if (halo.enabled == false)
            {
                if (other.GetComponent<PlayerContoller>())
                {
                    halo.enabled = true;
                }
            }
        }
        
    }
}
