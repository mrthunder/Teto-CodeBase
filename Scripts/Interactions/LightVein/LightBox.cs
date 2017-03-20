using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider),typeof(AkEnvironment))]
public class LightBox : MonoBehaviour
{

    private BoxCollider _collider;
    internal BoxCollider m_BoxCollider
    {
        get
        {
            if (_collider == null)
            {
                _collider = GetComponent<BoxCollider>();
                _collider.isTrigger = true;
            }
            return _collider;
        }
    }


    public GameObject m_Teto;

    /// <summary>
    /// True = Dark and False = Light
    /// </summary>
    private bool m_bWorldState = true;

    private AkEnvironment env;

    public bool m_b2DReverb = false;
    void Start()
    {
       env = GetComponent<AkEnvironment>();
       if(!env)
        {
            env = this.gameObject.AddComponent<AkEnvironment>();
            GetComponent<Rigidbody>().isKinematic = true;
        }
       SetReverb();

    }

    /// <summary>
    /// Change the environmental music
    /// </summary>
    /// <param name="dark">Change to dark world or light</param>
    void ChangeWorld(bool dark = true)
    {
        SoundManager.Instance.SetGameParameter(GameParameters.BG_Type, (dark ? 0 : 1));
        MusicState musicState = MusicState.Idle;
        if (!dark)
        {
            musicState = MusicState.Light;

            GameObject from = SoundManager.Instance.m_StalkBGOwner;
            if(from!=null)
            {
                SoundManager.Instance.PlayEvent(SoundEvents.Stop_Stalk_BG, from);
            }
            
        }
        SoundManager.Instance.SetState(musicState,true);
         SetReverb();
    }

    /// <summary>
    /// Get all enemies in the area
    /// </summary>
    void GetAllEnemies()
    {
        Collider[] enemies = Physics.OverlapBox(m_BoxCollider.center + transform.position, m_BoxCollider.size / 2, transform.rotation, LayerMask.GetMask("Enemies"), QueryTriggerInteraction.UseGlobal);
        foreach (Collider enemy in enemies)
        {
            EnemyEffect(enemy);
        }
    }

    /// <summary>
    /// Destroy them all
    /// </summary>
    /// <param name="col">Enemy</param>
    void EnemyEffect(Collider col)
    {
        AiController enemy = col.GetComponent<AiController>();
        
        if (enemy == null)
        {
            Spawner spawner = col.GetComponent<Spawner>();
            if (spawner == null)
            {

                Destroy(col.gameObject);
            }
            else
            {
                spawner.Die();
            }
        }
        else
        {
            enemy.Die();
        }
    }

    /// <summary>
    /// One single function activate in the light vein
    /// </summary>
    public void LightVeinEffect()
    {
        if (!m_bWorldState) return;
        m_bWorldState = false;
        ChangeWorld(m_bWorldState);
        GetAllEnemies();
    }

    public void SetReverb(bool enter = true)
    {
        BusName reverb = BusName.Dark_Rvrb;
        if (!m_bWorldState) reverb = BusName.Light_Rvrb;
        if (m_b2DReverb && enter) reverb = BusName.Temple_Rvrb;
        SoundManager.Instance.ChangeBus(reverb, ref env);

    }

    public void OnTriggerEnter(Collider other)
    {
        ChangeWorld(m_bWorldState);
       
    }

    public void OnTriggerExit(Collider other)
    {
        ChangeWorld();
        
    }




#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(m_BoxCollider.center + transform.position, m_BoxCollider.size);
    }
#endif

}
