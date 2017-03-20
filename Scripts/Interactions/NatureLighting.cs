using UnityEngine;

public class NatureLighting : MonoBehaviour {

    Behaviour fruitHalo;
    Light fruitLight;

    // Use this for initialization
    void Start ()
    {
        fruitHalo = (Behaviour)GetComponent("Halo");
        fruitLight = GetComponentInChildren<Light>();
    }
	
	// Update is called once per frame
	public void AddAreaLighting()
    {
        fruitHalo.enabled = true;
        fruitLight.enabled = true;
    }
}
