using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Raven : AiController
{
    #region Scanning
    [Tooltip("Time Between Scan Points"), Space(2f), Header("Scanning Controls")]
    public float m_fTimeToNextScanPoint = 3f;

    [Tooltip("Parent GameObject with ScanPoints")]
    public GameObject m_ScanPointObj;
    private GameObject[] m_ScanPoints;
    //private Quaternion m_initialRotation;

    [Tooltip("When alerted track player for X seconds")]
    public float m_fTrackingFor = 1;

    [Tooltip("Speed the Raven ")]
    public float m_fTrackingSpeed = 2;

    [Tooltip("Base alert status when player is detected (Percentage)")]
    [Range(1, 90)]
    public float m_fStartAlertRatio = 25;

    public RavenGhost m_ravenGhostPrefab;

    private Light m_SpotLight;
    private Color m_SpotLightScanningColor;
    public Color m_SpotLightTargetedColor = Color.red;

    private float m_fLookingAround;

    #endregion

    void Awake()
    {
        GetComponent<NavMeshAgent>().enabled = false;
        GetComponent<Rigidbody>().useGravity = false;

        m_SpotLight = GetComponentInChildren<Light>();
        m_SpotLightScanningColor = m_SpotLight.color;
    }

    protected override void Start()
    {
        base.Start();
        m_ScanPoints = GameObjsChildrenToArray(m_ScanPointObj);
        SetState(OnNothing);
        m_fTrackingFor = m_fTrackingFor * 100;
        m_fStartAlertRatio = m_fStartAlertRatio * 0.01f;
    }

    protected override IEnumerator OnStart()
    {
        yield return null;
        SetState(this.OnScanning);
    }

    // Scanning STATE - Not DONE
    protected IEnumerator OnScanning()
    {
        int scanPoint = 0;
        //Quaternion angle = transform.rotation;
        Tweener tRotation;
        bool playerTargeted = false;
        m_fAttackTimer = m_fAttackCoolDown;
        PlaySounds("Play_Rvn_Observation");

        while (playerTargeted == false)
        {
            Vector3 nextScanPointAngle = m_ScanPoints[scanPoint].transform.position;
            tRotation = transform.DOLookAt(nextScanPointAngle, m_fTimeToNextScanPoint);

            yield return tRotation.WaitForCompletion();
            scanPoint = (scanPoint + 1) % m_ScanPoints.Length;

            bool canSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            if (canSeeThePlayer && m_Player.m_bIsVisable)
            {
                playerTargeted = true;
                m_fLookingAround = m_fTrackingFor - (m_fTrackingFor * m_fStartAlertRatio);
                yield return null;
                PlaySounds("Stop_Rvn_Observation");
                SetState(this.OnPlayerDetected);
            }
        }
    }

    private IEnumerator OnPlayerDetected()
    {
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