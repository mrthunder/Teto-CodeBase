using UnityEngine;
using System.Collections;

/*
 * Light Spirit Ai Scipts
 * By Gordon Niemann
 * Beta Build - Nov 27st 2016
 */

public class LightSpirit : AiController,IPickup , IPulseInteract
{
    private bool m_bDidPlayerEatMe = false;

    protected override void Start()
    {
        base.Start();
        SetState(OnNothing);
    }

    protected override IEnumerator OnStart()
    {
        yield return null;
        SetState(OnWandering, this.OnRetreatingToWandering);
    }

    public void OnPulseEnter(float pulseDistance)
    {
        FlashColorChange(true);
        SetState(OnRetreatFromPlayer, this.OnWaitingToWandering);
    }

    public void OnPulseExit()
    {
        FlashColorChange(false);
    }

    protected IEnumerator OnWaitingToWandering()
    {
        yield return new WaitForSeconds(1f);
        SetState(OnWandering, this.OnWaitingToWandering);
    }

    protected IEnumerator OnRetreatingToWandering()
    {
        yield return new WaitForSeconds(0.1f);
        SetState(OnRetreatFromPlayer, this.OnWaitingToWandering);
    }

    public void OnPickup(PlayerContoller player)
    {
        player.m_iEnergy = player.m_iMaxEnergy;
        player.m_iHealth = player.m_iMaxHealth;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_LS_Absorb,this.gameObject);
        m_bDidPlayerEatMe = true;
        Die();
    }

    public override void Die()
    {
        if (m_DeathParticle)
        {
            Instantiate(m_DeathParticle, transform.position, transform.rotation);
        }

        if (m_ReplaceWithWhenDie && !m_bDidPlayerEatMe)
        {
            Instantiate(m_ReplaceWithWhenDie, transform.position, transform.rotation);
        }

        base.Die();
    }
}
