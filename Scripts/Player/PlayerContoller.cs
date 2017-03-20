using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityStandardAssets.ImageEffects;


public class PlayerContoller : Unit
{
    public float m_fJumpHeightMax;
    public float rotationSpeed;
    public float jumpVelocityDamper = 0.5f;
    internal int CollectablesCollected;

    private Rigidbody rb;
    private Transform camContainer;
    private EdgeDetection edgeDetection;
    private int m_iHealthLastFrame;

    // Default Value for airBornDelta should always be 0
    private float airBornDelta = 0;

    private float lockedHorizontalInput;
    private float lockedverticalInput;
    private float modifiedHorizontalInput;
    private float modifiedLockedverticalInput;


    internal LightPulse m_abiLightPulse;


    public bool m_bTwoDMov = false;

    public ParticleSystem m_particleIdle;

    private float m_fJumpheight;
    private bool m_bJumpCharge = false;
    private bool m_bLanding = false;
    private float m_fJumpVelocity = 0;
    private bool m_bIsGroundTriggerAnim = false;

    private bool m_bDoubleJump = true;
    private bool m_bCanJump = false;

    private float horizontalInput;
    private float verticalInput;

    public bool m_bRigidAtivated = true;
    bool m_bLockTotalMovement = false;

    /// <summary>
    /// Delay in seconds
    /// </summary>
    [Tooltip("Delay in Seconds")]
    public float m_fEnergyRegenDelay = 2f;

    Material m_groundMat;

    [Header("Collision")]
    [Tooltip("Layers which that will stop the player's movement")]
    private int collisionLayer = 0;

    Coroutine m_deathCoroutine = null;

    private Animator m_WolfFangAnimator;

   

    // Use this for initialization
    void Start()
    {
        collisionLayer = ~(1 << 8);
        GameManager.Instance.m_vPlayerInitialPos = transform.position;
        rb = GetComponent<Rigidbody>();


        camContainer = FindObjectOfType<CameraFollow>().transform;
        edgeDetection = camContainer.GetComponentInChildren<EdgeDetection>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        // Abilities

        m_abiLightPulse = GetComponent<LightPulse>();
        HealthSound();
        StartCoroutine(IdleAnimations());

        SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Aura, this.gameObject);

        m_iHealthLastFrame = m_iHealth;

        GameObject obj = GameObject.Find("WolfFangCanvas");
        if (obj)
            m_WolfFangAnimator = obj.GetComponentInChildren<Animator>();

#if CHEATCODE
        if (LightVeinTeleport.Instance != null)
        {

        }
#endif

    }

    IEnumerator IdleAnimations()
    {
        while (Application.isPlaying)
        {
            yield return new WaitUntil(() => IsPlayerStop());
            yield return new WaitForSeconds(4f);
            if (IsPlayerStop())
            {
                anim.SetTrigger("IdleAnim1");

                yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(0).IsName("Base.Air sniff"));
            }
        }
    }

    public bool IsPlayerStop()
    {
        bool result = (horizontalInput == 0) && (verticalInput == 0);


        return result;
    }

    public bool IsPlayerStop(out Vector3 direction)
    {
        direction = new Vector3(horizontalInput, 0, verticalInput);
        bool result = (horizontalInput == 0) && (verticalInput == 0);

        return result;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (m_bLockTotalMovement)
        {
            anim.SetFloat("VerticalSpeed", 0);
            anim.SetFloat("HorizontalSpeed", 0);
            anim.SetBool("IsMoving", false);
            anim.SetFloat("RunSpeed",0);
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        anim.SetBool("IsMoving", !IsPlayerStop());
        anim.SetBool("IsGrounded", IsGrounded());

        GetInputs(out horizontalInput, out verticalInput);

        // Rotation
        Rotate(horizontalInput, verticalInput);

        //Where the abilities controls are
        AbilitiesControl();

        Movement(horizontalInput, verticalInput);

        anim.SetFloat("VerticalSpeed", verticalInput);
        anim.SetFloat("HorizontalSpeed", horizontalInput);
        anim.SetFloat("RunSpeed", Mathf.Lerp(0, 2, Mathf.Abs(verticalInput) + Mathf.Abs(horizontalInput)));

        // used for flashing the screen when the player is damaged
        if (m_iHealthLastFrame > m_iHealth)
        {
            StartCoroutine(screenFlash());
            m_iHealthLastFrame = m_iHealth;
        }

    }

    IEnumerator screenFlash()
    {
        if (m_WolfFangAnimator)
        {
            m_WolfFangAnimator.SetTrigger("Bite");
        }
        edgeDetection.edgesOnly = 0.75f;
        yield return new WaitForSeconds(0.1f);
        edgeDetection.edgesOnly = 0;
    }

    private void Movement(float horizontalInput, float verticalInput)
    {
        IdleParticle();
        // Movment


        JumpInput(horizontalInput, verticalInput);


        if (IsGrounded())
        {
            if (!m_bIsGroundTriggerAnim)
            {
                m_bIsGroundTriggerAnim = true;
                anim.SetTrigger("GroundTrigger");
            }
            if (!m_bSaveJump)
            {
                m_bSaveJump = true;
            }
            Movment(new Vector3(horizontalInput, rb.velocity.y, verticalInput), 1);
            airBornDelta = 0;




            if (m_bLanding)
            {
                m_bLanding = false;
                //

                SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Land, this.gameObject);

                anim.SetBool("IsJumping", false);
                anim.SetTrigger("Ground");
            }
        }
        else
        {
            if (m_bIsGroundTriggerAnim)
            {
                m_bIsGroundTriggerAnim = false;
                anim.SetTrigger("FloatTrigger");
            }
            if (!m_bLanding && rb.velocity.y < 0)
            {

                m_bLanding = true;
                anim.SetTrigger("Landing");

            }
            m_fJumpVelocity = rb.velocity.y;


            // Airborn units lose "some" movement control (See jumpVelocityDamper)
            if (airBornDelta == 0)
            {
                lockedHorizontalInput = horizontalInput;
                lockedverticalInput = verticalInput;
            }

            airBornDelta = airBornDelta + Time.deltaTime;

            if (lockedHorizontalInput < 0)
            {
                modifiedHorizontalInput = Mathf.Max(((float)(lockedHorizontalInput + (horizontalInput * jumpVelocityDamper))), -1);
            }
            else
            {
                modifiedHorizontalInput = Mathf.Min(((float)(lockedHorizontalInput + (horizontalInput * jumpVelocityDamper))), 1);
            }

            if (lockedverticalInput < 0)
            {
                modifiedLockedverticalInput = Mathf.Max(((float)(lockedverticalInput + (verticalInput * jumpVelocityDamper))), -1);
            }
            else
            {
                modifiedLockedverticalInput = Mathf.Min(((float)(lockedverticalInput + (verticalInput * jumpVelocityDamper))), 1);
            }


            Movment(new Vector3(horizontalInput, rb.velocity.y, verticalInput), 1);
        }
    }


    private void JumpInput(float horizontalInput, float verticalInput)
    {
        if (Input.GetButtonDown("Jump"))
        {
            m_fJumpheight = 1;
            CheckJump(horizontalInput, verticalInput);

        }
    }

    bool m_bSaveJump = true;
    private void CheckJump(float horizontalInput, float verticalInput)
    {
        m_bJumpCharge = false;
        if (IsGrounded())
        {
            m_bDoubleJump = true;
            Jump(horizontalInput, verticalInput);
        }
        else if (m_bDoubleJump)
        {
            m_bLanding = false;
            m_bDoubleJump = false;
            m_bSaveJump = false;
            Jump(horizontalInput, verticalInput);
        }
        else if (m_bSaveJump)
        {
            m_bDoubleJump = true;
            m_bSaveJump = false;
            m_bLanding = false;

            Jump(horizontalInput, verticalInput);
        }
    }

    public void LockTotalMovement(bool lockmov)
    {
        m_bLockTotalMovement = lockmov;

    }

    void Jump(float horizontalInput, float verticalInput)
    {

        SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Jump, this.gameObject);

        Movment(new Vector3(horizontalInput, m_fJumpheight * m_fJumpHeightMax, verticalInput), jumpVelocityDamper);
        anim.SetTrigger("Jump");
        anim.SetBool("IsJumping", true);
        airBornDelta = 0;
        m_fJumpVelocity = rb.velocity.y;
    }

    private void IdleParticle()
    {
        if (rb.velocity.sqrMagnitude > 0)
        {
            m_particleIdle.gameObject.SetActive(false);
        }
        else
        {
            m_particleIdle.gameObject.SetActive(true);
        }
    }

    private void AbilitiesControl()
    {


        if (Input.GetButtonDown("Pulse"))
        {
            m_abiLightPulse.UseAbility(anim, gameObject, rb);
        }
    }

    void Movment(Vector3 movement, float modifier)
    {

        //Checking if there is colling with something
        Vector3 center = transform.position + Vector3.up * 0.5f;
        bool moveFoward = Physics.Raycast(center, transform.forward, 1f, collisionLayer, QueryTriggerInteraction.Ignore);
        bool moveSide = Physics.Raycast(center, transform.right * (movement.x >= 0 ? 1 : -1), 0.8f, collisionLayer, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
        Debug.DrawLine(center, center + transform.forward * 1f, Color.red);
#endif
        float jump = (m_fJumpHeightMax * m_fJumpheight);
        if (moveFoward && Mathf.Abs(movement.z) > 0 && movement.y != jump) return;
        if (moveSide && Mathf.Abs(movement.x) > 0 && movement.y != jump && !GameManager.Instance.m_PlayerCamera.m_bIn2DSection) return;
        //end

        Vector3 input;
        if (m_bTwoDMov)
        {
            input = new Vector3(movement.x * m_fUnitSpeed, 0, 0);
        }
        else
        {
            input = new Vector3(movement.x * m_fUnitSpeed, 0, movement.z * m_fUnitSpeed);
        }

        Vector3 vel = camContainer.TransformVector(input);
        vel.y = movement.y;

        rb.velocity = vel;

    }

    private void Rotate(float horizontalInput, float verticalInput)
    {
        if (Mathf.Abs(horizontalInput) != 0 || Mathf.Abs(verticalInput) != 0)
        {
            Quaternion offset;
            if (m_bTwoDMov)
            {
                offset = Quaternion.LookRotation(camContainer.TransformVector(new Vector3(horizontalInput, 0, 0)));
            }
            else
            {
                offset = Quaternion.LookRotation(camContainer.TransformVector(new Vector3(horizontalInput, 0, verticalInput)));
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, offset, Time.deltaTime * rotationSpeed);
        }
    }

    private static void GetInputs(out float horizontalInput, out float verticalInput)
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    public override void Die()
    {
        if (m_deathCoroutine == null)
        {
            m_deathCoroutine = StartCoroutine(SpawnSequence());
        }

    }

    IEnumerator SpawnSequence()
    {
        SoundManager.Instance.SetState(MusicState.Idle);
        LockTotalMovement(true);
        SoundManager.Instance.PlayTrigger(this.gameObject, "Dead");

        IsTwoD(false);
        m_iHealth = m_iHealthLastFrame = m_iMaxHealth;
        render.enabled = false;
        ParticleSystem particleDeath = transform.FindChild("Particle_Death").GetComponent<ParticleSystem>();
        particleDeath.Emit(1);
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Teto_Spawn, this.gameObject);
        yield return particleDeath.WaitUntilParticleComplete();
        transform.position = GameManager.Instance.GetLastCheckpoint();
        ResetRotation();

        GameObject spawnParticle = transform.FindChild("Particle_Spawning").gameObject;
        SoundManager.Instance.PlayEvent(SoundEvents.Play_EB_Activation, this.gameObject);
        spawnParticle.SetActive(true);
        yield return new WaitForSeconds(1f);
        render.enabled = true;
        GameManager.Instance.m_iPlayerDeathCount++;
        yield return new WaitForSeconds(0.5f);
        LockTotalMovement(false);
        spawnParticle.SetActive(false);
        m_deathCoroutine = null;
    }

    public void ResetRotation(Transform _transform = null)
    {
        Transform t = null;
        if (GameManager.Instance.m_LastCheckPoint && !_transform)
        {
            t = GameManager.Instance.m_LastCheckPoint.transform;
            Quaternion rot = Quaternion.LookRotation(t.forward);
            transform.eulerAngles = rot.eulerAngles;
        }
        else if (_transform)
        {
            t = _transform;
            Quaternion rot = Quaternion.LookRotation(t.forward);
            transform.eulerAngles = rot.eulerAngles;
        }
        GameManager.Instance.m_PlayerCamera.Reset(true, t);
    }

    public void IsTwoD(bool twod)
    {
        m_bTwoDMov = twod;
        if (!twod)
        {
            rb.constraints &= RigidbodyConstraints.FreezeRotation;
        }

    }
    public void LockMov(string axis)
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        if (axis == "x")
        {

            rb.constraints |= RigidbodyConstraints.FreezePositionX;
        }
        else if (axis == "z")
        {

            rb.constraints |= RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        IPickup pickup = collision.collider.GetComponent<IPickup>();

        if (pickup != null)
        {
            pickup.OnPickup(this);
        }
    }




}
