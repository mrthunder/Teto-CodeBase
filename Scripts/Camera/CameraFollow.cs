using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Runtime.InteropServices;


///<author>Lucas Goes</author>
///<year>2016</year>
public class CameraFollow : MonoBehaviour
{
    private Camera m_cMainCamera;
    private bool m_bFieldOfViewUpdateComplete = true;
    internal bool m_bIn2DSection = false;

    private PlayerContoller m_pcTarget;
    private Rigidbody m_rbPlayer;


    private Transform m_tCameraContainer;

    private Vector3 m_vVelocity;

    private bool m_bLockRotation = false;
    public float m_fRotationSpeed = 2;

    public bool m_bRotateX = true;
    public bool m_bRotateY = true;
    public bool m_bInvertX = false;
    public bool m_bInvertY = true;

    public float m_fLimitAngle = 20;
    private float m_fxAngle = 0;

    private Tweener tween;

    private Sequence m_Sequence;

    /// <summary>
    /// Is the value that set the distance between the camera to the player
    /// </summary>
    private float m_fSpring = 0;

    /// <summary>
    /// Maximum distance between the camera and the player
    /// </summary>
    [Range(0, 10), Tooltip("Maximum distance between the camera and the player")]
    public float m_fCamMaxDistance = 0;

    /// <summary>
    /// Time use to the change the  <see cref="m_fSpring"/> to the max distance
    /// </summary>
    [Range(0f, 5f), Tooltip("Time use to the change the spring value to the max distance")]
    public float m_fSpringVelocity = 1;

    public LayerMask m_CollisionCheck = new LayerMask();

    bool m_bFollowing = true;

    float m_fCamMoveSpeed = 0.1f;

    // Use this for initialization
    void Awake()
    {
        if (m_fRotationSpeed < 5) m_fRotationSpeed = 5f;
        SoundManager.Instance.SetState(MusicState.Idle);
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Music, this.gameObject);
        SoundManager.Instance.PlayEvent(SoundEvents.Play_Wind, this.gameObject);
        SoundManager.Instance.PlayEvent(SoundEvents.Play_BG, this.gameObject);
        GetAxisSetting();
        m_pcTarget = FindObjectOfType<PlayerContoller>();
        m_rbPlayer = m_pcTarget.GetComponent<Rigidbody>();
        m_tCameraContainer = transform.Find("CamContainer");
        transform.position = m_pcTarget.transform.position;
        m_Sequence = DOTween.Sequence();
        m_cMainCamera = GetComponentInChildren<Camera>();
    }

    void GetAxisSetting()
    {
        if (PlayerPrefs.HasKey(SettingsScreen.m_XInvertKey) && PlayerPrefs.HasKey(SettingsScreen.m_YInvertKey))
        {
            m_bInvertX = (PlayerPrefs.GetInt(SettingsScreen.m_XInvertKey) == 1 ? true : false);
            m_bInvertY = (PlayerPrefs.GetInt(SettingsScreen.m_YInvertKey) == 1 ? true : false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (m_bFollowing)
        {
            if (m_pcTarget != null)
            {
                if (Input.GetKeyDown(KeyCode.Joystick1Button9) && !m_bLockRotation)
                {
                    StartCoroutine(RotateCameraToBack());
                }
                Clipping();
                Follow();
            }
#if CHEATCODE
            if(Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button6))
            {
                
                if(m_pcTarget)
                {
                    m_pcTarget.LockTotalMovement(true);
                }
                m_bFollowing = false;
            }
#endif
        }
        else
        {
#if CHEATCODE
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Joystick1Button6))
            {
                m_fRotationSpeed = 5f;
                if (m_pcTarget)
                {
                    m_pcTarget.LockTotalMovement(false);
                }
                m_bFollowing = true;
            }
            Move();
#endif

        }


    }
    

    void Clipping()
    {
        if (m_bLockRotation) return;
        float distance = 0.3f;
        float rotateSpeed = 0.5f;

        //check the back
        if (CheckSurround(-Camera.main.transform.forward, distance))
        {
            if (CheckSurround(-Camera.main.transform.right, distance))
            {
                transform.Rotate(Vector3.up * -rotateSpeed);
            }
            else if (CheckSurround(Camera.main.transform.right, distance))
            {
                transform.Rotate(Vector3.up * rotateSpeed);
            }
        }
        else if (CheckSurround(-Camera.main.transform.right, distance))
        {
            transform.Rotate(Vector3.up * -rotateSpeed);
        }
        else if (CheckSurround(Camera.main.transform.right, distance))
        {
            transform.Rotate(Vector3.up * rotateSpeed);
        }


    }

    bool CheckSurround(Vector3 dir, float dist)
    {
#if UNITY_EDITOR
        Debug.DrawLine(Camera.main.transform.position, Camera.main.transform.position + dir * dist, Color.green);
#endif
        int mask = ~(1 << 8);
        return Physics.Raycast(Camera.main.transform.position, dir, dist, mask, QueryTriggerInteraction.Ignore);
    }


    IEnumerator RotateCameraToBack(Transform _transform = null)
    {

        Quaternion rot = Quaternion.LookRotation((_transform ? _transform.transform.forward : m_pcTarget.transform.forward));

        tween = transform.DORotate(rot.eulerAngles, 1f);
        yield return tween.WaitForCompletion();
    }

    void Follow()
    {
        Vector3 distance = m_pcTarget.transform.position;

        float alpha = Mathf.Clamp01(m_rbPlayer.velocity.magnitude);
        float springDistance = Mathf.Lerp(0, m_fCamMaxDistance, alpha);
        float vel = m_fSpringVelocity * Time.deltaTime;
        if (alpha == 0)
        {
            vel = Time.deltaTime / 10;
        }
        m_fSpring = Mathf.MoveTowards(m_fSpring, springDistance, vel);

        transform.position = Vector3.SmoothDamp(transform.position, distance, ref m_vVelocity, m_fSpring);
        Rotate();
    }

    void Move()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button5))
        {
            m_fRotationSpeed += 0.1f;
            if (m_fRotationSpeed >= 10f)
            {
                m_fRotationSpeed = 10f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button4))
        {
            m_fRotationSpeed -= 0.1f;
            if (m_fRotationSpeed <= 0.1f)
            {
                m_fRotationSpeed = 0.1f;
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            m_fCamMoveSpeed += 0.1f;
            if (m_fCamMoveSpeed >= 1)
            {
                m_fCamMoveSpeed = 1;
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            m_fCamMoveSpeed -= 0.1f;
            if (m_fCamMoveSpeed <= 0)
            {
                m_fCamMoveSpeed = 0;
            }
        }

        float up = Input.GetAxis("Beam") - Input.GetAxis("Pounce");
        float horizontal = Input.GetAxis("Horizontal") * m_fCamMoveSpeed;
        float vertical = Input.GetAxis("Vertical") * m_fCamMoveSpeed;
        transform.Translate(horizontal, up, vertical, Camera.main.transform);
        Rotate();
    }

    private void Rotate()
    {
        float horizontalRaw = Input.GetAxis("RightStickHorizontal");
        if (Mathf.Abs(horizontalRaw) < 0.5f)
        {
            horizontalRaw = 0;
        }

        float verticalRaw = Input.GetAxis("RightStickVertical");

        if (Mathf.Abs(verticalRaw) < 0.5f)
        {
            verticalRaw = 0;
        }

        float horizontal = horizontalRaw * m_fRotationSpeed;
        float vertical = verticalRaw * m_fRotationSpeed;

        System.Func<float, float, bool> IsMoving = (x, y) => x != 0 || y != 0;

        if (IsMoving(horizontalRaw, verticalRaw))
        {
            if (tween != null && tween.IsPlaying())
            {
                tween.Kill(false);
            }
        }

        if (!m_bLockRotation)
        {
            m_fxAngle += vertical * Invert(m_bInvertY);
            float limit = m_bFollowing ? 10 : 90;
            m_fxAngle = Mathf.Clamp(m_fxAngle, -limit, limit);


            m_tCameraContainer.localEulerAngles = new Vector3(m_fxAngle, 0, 0);


            transform.Rotate(Vector3.up, horizontal * Invert(m_bInvertX));
        }
    }

    private int Invert(bool value)
    {
        if (value)
        {
            return -1;
        }
        return 1;
    }

    public void Entering2DSection()
    {
        m_bIn2DSection = true;
        WolfsChasing(false);
    }

    public void Leaving2DSection()
    {
        m_bIn2DSection = false;
    }


    /// <summary>
    /// When the wolfs is chasing the player, the camera will get far from the player and when stop chasing the camera get closer 
    /// </summary>
    /// <param name="isChasing">If is chasing or not</param>
    public void WolfsChasing(bool isChasing)
    {
        // Chase Vars
        float time = 1.5f;
        float fieldOfViewNorm = 60;
        float fieldOfViewChase = 90;

        if (m_cMainCamera.enabled == true)
        {
            if (isChasing && !m_bIn2DSection && m_bFieldOfViewUpdateComplete && m_cMainCamera.fieldOfView == fieldOfViewNorm)
            {
                StartCoroutine(FieldOfViewUpdate(fieldOfViewNorm, fieldOfViewChase, time));
            }
            else if (!isChasing && m_bFieldOfViewUpdateComplete && m_cMainCamera.fieldOfView == fieldOfViewChase)
            {
                StartCoroutine(FieldOfViewUpdate(fieldOfViewChase, fieldOfViewNorm, time));
            }
        }
    }

    IEnumerator FieldOfViewUpdate(float fieldOfViewFrom, float fieldOfViewTo, float time)
    {
        m_bFieldOfViewUpdateComplete = false;
        Tweener cameraUpdate = DOTween.To(x => m_cMainCamera.fieldOfView = x, fieldOfViewFrom, fieldOfViewTo, time);
        yield return cameraUpdate.WaitForCompletion();
        m_cMainCamera.fieldOfView = fieldOfViewTo;
        m_bFieldOfViewUpdateComplete = true;
    }



    public void ChangeAngle(float angle)
    {
        KillSequence();
        //m_Sequence.Append(transform.DORotate(Vector3.zero, 0.1f));
        m_Sequence.Append(transform.DORotate(Vector3.up * angle, 2f));
    }
    public void CameraLock(bool isLock)
    {
        m_bLockRotation = isLock;
    }

    public void ChangeDistance(float camDistance)
    {
        KillSequence();
        m_Sequence.Append(Camera.main.transform.DOLocalMoveZ(camDistance, 2f));
    }

    public void ChangeCameraViewX(float x)
    {
        KillSequence();
        m_Sequence.Append(Camera.main.transform.DOLocalMoveX(x, 2f));
    }

    public void ChangeCameraViewY(float y)
    {
        KillSequence();
        m_Sequence.Append(Camera.main.transform.DOLocalMoveY(y, 2f));
    }
    public void ChangeCameraViewZ(float z)
    {
        KillSequence();
        m_Sequence.Append(Camera.main.transform.DOLocalMoveZ(z, 2f));
    }

    public void RotateCameraX(float x)
    {
        KillSequence();
        m_Sequence.Append(m_tCameraContainer.DOLocalRotate(new Vector3(x, 0, 0), 2f));
    }

    public void Reset(bool back = false, Transform _transform = null)
    {

        if (back && !_transform)
        {
            ChangeAngle(0);
            StartCoroutine(RotateCameraToBack());
        }
        else if (!_transform)
        {
            ChangeAngle(0);
            RotateCameraX(0);
        }
        else if (_transform)
        {
            Quaternion rot = Quaternion.LookRotation(_transform.forward);
            transform.eulerAngles = rot.eulerAngles;
        }
        CameraLock(false);
        ChangeCameraViewZ(-5);
    }



    void KillSequence()
    {
        if (m_Sequence.IsPlaying())
        {
            m_Sequence.Kill();
        }
    }






}