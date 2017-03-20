using UnityEngine;
using System.Collections;
using DG.Tweening;

/*
 * Teto Wolf Ai Scipts
 * By Gordon Niemann
 * Beta Build - Nov 27th 2016
 */

public class Wolf : AiController, IPulseInteract
{
    public float m_fPounceHeight            = 2.5f;
    public float m_fPounceDuration          = 0.5f;
    public float m_fPounceMinDistance       = 1f;
    public float m_fPounceMaxDistance       = 5f;
    public float m_fPounceStrengthModifier  = 2.5f;
    public float m_fFlankRadius             = 6;
    public float m_fHowlAnimTime            = 2.416f;
    public float m_fHowleBreathTime         = 1.5f;

    private bool            m_bHowling      = false;
    private bool            m_bAttackComplete;
    private LightPulse      m_PlayerLightPulse;
    private ParticleSystem  m_DarkBreathParticle;
    private ParticleSystem  m_DistortionParticle;
    private ParticleSystem  m_FearParticle;
    private ParticleSystem  m_PounceParticle;

    protected override void Start()
    {
        base.Start();
        SetState(OnNothing);
        m_PlayerLightPulse = FindObjectOfType<LightPulse>();

        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
        m_DarkBreathParticle = particles[0];
        m_DistortionParticle = particles[1];
        m_FearParticle = particles[2];
        m_PounceParticle = particles[3];
    }

    public void OnPulseEnter(float pulseDistance)
    {
        if (!m_bHowling)
        {
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Wolf_Hit, this.gameObject);
            FlashColorChange(true);
            m_DistortionParticle.Stop();
            m_FearParticle.Play();
            Invoke("RestartParticles",m_fRetreatCoolDown);
            SetState(this.WaitUntilGrounedThenRetreat);
            
        }
        else
        {
            StartCoroutine(Wait2Howl());
        }
    }

    public void OnPulseExit()
    {
        FlashColorChange(false);
    }

    IEnumerator Wait2Howl()
    {
        FlashColorChange(true);
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Wolf_Hit, this.gameObject);
        yield return new WaitForSeconds(m_fHowleBreathTime);
        SetState(this.WaitUntilGrounedThenRetreat);
    }

    void RestartParticles()
    {
        m_DistortionParticle.Play();
        m_FearParticle.Stop();
    }


    protected override IEnumerator OnStart()
    {
        PlaySounds("Stop_Stalk_BG");
        SetState(OnWandering, this.ToChasingPlayer);
        yield return null;
    }

    protected override IEnumerator AiUpdate()
    {
        RaycastHit hitInfo;
        int ignoreEnemyLayerMask = ~(1 << 11);
        Vector3 offset = new Vector3(0, 0.5f, 0);

        while (Application.isPlaying)
        {
            if (Physics.SphereCast(transform.position + offset, 0.8f, Vector3.up, out hitInfo, 0.4f, ignoreEnemyLayerMask))
            {
                if (hitInfo.transform == m_Player.transform)
                {
                    m_Player.OnHit(m_iAttackPower);
                    PlaySounds("Play_Wolf_Attack");
                    anim.SetTrigger("Attack");
                    yield return new WaitForSeconds(1);
                }
            }
            yield return null;
        }
        yield return null;
    }

    protected IEnumerator ToChasingPlayer()
    {
        yield return null;
        PlaySounds("Play_Wolf_Growl");
        PlaySounds("Play_Stalk_BG");
        SetState(OnChasingPlayer); 
    }

    protected IEnumerator WaitUntilGrounedThenRetreat()
    {
        yield return new WaitUntil(() => IsGrounded());
        yield return NavMeshControl(true);
        yield return null;
        SetState(OnRetreatFromPlayer, OnChasingPlayer);
    }

    protected override IEnumerator OnAttacking()
    {
        m_bAttackComplete = false;
        int AttackMethod = Random.Range(1, 4);

        Vector3 predictedPlayerLoc = AiManager.Instance.m_AvgPlayerLocation;
        Vector3 predictedPlayerFutureLoc = AiManager.Instance.m_AvgPlayerLocation;
        
        if (AiManager.Instance.SetAttackTokenHolder(this) && !m_PlayerLightPulse.m_bOnCoolDown)
        {

            if (AttackMethod == 1 && m_fMyDistanceToPlayer > m_fPounceMinDistance && m_fMyDistanceToPlayer < m_fPounceMaxDistance)
            {
                StartCoroutine(WolfPounce(predictedPlayerLoc, m_fMyDistanceToPlayer));
                yield return new WaitUntil(() => m_bAttackComplete == true);
            }
            else if (AttackMethod == 2 && m_fMyDistanceToPlayer > m_fPounceMinDistance && m_fMyDistanceToPlayer < m_fPounceMaxDistance)
            {
                StartCoroutine(WolfPounce(predictedPlayerFutureLoc, m_fMyDistanceToPlayer));
                yield return new WaitUntil(() => m_bAttackComplete == true);
            }
            if (m_fMyDistanceToPlayer < m_fHitRange)
            {
                StartCoroutine(WolfBite());
                yield return new WaitUntil(() => m_bAttackComplete == true);
            }
        }

        yield return null;
        SetState(OnChasingPlayer);
    }

    IEnumerator WolfPounce(Vector3 attackLocation, float distanceToPlayer)
    {
        float targetAngle = Vector3.Angle(transform.forward, attackLocation - transform.position);
        m_fAttackTimer = m_fAttackCoolDown;

        if (m_Agent.CalculatePath(attackLocation, m_Path) && targetAngle < m_fAttackAngle)
        {
            yield return NavMeshControl(false);
            PlaySounds("Play_Wolf_Jump");
            anim.SetTrigger("JumpAttack");
            yield return m_RB.DOJump(attackLocation, m_fPounceHeight, 1, m_fPounceDuration, false).OnComplete(() => PlaySounds("Play_Wolf_Attack", "Play_Wolf_Land")).WaitForCompletion();
            yield return NavMeshControl(true);
            m_PounceParticle.Play();

            if (distanceToPlayer < m_fHitRange * m_fPounceStrengthModifier)
            {
                int damage = Mathf.FloorToInt(m_iAttackPower * m_fPounceStrengthModifier);
                m_Player.OnHit(damage);
            }
            yield return new WaitForSeconds(m_fPounceDuration + 1);
        }
        else
        {
            StartCoroutine(Flank(m_fFlankRadius));
            yield return new WaitUntil(() => m_bAttackComplete == true);
        }

        m_bAttackComplete = true;
        yield return null;
    }

    IEnumerator WolfBite()
    {
        PlaySounds("Play_Wolf_Attack");
        anim.SetTrigger("Attack");
        m_Player.OnHit(m_iAttackPower);
        m_fAttackTimer = m_fAttackCoolDown;
        yield return new WaitForSeconds(2);
        m_bAttackComplete = true;
        yield return null;
    }

    IEnumerator Flank(float distance)
    {
        AiManager.Instance.ReleaseAttackToken(this);
        for (int i = 0; i < 30; i++)
        {
            Vector3 flankPosition = RandomCircumference(distance, m_Player.transform.position);
            if (m_Agent.CalculatePath(flankPosition, m_Path))
            {
                m_Agent.SetDestination(flankPosition);
                float wait2sec = Time.time + 2;
                yield return new WaitUntil(() => pathComplete() || Time.time > wait2sec);
                break;
            }
            yield return null;
        }
        yield return null;
        m_bAttackComplete = true;
    }

    protected override void SetMovementAnim(float speed)
    {
        anim.SetFloat("Speed", speed);
    }

    protected override float SetAlertAnim()
    {
        StartCoroutine(HowleBreath());

        anim.SetTrigger("Wolf_Awoo");
        PlaySounds("Play_Wolf_Howling");

        return m_fHowlAnimTime;
    }

    private IEnumerator HowleBreath()
    {
        yield return null;
        m_bHowling = true;

        ParticleSystem.EmissionModule em = m_DarkBreathParticle.emission;
        ParticleSystem.MinMaxCurve rate = em.rate;

        float original_dbp_sp = m_DarkBreathParticle.startSpeed;
        float original_dbp_er = rate.constantMax;

        Transform dbp_obj = m_DarkBreathParticle.GetComponent<Transform>();
        Vector3 originalPos_dbp_obj = dbp_obj.transform.localPosition;
        Quaternion originalRot_dbp_obj = dbp_obj.transform.rotation;

        m_DarkBreathParticle.startSpeed = 4;
        rate.constantMax = 50;

        dbp_obj.localPosition = new Vector3(0, 0.95f, 0.9f);
        dbp_obj.localRotation = new Quaternion(-0.35f, 0, 0, 1);
        
        yield return new WaitForSeconds(m_fHowleBreathTime);

        dbp_obj.localRotation = originalRot_dbp_obj;
        dbp_obj.localPosition = originalPos_dbp_obj;

        m_DarkBreathParticle.startSpeed = original_dbp_sp;
        rate.constantMax = original_dbp_er;
        m_bHowling = false;
    }

    public override void Die()
    {
        if (m_DeathParticle)
        {
            Instantiate(m_DeathParticle, transform.position, transform.rotation);
        }

        if (m_ReplaceWithWhenDie)
        {
            Instantiate(m_ReplaceWithWhenDie, transform.position, transform.rotation);
        }

        base.Die();
    }


    public void SoundWolfFootSteps()
    {
        PlaySounds("Play_Wolf_Footsteps");
    }

#if (UNITY_EDITOR)

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
    }

#endif
}
