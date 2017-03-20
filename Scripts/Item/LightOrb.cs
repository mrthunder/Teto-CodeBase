using UnityEngine;
using System.Collections;

public class LightOrb : MonoBehaviour {

    public int EnergyPowerupAmount = 10;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //Adding the OnTriggerEnter for the Light orbs
    void OnTriggerEnter(Collider otherCollider)
    {
        if (otherCollider.GetComponent<PlayerContoller>() != null)
        {
            otherCollider.GetComponent<PlayerAttributes>().GetEnergy(EnergyPowerupAmount);
            Destroy(gameObject);
        }
    }
}
