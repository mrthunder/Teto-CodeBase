using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;


public class HealthBar : MonoBehaviour
{
    private  PlayerContoller player;

    public  Image healthBar;
    public Image energyBar;
    private  float maxHealth;

    void Start()
    {
        player = FindObjectOfType<PlayerContoller>();
        //xHealth = player.m_iMaxHealth;
        //healthBar = transform.Find("HealthBarContent").GetComponent<Image>();
        //energyBar = transform.Find("EnergyBarContent").GetComponent<Image>();
    }

    public void Update()
    {
        if(player!= null)
        {
            UpdateBar(player.PercentageHealth() , healthBar);
            UpdateBar(player.PercentageEnergy() , energyBar);
        }
        else
        {
            player = FindObjectOfType<PlayerContoller>();
        }
        
    }

    public void UpdateBar(float value,Image img)
    {
        float tweenDuration = 0.5f;
        float tweenTargetValue = value;
        Tween tween = DOTween.To(() => img.fillAmount, x => img.fillAmount = x, tweenTargetValue, tweenDuration);
    }
}
