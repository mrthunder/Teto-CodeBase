using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Teto Ai Controller STATE Systems
 * By Gordon Niemann
 * Final Build - Nov 29th 2016
 */

public class AiController : Unit
{
    protected delegate void     findClosestHandler(float alertRange);
    protected event             findClosestHandler findClostest;

    // Coroutine State Controllers
    protected delegate          IEnumerator StateMethod();
    protected delegate          IEnumerator SecondState(StateMethod nextState);
    protected                   StateMethod m_CurrentState;
    protected                   SecondState m_NextState;
    
    // NavMesh Controllers
    protected NavMeshPath   m_Path;
    protected NavMeshAgent  m_Agent;
    private int             PathingMethod;

    // Player and Target Controllers
    protected Unit      m_Player;
    protected float     m_fMyDistanceToPlayer;
    protected bool      m_bDiscoveredPlayer = false; // First Ai in an area to Discover the player (Used to control who sends Alert Events)
    protected bool      m_bAiAlerted2Player = false; // Ai is alerted to the presence of the player 
    protected bool      m_bPlayerInActivateRange = false;

    // Contollers
    protected Rigidbody m_RB;
    protected Animator  m_anim;

    #region Ai Sight and Senses
    [Tooltip("Range from the player that Ai starts Acting, MUST be larger then all other values"), Space(2f), Header("Sight & Movement Controls")]
    public int m_iAiWakeupRange = 100;
    [Tooltip("Ai Forward Sight Angle in Degrees")]
    public float m_fEyeSightAngle = 45f;
    [Tooltip("Ai Forward Sight Range")]
    public float m_fEyeSightRange = 20;
    [Tooltip("Range the Ai will be automatically alerted of the player’s presence")]
    public float m_fAutoAlertedRange = 5;
    [Tooltip("Sight hight offset from the ground")]
    public float m_fSightHightOffset = .5f;
    [Tooltip("Can be Alerted from Other Ai")]
    public bool m_bAlertable = true;
    [Tooltip("This Ai Can alert other Ai's to Players Location")]
    public bool m_bAlerter = false;
    [Tooltip("Alertable Distance to other Ai's")]
    public float m_fAlertDistance = 10;

    internal Vector3 m_AiSightHeightOffsetVector; // Sight Offset so the Ai is not looking at Feet...
    #endregion

    #region Movement and Retreat
    [Tooltip("The Min Distance the Ai can Wander on one path"), Space(2f), Header("Wandering Controls")]
    public float m_fWanderRadiusMin = 10;
    [Tooltip("The Max Distance the Ai can Wander on one path")]
    public float m_fWanderRadiusMax = 20;
    [Tooltip("How long till the Ai Picks a new Path")]
    public float m_fWanderCoolDown = 5;
    [Tooltip("The speed the Ai can rotate")]
    public float m_fRotationSpeed = 2;
    [Tooltip("How Close the Ai will walk up to a wall")]
    public float m_fEdgeStopDistance = 1;
    [Tooltip("How long the Ai can be stunned for")]
    public float m_fWaitingCoolDown = 5f;
    [Tooltip("Cool Down Time till the Ai stops retreating")]
    public float m_fRetreatCoolDown = 8f;
    [Tooltip("The Minimum length the Ai plots before changing course when retreating")]
    public float m_fMinRetreatPath = 15;
    [Tooltip("The Maximum length the Ai plots before changing course when retreating")]
    public float m_fMaxRetreatPath = 30;
    #endregion

    #region Attack
    [Tooltip("Ai Cooldown bettween Attacks"), Space(2f), Header("Attack & Retreat Controls")]
    public float m_fAttackCoolDown = 1f;
    [Tooltip("Ai Cooldown when it cannot directly see you")]
    public float m_fChaseCoolDown = 25f;
    [Tooltip("The Range the Ai can hit the player")]
    public float m_fHitRange = 2f;
    [Tooltip("The Range the Ai can start an attack on the PLayer")]
    public float m_fAttackRange = 5;
    [Tooltip("The Angle the Ai can use to hit the player")]
    public float m_fAttackAngle = 10;
    [Tooltip("Ai turning speed when targeting the player for an attack")]
    public float m_fCombatTurningSpeed = 360;
    [Tooltip("When the Ai Dies, it will be come this")]
    public GameObject m_ReplaceWithWhenDie = null;
    [Tooltip("When the Ai Dies, Use this Particle Effect")]
    public GameObject m_DeathParticle = null;

    #endregion

    #region Timers
    protected float m_fWanderTimer      = 0;
    protected float m_fCanSeeYouTimer   = 0;
    protected float m_fAttackTimer      = 0;
    protected float m_fWaitingTimer     = 0;
    protected float m_fRetreatTimer     = 0;
    #endregion

    /// <summary>
    /// Starts a new coroutine and stops all previous ones | Important: "this" on IEnumerator defined in children
    /// </summary>
    /// <param name="newState">One Coroutine to rule them all</param>
    protected void SetState(StateMethod newState)
    {
        StateDefaults();
        m_CurrentState = newState;
        StartCoroutine(m_CurrentState());
    }
    /// <param name="nextState">The next Coroutine in the stack to be run</param>
    protected void SetState(SecondState newState, StateMethod nextState)
    {
        StateDefaults();
        m_NextState = newState;
        StartCoroutine(m_NextState(nextState));
    }

    protected void StateDefaults()
    {
        StopAllCoroutines();
        NavMeshControl(true);
        StartCoroutine(DistanceChecker());
        StartCoroutine(AiStateSuspender());
        StartCoroutine(AiUpdate());
    }

    protected virtual void Start()
    {
        m_anim      = GetComponentInChildren<Animator>();
        m_Player    = GameManager.Instance.m_Player;
        m_RB        = GetComponent<Rigidbody>();
        m_Path      = new NavMeshPath();

        NavMeshAgent agent          = GetComponent<NavMeshAgent>();
        m_AiSightHeightOffsetVector = new Vector3(0, m_fSightHightOffset);

        if (agent.enabled == true)
        {
            m_Agent = agent;
        }

        findClostest += Alerted;
    }

    // Event Bordcast that alerts Ai to players location
    protected void Alerted(float alertRange)
    {
        if (m_bAlertable && !m_bDiscoveredPlayer && transform.position.FastDistance(m_Player.transform.position) < alertRange)
        {
            m_fCanSeeYouTimer = m_fChaseCoolDown;
            SetState(OnTrackPlayer);
        }
    }

    protected virtual void SetMovementAnim(float speed) { }
    protected virtual float SetAlertAnim() {return 0;}

    /// <summary>
    /// Basic Startup Setup for Ais
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator OnStart()
    {
        SetState(OnWandering, OnChasingPlayer);
        yield return null;
    }

    /// <summary>
    /// Generic Coroutine Update
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator AiUpdate()
    {
        yield return null;
    }

    protected IEnumerator DistanceChecker()
    {
        while (Application.isPlaying)
        {
            if (m_bPlayerInActivateRange)
            {
                m_fMyDistanceToPlayer = Vector3.Distance(transform.position, m_Player.transform.position);
            }
            yield return null;
        }
        yield return null;
    }

    /// <summary>
    /// Used to set a nothing state for some Ai
    /// </summary>
    /// <returns></returns>
    protected IEnumerator OnNothing()
    {
        yield return null;
    }

    // Used to make Ai sleep until player is in range (Save CPU time)
    protected IEnumerator AiStateSuspender()
    {
        yield return null;

        while (Application.isPlaying)
        {
            if (m_bPlayerInActivateRange == false && transform.position.IsCloseTo(m_Player.transform.position, m_iAiWakeupRange))
            {
                m_bPlayerInActivateRange = true;
                yield return null;
                SetState(OnStart);
                break;
            }
            else if (m_bPlayerInActivateRange == true && !transform.position.IsCloseTo(m_Player.transform.position, m_iAiWakeupRange))
            {
                m_bPlayerInActivateRange = false;
                yield return null;
                SetState(OnNothing);
                break;
            }
            yield return new WaitForSeconds(2);
        }
        yield return null;
    }

    /// <summary>
    /// Makes the Ai Wait for a period (m_fWaitingCoolDown) of time and then jumps to a new coroutine
    /// </summary>
    /// <param name="nextState">The next Coroutine in the stack to be run</param>
    /// <returns></returns>
    protected IEnumerator OnWaiting(StateMethod nextState)
    {
        m_Agent.Stop();
        m_fWaitingTimer = m_fWaitingCoolDown;
        while (m_fWaitingTimer > 0)
        {
            m_fWaitingTimer = Wait(m_fWaitingTimer);
            yield return null;
        }
        yield return null;
        m_Agent.Resume();
        SetState(nextState);
    }

    /// <summary>
    /// Sets the Ai to wander around aimlessly
    /// </summary>
    /// <param name="nextStateWhenPlayerDetected">What the next state should be when the player is detected</param>
    /// <returns></returns>
    protected IEnumerator OnWandering(StateMethod nextStateWhenPlayerDetected)
    {
        m_bDiscoveredPlayer     = false;
        m_bAiAlerted2Player     = false;
        m_fWanderTimer          = 0;
        m_Agent.speed           = m_fUnitSpeed;
        Vector3 NewLocation     = Vector3.zero;

        SetMovementAnim(m_Agent.speed);
        AiManager.Instance.StoppedChasingPlayer(this);

        while (m_bDiscoveredPlayer == false)
        {
            bool CanSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            //If i can see the player, start Chansing them
            if ((CanSeeThePlayer || m_fMyDistanceToPlayer < m_fAutoAlertedRange))
            {
                m_bDiscoveredPlayer = true;
                m_fCanSeeYouTimer = m_fChaseCoolDown;
                yield return null;
                SetState(nextStateWhenPlayerDetected);
                yield return null;
            }

            bool worldColl = WorldCollision(NewLocation, 2f);
            bool pathComp = pathComplete();

            if (m_fWanderTimer <= 0 || worldColl || pathComp)
            {
                // Set timer & find new path
                m_fWanderTimer = m_fWanderCoolDown;
               // this.Print3D("I'm here", Vector3.up * 1.5f);

                AiManager.Instance.Request(this, m_fWanderRadiusMin, m_fWanderRadiusMax,
                (boolean, vector) => 
                {
                    if (boolean && m_Agent != null && m_Agent.enabled == true)
                    {
                        m_Agent.SetDestination(vector); 
                    }
                });
            }
            m_fWanderTimer = Wait(m_fWanderTimer);

            // yield return NULL will pause the execution of a method for one frame (Mostly a note to myself)
            yield return null;
        }
    }

    /// <summary>
    /// Sets the Ai to Chase the player, always switchs to OnWandering when it cannot find the player
    /// </summary>
    /// <returns></returns>
    protected IEnumerator OnChasingPlayer()
    {
        bool pathable = true;
        m_bAiAlerted2Player = true;
        Vector3 currentPlayerLoc;
        Vector3 PredictedPlayerLoc;
        Vector3 PredictedPlayerFutureLoc;

        if (m_bAlerter && m_bDiscoveredPlayer && m_fCanSeeYouTimer > 0 && AiManager.Instance.SetAlerterTokenHolder(this))
        {
            m_Agent.speed = 0;
            float animTime = SetAlertAnim();
            findClostest.Invoke(m_fAlertDistance);
            m_Agent.speed = 0;
            yield return new WaitForSeconds(animTime);
            m_bDiscoveredPlayer = false;
        }

        m_Agent.ResetPath();
        m_Agent.speed = m_fUnitRunningSeed;
        SetMovementAnim(m_Agent.speed);

        while (pathable)
        {
            bool canSeeThePlayer = CanSeeYou(m_Player.transform, m_fEyeSightAngle, m_fEyeSightRange, m_fAutoAlertedRange, m_AiSightHeightOffsetVector);

            currentPlayerLoc = m_Player.transform.position;
            PredictedPlayerLoc = AiManager.Instance.m_AvgPlayerLocation;
            PredictedPlayerFutureLoc = AiManager.Instance.m_AvgPlayerLocation;
            pathable = m_Agent.CalculatePath(currentPlayerLoc, m_Path);

            if (pathable)
            {
                AiManager.Instance.ChasingPlayer(this);
            }

            switch (PathingMethod)
            {
                case 1:
                    if (pathable)
                    {
                        m_Agent.SetDestination(currentPlayerLoc);
                    }
                    break;
                case 2:
                    if (m_Agent.CalculatePath(PredictedPlayerLoc, m_Path))
                    {
                        m_Agent.SetDestination(PredictedPlayerLoc);
                    }
                    break;
                case 3:
                    if (m_Agent.CalculatePath(PredictedPlayerFutureLoc, m_Path))
                    {
                        m_Agent.SetDestination(PredictedPlayerFutureLoc);
                    }
                    break;
                default:
                    m_Agent.SetDestination(m_Player.transform.position);
                    break;
            }

            if (!pathable)
            {
                AiManager.Instance.StoppedChasingPlayer(this);
                SetState(OnTrackPlayer);
            }
            else if (m_fAttackTimer <= 0 && canSeeThePlayer && m_fMyDistanceToPlayer < m_fAttackRange)
            {
                m_fCanSeeYouTimer = m_fChaseCoolDown;
                yield return null;
                SetState(OnAttacking);
            }
            else if (canSeeThePlayer)
            {
                m_fCanSeeYouTimer = m_fChaseCoolDown;
            }
            else if (m_fCanSeeYouTimer <= 0)
            {
                break;
            }

            m_fAttackTimer = Wait(m_fAttackTimer);
            m_fCanSeeYouTimer = Wait(m_fCanSeeYouTimer);
            yield return null;
        }

        if (m_fCanSeeYouTimer > 0)
        {
            SetState(OnTrackPlayer);
        }
        yield return null;
        SetState(OnStart);
    }

    /// <summary>
    /// Ai Waits for Player to be Pathable, near to the Players last pathable position
    /// </summary>
    /// <returns></returns>

    protected IEnumerator OnTrackPlayer()
    {
        bool pathable = false;
        bool isAtNavmeshEdge;
        AiManager.Instance.StoppedChasingPlayer(this);

        while (m_fCanSeeYouTimer > 0 || (m_fMyDistanceToPlayer > 2 && m_fMyDistanceToPlayer < 10))
        {
            m_fCanSeeYouTimer = Wait(m_fCanSeeYouTimer);
            pathable = m_Agent.CalculatePath(m_Player.transform.position, m_Path);
            isAtNavmeshEdge = WorldCollision(transform.position + transform.forward, 1.5f);

            if (pathable)
            {
                SetState(OnChasingPlayer);
                yield return null;
            }
            else if (isAtNavmeshEdge && (!pathable || m_Path.status == NavMeshPathStatus.PathPartial))
            {
                RotateTowards(m_Player.transform, 5);
                m_Agent.speed = 0;
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                m_Agent.speed = m_fUnitRunningSeed;
                m_Agent.SetDestination(m_Player.transform.position);
            }
            SetMovementAnim(m_Agent.velocity.magnitude);
            yield return null;
        }

        yield return null;
        SetState(OnStart);
    }

    /// <summary>
    /// Sets Ai to Retreat from Player
    /// </summary>
    /// <param name="nextState">State to exit it when finished Retreating</param>
    /// <returns></returns>
    protected IEnumerator OnRetreatFromPlayer(StateMethod nextState)
    {
        Vector3 retreatTo   = transform.position;
        m_fRetreatTimer     = m_fRetreatCoolDown;
        m_Agent.speed       = m_fUnitRunningSeed;
        float tryAgainIn    = 0.5f;
        bool pathable       = true;

        AiManager.Instance.StoppedChasingPlayer(this);
        AiManager.Instance.ReleaseAttackToken(this);

        while (m_fRetreatTimer > 0)
        {
            m_fRetreatTimer = Wait(m_fRetreatTimer);

            float retreatPointDistanceToPlayer = Vector3.Distance(retreatTo, m_Player.transform.position);

            if (retreatPointDistanceToPlayer < m_fMinRetreatPath)
            {
                RandomPoint(transform.position, m_fMinRetreatPath + 0.1f, m_fMaxRetreatPath, out retreatTo);
                retreatPointDistanceToPlayer = Vector3.Distance(retreatTo, m_Player.transform.position);
            }

            pathable = m_Agent.CalculatePath(retreatTo, m_Path);

            if (pathable && (retreatPointDistanceToPlayer > m_fMinRetreatPath || retreatPointDistanceToPlayer > m_fMyDistanceToPlayer))
            {
                m_Agent.SetDestination(retreatTo);
                m_fRetreatTimer = m_fRetreatTimer - tryAgainIn;

                yield return new WaitForSeconds(tryAgainIn);
            }
            yield return null;
            SetMovementAnim(m_Agent.velocity.magnitude);
        }
        yield return null;
        SetState(nextState);
    }

    /// <summary>
    /// Sets Ai to Retreat from its current location
    /// </summary>
    /// <param name="nextState">State to exit it when finished Retreating</param>
    /// <returns></returns>

    protected IEnumerator OnRetreatFromHere(StateMethod nextState)
    {
        Vector3 retreatTo   = transform.position;
        Vector3 retreatFrom = transform.position;

        m_fRetreatTimer     = m_fRetreatCoolDown;
        m_Agent.speed       = m_fUnitRunningSeed;
        float tryAgainIn    = 0.5f;
        bool pathable       = true;

        AiManager.Instance.StoppedChasingPlayer(this);
        AiManager.Instance.ReleaseAttackToken(this);

        while (m_fRetreatTimer > 0)
        {
            m_fRetreatTimer = Wait(m_fRetreatTimer);

            float newRetreatPoint = transform.position.FastDistance(retreatFrom);

            if (newRetreatPoint < m_fMinRetreatPath)
            {
                RandomPoint(transform.position, m_fMinRetreatPath + 0.1f, m_fMaxRetreatPath, out retreatTo);
                newRetreatPoint = Vector3.Distance(retreatTo, retreatFrom);
            }

            pathable = m_Agent.CalculatePath(retreatTo, m_Path);

            if (pathable && newRetreatPoint > m_fMinRetreatPath)
            {
                m_Agent.SetDestination(retreatTo);
                m_fRetreatTimer = m_fRetreatTimer - tryAgainIn;

                yield return new WaitForSeconds(tryAgainIn);
            }
        }
        yield return null;
        SetState(nextState);
    }

    protected virtual IEnumerator OnAttacking()
    {
        yield return null;
    }

    // Controls an Ai's NavMesh & Kinematic state until the switch is 100% complete
    protected IEnumerator NavMeshControl(bool state)
    {
        m_Agent.enabled = state;
        m_RB.isKinematic = state;
        yield return new WaitUntil(() => m_Agent.enabled == state);
    }

    public override void Die()
    {
        StopAllCoroutines();
        findClostest -= Alerted;
        base.Die();
    }

    /// <summary>
    /// Makes a Circle Path around an origin point
    /// </summary>
    /// <param name="origin">center point of circle</param>
    /// <param name="distance">distance from origin</param>
    /// <param name="totalCircumferencePoints">circle resolution</param>
    /// <returns></returns>
    public List<Vector3> MakeCirclePath(Vector3 origin, float distance = 4, int totalCircumferencePoints = 12)
    {
        List<Vector3> pathS = new List<Vector3>();
        Vector3 path;

        for (int i = 0; i < totalCircumferencePoints; i++)
        {
            float stepSize = Mathf.PI * 2 / totalCircumferencePoints;
            float posX = Mathf.Cos(i * stepSize);
            float posZ = Mathf.Sin(i * stepSize);
            path = new Vector3((posX * distance) + origin.x, origin.y, (posZ * distance) + origin.z);
            pathS.Add(path);
        }
        return pathS;
    }

    // Random Path finding (Verson 2) - In Testing (to Replace Ver 1) NOTE: Now repleased in Wandering with Time Slice version (See AiManger)
    public bool RandomPoint(Vector3 center, float minRange, float maxRange, out Vector3 result)
    {
        Vector3 randomPoint;
        NavMeshHit hit;

        for (int i = 0; i < 60; i++)
        {
            randomPoint = center + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(minRange, maxRange);
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        // So Ai Cannot get stuck outside the NavMesh
        if (m_Agent.CalculatePath(center, m_Path) == false)
        {
            Die();
        }

        result = center;
        return false;
    }

    // Checks to see if the "target" is Visible 
    protected bool CanSeeYou(Transform target, float angle, float sightRange, float AutoAlertedRange, Vector3 sightHightOffset)
    {
        Vector3 targetPos = target.transform.position + sightHightOffset;
        float targetAngle = Vector3.Angle(transform.forward, targetPos - transform.position);

        if (InRange(targetPos, sightRange) > 0 && (targetAngle < angle || InRange(targetPos, AutoAlertedRange) > 0))
        {
            Ray sightRay = new Ray(transform.position + transform.forward + sightHightOffset, targetPos - (transform.position + sightHightOffset));
            return RaycastHit(sightRay, target);
        }
        return false;
    }

    // Range Tester/Clamp - Clamps to 0 if "target" within the "range", otherwise returns distance to the "target"
    protected float InRange(Vector3 target, float range)
    {
        float targetDist = Vector3.Distance(transform.position, target);

        if (targetDist < range)
        {
            return targetDist;
        }
        return 0;
    }

    // Raycast Hit Tester
    protected bool RaycastHit(Ray sightRay, Transform target)
    {
        RaycastHit hitInfo;
        int ignoreEnemyLayerMask = ~(1 << 11);

        if (Physics.Raycast(sightRay, out hitInfo,float.MaxValue ,ignoreEnemyLayerMask))
        {
            if (hitInfo.transform == target)
            {
                return true;
            }
        }
        return false;
    }

    // Checks for the clostest Edge and returns true if inside tested distance
    protected bool WorldCollision(Vector3 source, float distance)
    {
        NavMeshHit closestWorldEdge;
        NavMesh.FindClosestEdge(source, out closestWorldEdge, NavMesh.AllAreas);

        if (transform.position.FastDistance(closestWorldEdge.position) < distance)
        {
            return true;
        }
        return false;
    }

    // Checks if Path of NashMesh is complete
    protected bool pathComplete()
    {
        if (Vector3.Distance(m_Agent.destination, m_Agent.transform.position) <= m_Agent.stoppingDistance)
        {
            if (!m_Agent.hasPath || m_Agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }
        return false;
    }

    // Simple Countdown Timer
    protected float Wait(float duration)
    {
        duration = duration - Time.deltaTime;

        if (duration < 0)
            duration = 0;

        return duration;
    }

    // Retruns an Array of GameObjects from a GameObjects Children
    protected GameObject[] GameObjsChildrenToArray(GameObject gameObj)
    {
        if (gameObj != null)
        {
            int ScanArraySize = gameObj.transform.childCount;

            GameObject[] gameObjsChildrenArray = new GameObject[ScanArraySize];

            for (int i = 0; i < ScanArraySize; i++)
            {
                gameObjsChildrenArray[i] = gameObj.transform.GetChild(i).gameObject;
            }
            return gameObjsChildrenArray;
        }
        return null;
    }

    // Used to manually rotate Ai’s towards something
    protected void RotateTowards(Transform target, float RotationSpeed)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
    }

    // Used to make Wwise access easyer
    protected void PlaySounds(params string[] sound)
    {
        foreach (string soundname in sound)
        {
            SoundManager.Instance.PlayEvent(soundname, gameObject);
        }
    }

    // Used by Ai to find random points around the player when pathfinding/attacking to player
    protected Vector3 RandomCircumference(float radius, Vector3 center)
    {
        int degree      = Random.Range(0, 360);
        float radian    = degree * Mathf.Deg2Rad;
        float x         = Mathf.Cos(radian);
        float z         = Mathf.Sin(radian);

        Vector3 circumferencePosition = (new Vector3(x, 0, z) * radius) + center;
        return circumferencePosition;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    // Debug Area (Not Compiled)
#if (UNITY_EDITOR)

    // Random Path finding (Verson 1) - Currently NOT used
    public Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        if (m_Agent.CalculatePath(navHit.position, m_Path) && !WorldCollision(navHit.position, m_fEdgeStopDistance))
        {
            m_Agent.SetDestination(navHit.position);
        }
        return navHit.position;
    }

    // Ai Surrounding Checks (Not currently used)
    private List<Unit> MyArea(float range)
    {
        List<Unit> MyAreaUnits = new List<Unit>();

        Collider[] surroundingColliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider c in surroundingColliders)
        {
            Unit otherUnit = c.GetComponent<Unit>();

            if (otherUnit != null
                && otherUnit != this
                && otherUnit.m_bIsVisable
                && otherUnit.m_iHealth > 0)
            {
                MyAreaUnits.Add(otherUnit);
            }
        }
        return MyAreaUnits;
    }

    // Returns TRUE if Ai Path is complete, based on required distance to target (Current Not Used)
    protected bool pathCompleteLite(float minDistance)
    {
        if (m_Agent.isActiveAndEnabled)
        {
            float dist = m_Agent.remainingDistance;
            if (dist != Mathf.Infinity && m_Agent.pathStatus == NavMeshPathStatus.PathComplete && m_Agent.remainingDistance < minDistance)
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    // Debug Gizmos (Turn off for Final Builds)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_fEyeSightRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_fAutoAlertedRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_fHitRange);

        if (Application.isPlaying && m_Player != null)
        {
            Debug_Gizmos();

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(AiManager.Instance.m_AvgPlayerLocation, 1);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(AiManager.Instance.m_AvgTargetLocationFuture, 1);
        }
    }

    private void Debug_Gizmos()
    {
        Vector3 leftRayRotation = Quaternion.AngleAxis(-m_fEyeSightAngle * 0.5f, transform.up) * transform.forward;
        Vector3 rightRayRotation = Quaternion.AngleAxis(m_fEyeSightAngle * 0.5f, transform.up) * transform.forward;

        Debug.DrawLine(transform.position, transform.position + leftRayRotation * m_fEyeSightRange, Color.blue);
        Debug.DrawLine(transform.position, transform.position + rightRayRotation * m_fEyeSightRange, Color.blue);

        Debug_NavMeshEdgeGizmo();
        //Debug_DrawPath();
        Debug_DrawRaySight();
    }

    private void Debug_DrawRaySight()
    {
        Vector3 targetPos = m_Player.transform.position + m_AiSightHeightOffsetVector;
        float targetAngle = Vector3.Angle(transform.forward, targetPos - transform.position);

        if (InRange(targetPos, m_fEyeSightRange) > 0 && (targetAngle < m_fEyeSightAngle || InRange(targetPos, m_fAutoAlertedRange) > 0))
        {
            Debug.DrawRay(transform.position + transform.forward + m_AiSightHeightOffsetVector, targetPos - (transform.position + m_AiSightHeightOffsetVector), Color.yellow);
        }
    }

    // Edge Detector Gizmos (Turn off for Final Builds)
    private void Debug_NavMeshEdgeGizmo()
    {
        NavMeshHit hit;
        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            Debug.DrawRay(hit.position, Vector3.up, Color.red);
        }
    }

    // Draw Ai Path (Turn off from Final Builds)
    private void Debug_DrawPath()
    {
        if (m_Agent != null && m_Agent.enabled)
        {
            m_Agent.CalculatePath(m_Agent.destination, m_Path);
            for (int i = 0; i < m_Path.corners.Length - 1; i++)
            {
                Debug.DrawLine(m_Path.corners[i], m_Path.corners[i + 1], Color.red);
            }
        }
    }
#endif
}