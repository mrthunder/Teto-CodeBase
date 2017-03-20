using UnityEngine;
using System.Collections;
using DG.Tweening;

/*
 * Raven Script
 * By Gordon Niemann
 * Final Build - Nov 30th 2016
 */

public class FlyingRaven : AiController
{
    #region Scanning

    [Tooltip("When alerted track player for X seconds")]
    public float m_fTrackingFor = 1;
    [Tooltip("Tracking Speed")]
    public float m_fTrackingSpeed = 2;
    [Tooltip("Base alert status when player is detected (Percentage)")]
    [Range(1, 90)]
    public float m_fStartAlertRatio = 25;

    public RavenGhost   m_ravenGhostPrefab;
    public Color        m_SpotLightScanningColor = Color.magenta;
    public Color        m_SpotLightTargetedColor = Color.red;

    private RotateAround    m_Rotater;
    private Light           m_SpotLight;
    private Quaternion      m_LastScriptRotationState;
    private Quaternion      m_LastModelRotationState;
    private float           m_fLookingAround;

    #endregion

    void Awake()
    {
        GetComponent<NavMeshAgent>().enabled = false;

        m_LastScriptRotationState   = transform.localRotation;
        m_LastModelRotationState    = transform.GetChild(0).localRotation;
        m_SpotLight                 = GetComponentInChildren<Light>();
        m_SpotLightScanningColor    = m_SpotLight.color;
        m_Rotater                   = GetComponentInParent<RotateAround>();
    }

    protected override void Start()
    {
        base.Start();
        m_fTrackingFor      = m_fTrackingFor * 100;
        m_fStartAlertRatio  = m_fStartAlertRatio * 0.01f;
        SetState(OnNothing);
    }

    protected override IEnumerator OnStart()
    {
        yield return null;
        SetState(this.OnScanning);
    }

    protected IEnumerator OnScanning()
    {
        bool playerTargeted = false;
        PlaySounds("Play_Rvn_Observation");

        Tweener scriptRotateBack = transform.DOLocalRotateQuaternion(m_LastScriptRotationState, 3);
        Tweener modelRotateBack = transform.GetChild(0).DOLocalRotateQuaternion(m_LastModelRotationState, 3);
        yield return scriptRotateBack.WaitForCompletion();
        m_Rotater.m_bPaused = false;

        while (playerTargeted == false)
        {
            bool canSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            if (canSeeThePlayer && m_Player.m_bIsVisable)
            {
                playerTargeted = true;
                m_fLookingAround = m_fTrackingFor - (m_fTrackingFor * m_fStartAlertRatio);
                yield return null;
                PlaySounds("Stop_Rvn_Observation");
                m_LastScriptRotationState = transform.localRotation;
                m_LastModelRotationState = transform.GetChild(0).localRotation;
                m_Rotater.m_bPaused = true;
                SetState(this.OnPlayerDetected);
            }
            yield return null;
        }
    }

    private IEnumerator OnPlayerDetected()
    {
        Vector3 ModelRotationFix = new Vector3(-45, 0, 0);

        Tweener RotationFix = transform.GetChild(0).DOLocalRotate(ModelRotationFix, 1);
        yield return RotationFix.WaitForCompletion();

        PlaySounds("Play_Rvn_Alert");
        while (m_fLookingAround > 0 && m_fLookingAround < m_fTrackingFor)
        {
            RotateTowards(m_Player.transform, m_fTrackingSpeed);

            bool canSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            m_SpotLight.color = Color.Lerp(m_SpotLightTargetedColor, m_SpotLightScanningColor, Mathf.InverseLerp(0, m_fTrackingFor, m_fLookingAround));

            if (canSeeThePlayer && m_Player.m_bIsVisable)
            {
                m_fLookingAround--;
            }
            else
            {
                m_fLookingAround++;
            }
            yield return new WaitForSeconds(0.01f);
        }

        if (m_fLookingAround <= 0)
        {
            yield return null;
            SetState(OnAttacking);
        }
        else
        {
            yield return null;
            SetState(this.OnScanning);
        }
    }

    protected override IEnumerator OnAttacking()
    {
        bool canSeeThePlayer = true;

        while (canSeeThePlayer)
        {
            canSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            m_fAttackTimer = Wait(m_fAttackTimer);
            RotateTowards(m_Player.transform, m_fTrackingSpeed);

            if (m_fAttackTimer <= 0 && canSeeThePlayer)
            {
                PlaySounds("Play_Rvn_Attack");
                RavenGhost RavenGhostClone = Instantiate(m_ravenGhostPrefab, transform.position, transform.rotation) as RavenGhost;
                RavenGhostClone.Init(AiManager.Instance.m_AvgPlayerLocation, m_iAttackPower);
                m_fAttackTimer = m_fAttackCoolDown;
            }
            yield return null;
        }
        m_fLookingAround++;
        SetState(this.OnPlayerDetected);
    }
}