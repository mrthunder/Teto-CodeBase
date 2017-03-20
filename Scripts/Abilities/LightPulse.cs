using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using DG.Tweening;

public class LightPulse : Abilities
{

    //All the variables for the pulse will be here

    /// <summary>
    /// Maximum range of the pulse
    /// </summary>
    [SerializeField, Header("Pulse"), Tooltip("Maximum range for the pulse interact with the objects"), Range(1, 100)]
    public float m_fPulseMaxRange;

    private float m_fPulseSize;

    /// <summary>
    /// Particle that makes the effect of the pulse.
    /// </summary>
    [SerializeField, Tooltip("Effect used by the pulse abillity to show the area of effect")]
    private ParticleSystem[] m_psPulseEffect = new ParticleSystem[3];

    /// <summary>
    /// Previous objects
    /// </summary>
    private List<Collider> m_preObjects = new List<Collider>();

    /// <summary>
    /// Range of the pulse.
    /// </summary>
    internal float m_fPulseRange = 0;

    /// <summary>
    /// Say if the player is charging or not.
    /// </summary>
    private bool m_bCharging = false;

    /// <summary>
    /// Indicates when the player is interacting with the objects or not.
    /// </summary>
    private bool m_bInteracting = false;

    /// <summary>
    /// Use to define the time that the pulse will stay based on percentage value.
    /// </summary>
    [System.Obsolete("This system is not need anymore, because we can use the animation curve", true)]
    public PercentageValue m_PulseDuration;

    public float m_fMaxDuration = 10;
    public AnimationCurve m_DurationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 10));

    /// <summary>
    /// The amount of energy consume during the charge
    /// </summary>
    private float m_fChargeAmount = 0;

    /// <summary>
    /// The energy value that the player has when the charge starts.
    /// </summary>
    private float m_fTotalEnegyValue = -1;

    private float m_fTimer = 0;


    private Animator anim;

    internal SoundEvents m_PulseSound = SoundEvents.Play_Teto_Pulse;

    public ParticleSystem m_explosionPrefab;

    private ParticleSystem m_InstanceExplosion;

    #region Begining
    public override void UseAbility(Animator anim, GameObject unit, Rigidbody rb)
    {
        if (this.anim == null)
        {
            this.anim = anim;
        }
        if (m_bOnCoolDown || m_bCharging) return;
        if (!HasEnergy())
        {
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Low_Energy, this.gameObject);
            return;
        }

#if UNITY_EDITOR
        //if(AnalyticsManager.Instance != null)
        //{
        //    AnalyticsManager.Instance.SetPulseData();
        //}

#endif

        if (this.rb == null)
        {
            this.rb = rb;

        }
        if (m_Player == null)
        {
            m_Player = unit.GetComponent<Unit>();
        }
        m_preObjects.RemoveAll(x => x != null);
        m_fTotalEnegyValue = -1;
        m_bCharging = true;
        anim.SetBool("IsChargingPulse", true);
        m_psPulseEffect[3].Play();
    }
    #endregion

    #region Charge the ability
    void Update()
    {
        m_fTimer += Time.deltaTime;

        if (m_bCharging)
        {
            if (Input.GetButtonUp("Pulse"))
            {
                StopCharge();

            }
            else if (m_fTimer > .3f)
            {
                Charge();
                m_fTimer = 0;
            }

        }

    }

    void StopCharge()
    {
        m_psPulseEffect[3].Stop();
        m_psPulseEffect[3].Clear();
        if (m_fTotalEnegyValue != -1)
        {
            float percentageUsage = Mathf.InverseLerp(0, m_Player.m_iMaxEnergy, m_fChargeAmount);
            m_fCoolDownTime = m_DurationCurve.Evaluate(percentageUsage) * m_fMaxDuration;
            m_fPulseSize = m_fPulseMaxRange;
            m_fChargeAmount = 0;

        }
        anim.SetBool("IsChargingPulse", false);
        m_bCharging = false;
        anim.SetTrigger("Pulse");
        SoundManager.Instance.PlayEvent(m_PulseSound, this.gameObject);

        StartCoroutine(CoolDown());
        StartCoroutine(UsePulseEffect());
        StartCoroutine(Pulse(gameObject));

    }

    private void Charge()
    {
        if (m_fTotalEnegyValue == -1 && m_Player != null)
        {
            m_fTotalEnegyValue = m_Player.m_iEnergy;
        }
        if (!HasEnergy())
        {
            StopCharge();
        }
        else
        {
            m_fChargeAmount += m_fEnergyUse;
        }

    }
    #endregion

    #region In Process
    /// <summary>
    /// Pulse abillity
    /// </summary>
    private IEnumerator Pulse(GameObject unit)
    {

        while (m_bOnCoolDown)
        {
            //Get all objects that can interact 
            List<Collider> pulseObjects = GetOjectsInRange(m_fPulseSize, unit);
            yield return null;
            //Check if any of the objects that was caught in the collider, had already interact before.
            List<Collider> objectsToCollide = pulseObjects.Except(m_preObjects).ToList();
            yield return null;
            m_preObjects.AddRange(objectsToCollide);
            StartCoroutine(ExitInteraction(pulseObjects));
            //Start interact with the objects
            StartCoroutine(TogglePulseOjects(objectsToCollide));
            //When the interaction finish I continue the pulse
            yield return new WaitUntil(() => !m_bInteracting);
            //If teto move, I can try check the object
            yield return new WaitUntil(() => rb.velocity.magnitude > 1 || !m_bOnCoolDown);
            if (!m_bOnCoolDown)
            {
                StartCoroutine(ExitInteraction(pulseObjects));
            }

        }
    }

    IEnumerator ExitInteraction(List<Collider> pulseObjects)
    {

        List<Collider> exitObjects;
        if (m_bOnCoolDown)
        {
            exitObjects = m_preObjects.Except(pulseObjects).ToList();
        }
        else
        {
            exitObjects = pulseObjects;
        }

        yield return null;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (exitObjects.Count > 0)
        {
            m_preObjects.Remove(exitObjects[0]);
            if (exitObjects[0] != null)
            {
                IPulseInteract pulse = exitObjects[0].GetComponent<IPulseInteract>();
                if (pulse != null)
                {
                    //ExitPulse
                    pulse.OnPulseExit();

                }
            }


            exitObjects.RemoveAt(0);
            if (sw.ElapsedMilliseconds > .07f)
            {
                sw.Stop();
                yield return null;
                sw.Reset();
                sw.Start();
            }

        }
        sw.Stop();
    }

    /// <summary>
    /// Toggle the pulse effect and checks if the variable is null to toggle
    /// </summary>
    /// <param name="order">0 - begin 1- using 2 - end</param>
    private IEnumerator UsePulseEffect()
    {
        if (m_psPulseEffect != null)
        {
            
            PulseExplosionEffect();
            DOTween.To(() => m_fPulseRange, x => m_fPulseRange = x, m_fPulseSize, m_psPulseEffect[0].duration);
            m_psPulseEffect[0].startSize = m_fPulseSize * 2;
            m_psPulseEffect[0].Emit(1);
            yield return new WaitForSeconds(m_psPulseEffect[0].duration - .7f);
            m_psPulseEffect[1].startSize = 0.1f;
            m_psPulseEffect[1].Play();
            m_psPulseEffect[4].Play();
            yield return new WaitUntil(() => !m_bOnCoolDown);
            m_psPulseEffect[1].Stop();
            m_psPulseEffect[4].Stop();
            m_psPulseEffect[1].Clear();
            m_psPulseEffect[4].Clear();
            m_psPulseEffect[2].startSize = 0.1f;
            m_psPulseEffect[2].Emit(200);
            DOTween.To(() => m_fPulseRange, x => m_fPulseRange = x, 0, m_psPulseEffect[0].duration);
        }
    }

    void PulseExplosionEffect()
    {
        if (!m_explosionPrefab) return;
        if (!m_InstanceExplosion)
        {
            m_InstanceExplosion = Instantiate(m_explosionPrefab);
        }

        m_InstanceExplosion.transform.position = transform.localPosition + Vector3.up*0.5f ;
        m_InstanceExplosion.Play(true);
       
    }

    /// <summary>
    /// Gets all the objects inside the range
    /// </summary>
    /// <typeparam name="T">Type of object</typeparam>
    /// <param name="range">range of the Sphere</param>
    /// <returns></returns>
    private List<Collider> GetOjectsInRange(float range, GameObject unit)
    {

        //Get all the objects from the same type surround and store in a list
        List<Collider> list = Physics.OverlapSphere(unit.transform.position+Vector3.up*0.39f, range, LayerMask.GetMask("Interaction", "Enemies"))
            .OrderBy(x => Vector3.Distance(x.transform.position, unit.transform.position))
            .ToList();


        //In the end I convert to an array, because the list will not grow any more.
        return list;
    }

    /// <summary>
    /// Toggle all the objects with the OnPulse.
    /// </summary>
    /// <param name="objs"></param>
    private IEnumerator TogglePulseOjects(List<Collider> objs)
    {
        m_bInteracting = true;

        float timePerInteraction = .05f;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (objs.Count > 0)
        {
            if (objs[0] == null)
            {
                objs.RemoveAt(0);
                continue;
            }
           // float distance = Vector3.Distance(transform.position, objs[0].transform.position);
            IPulseInteract pulse = objs[0].GetComponent<IPulseInteract>();
            if (pulse != null)
            {
                pulse.OnPulseEnter(0);
            }

            objs.RemoveAt(0);





            if (sw.ElapsedMilliseconds < timePerInteraction)
            {

                sw.Stop();
                yield return null;
                sw.Reset();
                sw.Start();
            }
        }
        sw.Stop();

        m_bInteracting = false;

    }








    #endregion



#if CODEDEBUG

    //Debug Method
    void OnDrawGizmosSelected()
    {
        //Pulse range display
        Gizmos.color = Color.black;

        Gizmos.DrawWireSphere(transform.position+Vector3.up*0.39f, m_fPulseMaxRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.39f, m_fPulseRange);

    }

#endif
}
