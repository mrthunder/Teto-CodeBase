using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerContoller))]
public class PlayerAttributes : MonoBehaviour {

    public int startingHealth = 100; // Added 
    public int currentHealth;
    private int energy = 100;
    //public Slider healthSlider;
    //public Slider energySlider;

    // Use this for initialization
    void Start () {
        currentHealth = startingHealth;
    }
	
	// Update is called once per frame
	void Update () {

    }

    public void Heal()
    {
        startingHealth = 100;
        print(startingHealth);
    }

    public void Energize()
    {
        energy = 100;
    }

    public void GetEnergy(int amount)
    {
        energy += amount;
        if (energy > 100)
            energy = 100;
    }

    public void TakeDamage(int damage)
    {
        startingHealth -= damage;
        //healthSlider.value = currentHealth;
        if (startingHealth == 0)
            Die();
    }

    public void useEnergy(int energyCost)
    {
        energy -= energyCost;
    }

    public virtual void Die()
    {
        Debug.Log("Is Dead");
    }

}
