using UnityEngine;
using DG.Tweening;
using System.Collections;

public class Dash : Abilities
{
    //Dash Variables

    /// <summary>
    /// Maximum distance that the character can go
    /// </summary>
    public float m_fDashDistance;

    /// <summary>
    /// How fast is going to be the dash
    /// </summary>
    public float m_fDashDuration;

    [HideInInspector]
    /// <summary>
    /// If the player is dashing
    /// </summary>
    public bool m_bIsDashing;

    internal SoundEvents m_DashSound = SoundEvents.Play_Teto_Dash;

    

    public override void UseAbility(Animator anim, GameObject unit, Rigidbody rb)
    {
        m_fCoolDownTime = .3f;

        if (m_bOnCoolDown) return;
        if (!HasEnergy()) return;
        m_bOnCoolDown = true;

#if UNITY_EDITOR
        //if (AnalyticsManager.Instance != null)
        //{
        //    AnalyticsManager.Instance.SetDashData();
        //}
#endif


        SoundManager.Instance.PlayEvent(m_DashSound, this.gameObject);





        //The dash will go from the position of the player until the maximum distance.
        Vector3 dashTarget = unit.transform.position+ (Vector3.up*.5f) + m_fDashDistance * unit.transform.forward;
       
   

        IsSomethingBlocking(ref dashTarget, unit,m_fDashDistance,unit.transform.forward);

        //We do a simple movement tween on the player's transform through transform.DOMove()
        //We keep information about the animation in an object called myTween
        //We can change some of myTweens properties like the easing (how it animates)
        //We can also set up actions to take once the animation ends. Syntax is a bit fucky
        //But it's extremely useful, and syntax is always the same: myTween.OnComplete( () => { ACTIONS CODE BLOCK } );
        if(!m_bIsDashing)
        {
            if (!anim.GetBool("dash"))
            {
                anim.SetBool("dash", true);
                anim.SetTrigger("DashBegin");
            }

            Tweener myTween = rb.DOMove(dashTarget, m_fDashDuration);
            myTween.SetEase(Ease.InOutCubic);
            myTween.OnComplete(() =>
            {
                EndDash(anim);
            });

            m_bIsDashing = true;
        }
        

      

    }

    

    private void EndDash(Animator anim)
    {
        anim.SetBool("dash", false);
        m_bIsDashing = false;
        StartCoroutine(CoolDown());
    }

    private void DashSetting(bool active, Animator anim)
    {
        anim.SetBool("dash", active);
        m_bIsDashing = active;
    }


    

}
