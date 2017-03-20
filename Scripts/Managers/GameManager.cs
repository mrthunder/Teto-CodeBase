using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameManager : Singleton<GameManager>
{
    public PlayerContoller m_Player;
    public LightPulse m_abiPulse;

    public Vector3 m_vPlayerInitialPos;

    public LightVein m_LastCheckPoint;

    public Material[] m_matInteractions;

    public Unit m_gPlayerDoppel;

    /// <summary>
    /// Camera that is following the player
    /// </summary>
    internal CameraFollow m_PlayerCamera;

    internal int m_iPlayerDeathCount = 0;

    [Header("Cinematic:")]
    public bool m_bLevelTwoCinematic = false;

    public GameObject m_Meteor;

    public GameObject m_MeteorExplosionPrefab;

    public GameObject m_TetoPuffParticle;

    public float duration = 2f;


    // Use this for initialization
    void Awake()
    {
        
        m_Player = FindObjectOfType<PlayerContoller>();
       
        
        if (m_Player!=null)
        {
            m_PlayerCamera = FindObjectOfType<CameraFollow>();
            LoadLevel();
            m_vPlayerInitialPos = m_Player.transform.position;
            m_abiPulse = m_Player.GetComponent<LightPulse>();
            
        }

    }

    void Start()
    {
        if (m_Player != null)
        {
            if (m_bLevelTwoCinematic)
            {
                StartCoroutine(Cinematic());

            }
        }
    }
    
    IEnumerator Cinematic()
    {
        GameObject playerMesh = m_Player.transform.Find("Teto_GameReady").gameObject;
        Renderer[] renders = playerMesh.GetComponentsInChildren<Renderer>();
        m_PlayerCamera.enabled = false;
        bool jump = false;
        float maxDistance = m_vPlayerInitialPos.sqrMagnitude + m_Meteor.transform.position.sqrMagnitude;
        foreach(Renderer render in renders)
        {
            render.enabled = false;
        }
        m_Player.LockTotalMovement(true);
        Tweener tw = m_Meteor.transform.DOMove(m_vPlayerInitialPos, duration).SetEase(Ease.InFlash);
        tw.OnStart(()=> 
        {
            SoundManager.Instance.SetGameParameter(GameParameters.Distance, 1);
            SoundManager.Instance.PlayEvent(SoundEvents.Play_Meteor, m_Meteor);

        }).OnComplete(() => 
        {
            GameObject obj = Instantiate(m_MeteorExplosionPrefab);
            obj.transform.position = m_vPlayerInitialPos;
            SoundManager.Instance.SetGameParameter(GameParameters.Distance, 0);
            SoundManager.Instance.PlayEvent(SoundEvents.Stop_Meteor, m_Meteor);
            Destroy(obj, 5f);
        }).OnUpdate(()=> 
        {
            Camera.main.transform.LookAt(m_Meteor.transform);
            float distance = m_vPlayerInitialPos.sqrMagnitude + m_Meteor.transform.position.sqrMagnitude;
            SoundManager.Instance.SetGameParameter(GameParameters.Distance, Mathf.InverseLerp(0, maxDistance, distance));
            

        });

        yield return tw.WaitForCompletion();
        Destroy(m_Meteor);
        Camera.main.transform.DOLocalRotate(Vector3.zero, 1f);
        
        yield return new WaitForSeconds(3f);
       
        yield return null;
        Instantiate(m_TetoPuffParticle).transform.position = m_Player.transform.position + Vector3.up;
        m_PlayerCamera.enabled = true;
        foreach (Renderer render in renders)
        {
            render.enabled = true;
        }
        m_Player.LockTotalMovement(false);
    }

    void LoadLevel()
    {
        if (SaveManager.Instance.m_currentGame == null) return;

        string[] activated = SaveManager.Instance.m_currentGame.m_LightVeinActivated.ToArray();
       
        if(activated!=null && activated.Length >0)
        {
            print(activated.Length);
            LightVein[] lightVeins = FindObjectsOfType<LightVein>();

            for (int i = 0; i < lightVeins.Length; i++)
            {
                for (int j = 0; j < activated.Length; j++)
                {
                    if (lightVeins[i].name == activated[j])
                    {
                        if (SaveManager.Instance.m_currentGame.m_LastCheckPoint == lightVeins[i].name)
                        {
                            m_LastCheckPoint = lightVeins[i];
                        }

                        lightVeins[i].Activate();

                        break;
                    }
                }
            }
            
            m_Player.transform.position = GetLastCheckpoint();
        }
       
    }

    public void SetLastCheckPoint(LightVein checkpoint)
    {
        m_LastCheckPoint = checkpoint;
        SaveManager.Instance.m_currentGame.m_LastCheckPoint = checkpoint.name;
    }

    public Vector3 GetLastCheckpoint()
    {
        if (m_LastCheckPoint != null)
        {
            return m_LastCheckPoint.transform.position + Vector3.up;
        }
        return m_vPlayerInitialPos;
    }

    public void Update()
    {
#if UNITY_EDITOR
        //Bug Report
        if(Input.GetKeyDown(KeyCode.B))
        {
            Application.OpenURL("https://goo.gl/forms/GgYxsACr0adXpzGc2");
            
        }
#endif
        if(m_Player==null)
        {
            m_Player = FindObjectOfType<PlayerContoller>();
            if(m_Player!=null)
            {
                m_vPlayerInitialPos = m_Player.transform.position;
                m_abiPulse = m_Player.GetComponent<LightPulse>();
            }
           
        }

    }

    public GameObject GetPlayer()
    {
        
        return m_Player.gameObject;
    }
}
