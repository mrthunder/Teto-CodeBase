using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using DG.Tweening;

[System.Serializable]
public class LightVeinEvents : UnityEvent
{

}
public class LightVein : MonoBehaviour, IPulseInteract
{
    [Header("Light Vein - Interaction:")]
    [Tooltip("Click on the + to add slots. Drag and drop object, that will interact with the LV, to the slot and select the script. (Prioritize object with the method OnLightVein, that are not in the range)")]
    /// <summary>
    /// On interaction, will trigger all objects that have to interact with the light vein when activate.
    /// </summary>
    /// <example>
    /// If you want an object to interact to this light vein that normally would be in the range, you can just drag and drop in the inspector
    /// </example>
    public LightVeinEvents m_OnInteraction = new LightVeinEvents();

    [Header("Light Vein - Settings:")]
    [SerializeField, Tooltip("Max area of effect")]
    ///<summary>
    ///The Area of Effect of the pulse will grow until reachs the max
    ///</summary>
    private float m_fMaxRange;

    /// <summary>
    /// Actual range of AoE (Area of Effect)
    /// </summary>
    internal float m_fRange = 0;

    public Vector3 m_vRangeOffset = Vector3.zero;

    [Range(0, 10)]
    /// <summary>
    /// Size increment for the range
    /// </summary>
    public float m_fIncrement;

    [Header("Light Vein - Material:")]
    [SerializeField, Tooltip("Materials of the surround objects")]
    /// <summary>
    /// Materials of the object that is going to interact with this light vein
    /// </summary>
    private Material[] m_matMaterials;

    [Header("Light Vein - Effect:")]
    [SerializeField]
    /// <summary>
    /// Plane with the circle texture
    /// </summary>
    private GameObject m_gCircleEffect;

    /// <summary>
    /// This offset makes the plane shows the circle texture the same size as the range.
    /// </summary>
    public float offset = 1;

    /// <summary>
    /// Tell if the light vein is already activated. So it will not activated twice. 
    /// </summary>
    internal bool m_bIsActivate = false;

    /// <summary>
    /// Tells with the Light Vein will send the player to the next level or not.
    /// </summary>
    public bool m_bWarpToNextLevel = false;
    /// <summary>
    /// Where is the destination
    /// </summary>
    public string m_sNextLevelName = string.Empty;

    [Header("Light Vein - Camera Cinematic:")]
    ///<summary>
    ///The Gameobject that has the camera with the animator.
    /// </summary>
    public GameObject m_gCameraEffectContainer;
    /// <summary>
    /// Delay for the camera event starts
    /// </summary>
    public float m_fStartDelay;

    /// <summary>
    /// Cinematc in videos will be prioritize.
    /// </summary>
    [Tooltip("Cinematic in video")]
    public MovieTexture m_MovieCinematic;

    /// <summary>
    /// All objects that will interact with the light vein
    /// </summary>
    public List<Collider> m_ObjectsAround = new List<Collider>();

    /// <summary>
    /// This will be objects that can be affect by the environmental Transition.
    /// </summary>
    public List<Renderer> m_EnvironmentalMaterials = new List<Renderer>();

    private ParticleSystem m_GroundParticle;

    /// <summary>
    /// Object called GodRays
    /// </summary>
    private GameObject m_Ray;

    public bool m_bSoundLB = false;

    internal SoundEvents m_StartEmitter = SoundEvents.Play_LV_Emitter;

    internal SoundEvents m_StopEmitter = SoundEvents.Stop_LV_Emitter;

    internal SoundEvents m_Activation = SoundEvents.Play_LV_Activation;

    void Start()
    {
        if (m_fIncrement == 0) m_fIncrement = 1;
        if (!m_bWarpToNextLevel)
        {

            GetObjectsAround();

            FindMaterials();

        }

        if (m_bSoundLB)
        {
            m_StartEmitter = SoundEvents.Play_EB_Emitter;

            m_StopEmitter = SoundEvents.Stop_EB_Emitter;

            m_Activation = SoundEvents.Play_EB_Activation;
        }
        else
        {
            m_StartEmitter = SoundEvents.Play_LV_Emitter;

            m_StopEmitter = SoundEvents.Stop_LV_Emitter;

            m_Activation = SoundEvents.Play_LV_Activation;
        }

        SoundManager.Instance.PlayEvent(m_StartEmitter, this.gameObject);
        m_GroundParticle = transform.FindChild("Ground").GetComponent<ParticleSystem>();

        m_Ray = transform.FindChild("GodRays").gameObject;

    }

    [ContextMenu("Get Object Around")]
    public void GetObjectsAround()
    {
        m_ObjectsAround = Physics.OverlapSphere(transform.position + m_vRangeOffset, m_fMaxRange, LayerMask.GetMask("Interaction", "EnvironmentalTransition")).OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).Where(x => x.GetComponent<IVeinInteract>() != null).ToList();
    }
    [ContextMenu("Reset Array")]
    public void ResetArray()
    {
        m_ObjectsAround.RemoveAll(x => x != null);
    }
    [ContextMenu("Find Materials")]
    public void FindMaterials()
    {
        m_EnvironmentalMaterials = Physics.OverlapSphere(transform.position + m_vRangeOffset, m_fMaxRange, LayerMask.GetMask("EnvironmentalTransition")).OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).Select(x => x.GetComponent<Renderer>()).ToList();
    }

    /// <summary>
    /// Interface function that it calls when the player uses the pulse and the object is inside the pulse range.
    /// </summary>
    /// <param name="pulsePower"></param>
    public void OnPulseEnter(float pulsePower)
    {

        if (!m_bIsActivate)
        {

            SaveManager.Instance.m_currentGame.m_LightVeinActivated.Add(gameObject.name);
            SaveManager.Instance.SaveGame();
            SoundManager.Instance.PlayEvent(m_Activation, this.gameObject);
            m_bIsActivate = true;
            if (m_bWarpToNextLevel)
            {

                StartCoroutine(NextLevel());

            }
            else
            {
                StartCoroutine(ActivateVein());
            }
            ParticleLight();

            if(m_Ray)
            {
                m_Ray.transform.DOScaleY(0f, 1f).OnComplete(()=>m_Ray.SetActive(false));
            }

        }
    }

    IEnumerator NextLevel()
    {
        SoundManager.Instance.StopAllEvents();
        if (m_gCameraEffectContainer && !m_MovieCinematic)
        {

            yield return ActivateVein();
            yield return new WaitUntil(() => !m_gCameraEffectContainer.activeInHierarchy);
        }else if(m_MovieCinematic)
        {
            yield return ActivateVein();
            yield return CinematicUI.Instance.OnPlaying(m_MovieCinematic,false);
        }
        else
        {
            yield return null;
        }

        string levelName = string.Empty;

        switch (m_sNextLevelName)
        {
            case "1":
                levelName = ProjectNames.LevelOne;
                break;
            case "2":
                levelName = ProjectNames.LevelTwo;
                break;
            case "3":
                levelName = ProjectNames.LevelThree;
                break;
            case "menu":
                levelName = ProjectNames.MainMenu;
                break;
            case "end":
                levelName = ProjectNames.EndScene;
                break;
        }
        if (levelName == ProjectNames.MainMenu || levelName == ProjectNames.EndScene)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ScreenManager.DestroySingleton();
            SceneManager.LoadScene(levelName);


        }
        else
        {

            SceneLoader.scene = levelName;
            SaveManager.Instance.m_currentGame.m_LightVeinActivated.RemoveAll(x => !string.IsNullOrEmpty(x));
            SaveManager.Instance.m_currentGame.m_LastCheckPoint = string.Empty;
            SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
        }

    }


    void ParticleLight()
    {
        Transform lightParticle = transform.Find("Light");
        if(lightParticle)
        {
            ParticleSystem light = lightParticle.GetComponent<ParticleSystem>();
            light.startLifetime = 5;
            light.Stop();
            light.Play();
        }
        Transform otherParticle = transform.Find("Particle System");
        if(otherParticle)
        {
            ParticleSystem particles = otherParticle.GetComponent<ParticleSystem>();
            particles.startLifetime = 2;
            particles.Stop();
            particles.Play();
        }
       
    }

    /// <summary>
    /// Activate the light vein
    /// </summary>
    /// <returns></returns>
    private IEnumerator ActivateVein()
    {
        GameManager.Instance.m_PlayerCamera.Reset(false, transform);
        if (m_gCameraEffectContainer)
        {
            m_gCameraEffectContainer.SetActive(true);
        }

        GameManager.Instance.SetLastCheckPoint(this);

        SoundManager.Instance.PlayEvent(SoundEvents.Play_Restoration_BG, this.gameObject);


        StartCoroutine(InteractWithObjects());

        m_OnInteraction.Invoke();
        while (m_fRange <= m_fMaxRange)
        {

            m_fRange += m_fIncrement;
            CircleEffect();
            InteractWithMaterials();



            yield return null;
        }
        SoundManager.Instance.PlayEvent(SoundEvents.Stop_Restoration_BG, this.gameObject);
        for (int i = 0; i < m_EnvironmentalMaterials.Count; i++)
        {
            MaterialPropertyBlock property = new MaterialPropertyBlock();
            property.SetFloat("_Show", 1);
            m_EnvironmentalMaterials[i].SetPropertyBlock(property);
        }
        CircleEffect(false);



    }

    /// <summary>
    /// Method for the load of the level
    /// </summary>
    public void Activate()
    {

        m_bIsActivate = true;
        m_fRange = m_fMaxRange;
        if (m_ObjectsAround == null || m_ObjectsAround.Count == 0)
        {
            StartCoroutine(InteractWithObjects());

        }
        m_OnInteraction.Invoke();
        StartCoroutine(InteractWithObjects());
        InteractWithMaterials();
        CircleEffect(false);
    }

    /// <summary>
    /// Send the range of the AoE and the position of the light vein to the material
    /// </summary>
    /// <param name="mat">material</param>
    private void InteractWithMaterial(Material mat, bool reset)
    {
        Vector4 position = new Vector4(transform.position.x + m_vRangeOffset.x, transform.position.y + m_vRangeOffset.y, transform.position.z + m_vRangeOffset.z, 0);
        if (mat != null)
        {
            if (reset)
            {
                mat.SetFloat("_Range", 0);
                mat.SetVector("_LightVein", position);
            }
            else
            {
                mat.SetFloat("_Range", m_fRange);
                mat.SetVector("_LightVein", position);
            }
        }


    }

    /// <summary>
    /// Interact with all individual material provide in the array.
    /// </summary>
    private void InteractWithMaterials(bool reset = false)
    {

        int index = 0;
        Func<Renderer, bool> CheckChanges = (x) =>
        {
            if (Vector3.Distance(x.transform.position + x.bounds.extents, transform.position) > m_fRange)
            {
                return true;
            }
            return false;
        };
        List<Renderer> newList = new List<Renderer>();
        while (index < m_EnvironmentalMaterials.Count)
        {
            MaterialPropertyBlock property = new MaterialPropertyBlock();

            property.SetVector("_LightVein", transform.position);
            property.SetFloat("_Range", m_fRange);

            if (!CheckChanges(m_EnvironmentalMaterials[index]))
            {

                property.SetFloat("_Show", 1);
            }
            else
            {
                newList.Add(m_EnvironmentalMaterials[index]);
            }

            m_EnvironmentalMaterials[index].SetPropertyBlock(property);
            index++;

        }
        m_EnvironmentalMaterials = newList;


    }

    /// <summary>
    /// Interact with objects that have the Interface
    /// </summary>
    /// <returns></returns>
    private IEnumerator InteractWithObjects()
    {
        int index = 0;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        while (m_ObjectsAround.Count > 0)
        {
            if (m_ObjectsAround[0] == null)
            {
                m_ObjectsAround.RemoveAt(0);
                continue;
            }
            IVeinInteract obj = m_ObjectsAround[0].GetComponent<IVeinInteract>();
            if (obj != null)
            {
                obj.OnLightVeinInteract(Vector3.Distance(m_ObjectsAround[0].transform.position, transform.position));
            }
            m_ObjectsAround.RemoveAt(0);
            index++;
            if (sw.ElapsedMilliseconds > 100f)
            {
                sw.Stop();
                sw.Reset();
                yield return null;
                sw.Start();
            }

        }
        sw.Stop();
    }

    /// <summary>
    /// Return the materials to the original state when the aplication quit
    /// </summary>
    public void OnApplicationQuit()
    {
        //InteractWithMaterials(true);
    }


    /// <summary>
    /// Makes the circle grow at the same time as the range.
    /// </summary>
    /// <param name="activate">if activate or not</param>
    private void CircleEffect(bool activate = true)
    {
        if (m_gCircleEffect == null)
        {
            return;
        }
        if (activate)
        {
            if (!m_gCircleEffect.activeInHierarchy)
            {
                m_gCircleEffect.SetActive(true);
            }
        }
        else
        {
            if (m_gCircleEffect.activeInHierarchy)
            {
                m_gCircleEffect.SetActive(false);
            }
        }

        m_gCircleEffect.transform.localScale = new Vector3(m_fRange * offset, m_fRange * offset, 1);


    }

    public void OnTriggerStay(Collider other)
    {
        PlayerContoller player = other.GetComponent<PlayerContoller>();
        if (player != null)
        {
            if (player.m_iHealth < player.m_iMaxHealth)
            {
                player.Heal(1);
            }
            if (player.m_iEnergy < player.m_iMaxEnergy)
            {
                player.m_iEnergy++;
            }

        }
    }

    public void OnTriggerExit(Collider other)
    {
        PlayerContoller player = other.GetComponent<PlayerContoller>();
        if (player != null)
        {
            if(m_GroundParticle)
            {
                m_GroundParticle.startLifetime = 2.59f;
                m_GroundParticle.startColor = Color.white;
            }
        }

    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerContoller player = other.GetComponent<PlayerContoller>();
        if (player != null)
        {
            if (m_GroundParticle)
            {
                m_GroundParticle.startLifetime =5f;
                m_GroundParticle.startColor = Color.yellow;
            }
        }
    }





    /// <summary>
    /// Draw the range in the editor
    /// </summary>
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.position + m_vRangeOffset, m_fRange);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + m_vRangeOffset, m_fMaxRange);
        }

    }

    public void OnPulseExit()
    {

    }

    void OnDestroy()
    {

#if !UNITY_EDITOR
       if(Application.isPlaying)
        {
            SoundManager.Instance.PlayEvent(m_StopEmitter, this.gameObject);
        }
#endif


    }
    public void SetMusicState(string state)
    {
        object check = System.Enum.Parse(typeof(MusicState), state);

        SoundManager.Instance.SetState((MusicState)check);
    }

}
