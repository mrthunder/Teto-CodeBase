using UnityEngine;
using System.Collections;

/*
 * Tetos Ai Scipts
 * By Gordon Niemann
 * Final Build - Nov 29th 2016
 */

public class Teto : AiController, IPulseInteract
{
    [Header("Tetos Random Color:")]
    public Gradient m_TetosColors = new Gradient();
    public Renderer m_SkinRenderer;
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        SetState(OnStart);
        if(m_SkinRenderer)
        {
            Color col = m_TetosColors.Evaluate(Random.value);
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_emission", col);
            propertyBlock.SetColor("_Color", col);
            m_SkinRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    // Make Teto Dance When pulsed on  
    public void OnPulseEnter(float pulseDistance)
    {
        StopAllCoroutines();
        m_Agent.speed = 0;
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsChargingPulse", true);
    }

    // "
    public void OnPulseExit()
    {
        StopAllCoroutines();
        m_Agent.speed = 0;
        anim.SetBool("IsMoving", false);
        anim.SetBool("IsChargingPulse", false);
        anim.SetTrigger("Pulse");
        StartCoroutine(TetosOnPulseExit());
    }

    // Restarts Teto after being Pulsed
    protected IEnumerator TetosOnPulseExit()
    {
        yield return new WaitForSeconds(4);
        m_Agent.speed = m_fUnitSpeed;
        anim.SetBool("IsMoving", true);
        SetState(this.OnStart);
    }

    protected override void SetMovementAnim(float speed)
    {
        anim.SetFloat("VerticalSpeed", speed);
        anim.SetFloat("HorizontalSpeed", speed);
    }

    protected override IEnumerator AiUpdate()
    {
        float rNumb = Random.Range(6,12);
        yield return new WaitForSeconds(rNumb);
        SetState(this.WaitAndSniff);
    }

    protected IEnumerator WaitAndSniff()
    {
        yield return null;
        SetMovementAnim(0);
        m_Agent.speed = 0;
        yield return new WaitForSeconds(1);
        anim.SetBool("IsMoving", false);
        yield return new WaitForSeconds(0.5f);
        anim.SetTrigger("IdleAnim1");
        yield return new WaitForSeconds(5);
        anim.SetBool("IsMoving", true);
        SetState(this.OnStart);
    }

    protected override IEnumerator OnStart()
    {
        yield return null;
        anim.SetBool("IsGrounded", true);
        SetState(OnWandering, this.OnWaitingToWandering);
        yield return null;
    }

    protected IEnumerator OnWaitingToWandering()
    {
        yield return new WaitForSeconds(0.1f);
        SetState(OnWandering, this.OnWaitingToWandering);
        yield return null;
    }
}
