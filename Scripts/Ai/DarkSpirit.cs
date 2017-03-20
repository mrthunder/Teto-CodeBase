using System.Collections;

/*
 * Dark Spirit Ai Scipts
 * By Gordon Niemann
 * Beta Build - Nov 1st 2016
 */

public class DarkSpirit : AiController,IPickup , IPulseInteract
{

    protected override void Start()
    {
        base.Start();
        SetState(OnNothing);
        PlaySounds("Play_Ghost_Movement");
    }

    public void OnPulseEnter(float pulseDistance)
    {
        FlashColorChange(true);
        SetState(OnRetreatFromPlayer, OnChasingPlayer);
    }

    public void OnPulseExit()
    {
        FlashColorChange(false);
    }

    protected override IEnumerator OnAttacking()
    {
        yield return null;
        SetState(OnChasingPlayer);
    }

    public void OnPickup(PlayerContoller player)
    {
        player.m_iEnergy = 0;
        PlaySounds("Stop_Ghost_Movement");
        Die();
    }
}
