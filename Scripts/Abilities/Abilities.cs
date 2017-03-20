using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using DG.Tweening;

/*
 * Lucas de Souza Goes - 2016
 */

/// <summary>
/// This class can only be inherited by others
/// </summary>
public abstract class Abilities : MonoBehaviour
{
    [Header("Ability")]
    /// <summary>
    /// The rigidbody from the who used the ability
    /// </summary>
    protected Rigidbody rb;

    /// <summary>
    /// Player reference
    /// </summary>
    protected Unit m_Player;

    ///<summary>
    ///If the ability is on cooldown
    /// </summary>
    internal bool m_bOnCoolDown = false;

    [Tooltip("Time in Seconds")]
    ///<summary>
    /// Cooldown Time in seconds that the ability will have to wait, until
    /// be able to use again.
    ///</summary>
    public float m_fCoolDownTime = 2f;

    [Tooltip("The Amount of energy that the ability will consume")]
    /// <summary>
    /// The amount of energy consume by the ability
    /// </summary>
    public int m_fEnergyUse = 1;

    /// <summary>
    /// Execute an action
    /// </summary>
    /// <param name="anim">Animator component</param>
    /// <param name="unit">Who is using the ability</param>
    /// <param name="rb">RigidBody component</param>
    public virtual void UseAbility(Animator anim, GameObject unit, Rigidbody rb)
    {
        if(this.rb == null)
        {
            this.rb = rb;
           
        }
        if(m_Player == null)
        {
            m_Player = unit.GetComponent<Unit>();
        }
       

        StartCoroutine(CoolDown());

    }

    /// <summary>
    /// If the player has energy it will be consumed
    /// </summary>
    /// <returns>True if consumed, false if not consumed</returns>
    public bool HasEnergy()
    {
        Unit unit = GetComponent<Unit>();
        if (unit != null)
        {
            if (unit.m_iEnergy < m_fEnergyUse)
            {
                return false;
            }
            else
            {
                unit.ConsumeEnergy(m_fEnergyUse);
                return true;
            }

        }
        return false;
    }

    /// <summary>
    /// Action execute after the cooldown finished.
    /// </summary>
    protected virtual void CoolDownFinish()
    {

    }

    /// <summary>
    /// Function that makes the ability enters in cooldown mode.
    /// </summary>
    protected IEnumerator CoolDown()
    {

        m_bOnCoolDown = true;
        yield return new WaitForSeconds(m_fCoolDownTime);
        m_bOnCoolDown = false;
        CoolDownFinish();
    }

    
    /// <summary>
    /// The character jumps to middle of the destination and than fly to the ground.
    /// </summary>
    /// <param name="destination">Where the character should go</param>
    /// <param name="height">How height should jump</param>
    /// <param name="duration">How fast is the movement</param>
    protected void RelativeQuickMove(GameObject unit, Vector3 destination, float height, float duration, UnityAction endMethod)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rb.DOJump(destination, height, 1, duration).SetRelative(true).OnUpdate(() =>
        {
            if (Physics.Raycast(transform.position, Vector3.down, .5f, LayerMask.GetMask("EnvironmentalTransition")))
            {
                sequence.Kill();
                endMethod();
            }
            else if (Physics.Raycast(transform.position + Vector3.up, Vector3.back, 2f, LayerMask.GetMask("EnvironmentalTransition", "Interaction")))
            {
                sequence.Kill();
                endMethod();
            }
            else if (Physics.Raycast(transform.position + Vector3.up, Vector3.up, 1f, LayerMask.GetMask("EnvironmentalTransition", "Interaction")))
            {
                sequence.Kill();
                endMethod();
            }
            Debug.DrawLine(transform.position, transform.position + Vector3.down);
            Debug.DrawLine(transform.position, transform.position + Vector3.up + Vector3.back);


        }));

        sequence.OnComplete(() => endMethod());
        sequence.Play();
    }

    /// <summary>
    /// Check if there is something in the way of the character
    /// </summary>
    /// <param name="dashTarget">Vector used for the dash</param>
    /// <param name="unit">unit that is using</param>
    protected bool IsSomethingBlocking(ref Vector3 dashTarget, GameObject unit, float distance, Vector3 direction, bool relative = false)
    {

        //I Check who is using the ability
        bool isPlayer = true;
        if (unit.GetComponent<AiController>() != null)
        {
            isPlayer = false;
        }
        //I get all the colliders in the distance
        RaycastHit[] colliders = Physics.RaycastAll(unit.transform.position + Vector3.up, direction, distance);

        foreach (RaycastHit hit in colliders)
        {
            if (isPlayer && hit.collider.GetComponent<PlayerContoller>() != null)
            {
                continue;
            }
            else if (!isPlayer && hit.collider.GetComponent<AiController>() != null)
            {
                continue;
            }
            dashTarget = hit.point - (unit.transform.forward * 0.5f);
            if (relative)
            {
                dashTarget -= unit.transform.position;
            }

            return true;
        }
        return false;
    }

   
}
