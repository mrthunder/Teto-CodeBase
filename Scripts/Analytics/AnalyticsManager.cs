using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// PG07 Lucas Goes 
/// </summary>

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    /// <summary>
    /// Object that will store the data
    /// </summary>
    AnalyticsObject m_AO;

    /// <summary>
    /// Gradient to display
    /// </summary>
    public Gradient m_GradientData = new Gradient();

    /// <summary>
    /// Gradient to display
    /// </summary>
    public Gradient m_GradientPulse = new Gradient();

    /// <summary>
    /// Gradient to display
    /// </summary>
    public Gradient m_GradientDoppler = new Gradient();

    /// <summary>
    /// Gradient to display
    /// </summary>
    public Gradient m_GradientDash = new Gradient();

    ///<summary>
    /// Player transform to get the players position
    ///</summary>
    Transform m_Player;

    [Range(50, 500)]
    ///<summary>
    ///Size of the grid in the x axis. Can go from 50 to 500 elements
    ///</summary>
    public int m_iGridSizeX = 100;

    [Range(50, 500)]
    ///<summary>
    ///Size of the grid in the z axis. Can go from 50 to 500 elements
    ///</summary>
    public int m_iGridSizeZ = 100;

    ///<summary>
    ///How big is the grid
    ///</summary>
    public int m_iGridGran = 1;

    ///<summary>
    ///Index in the grid based on the player position
    ///</summary>
    private int m_iXIndex, m_iZIndex;

    [Range(1f, 100f)]
    ///<summary>
    /// Max Height of the cubes in the Gizmos
    ///</summary>
    public float m_fScaleHeight = 4.0f;

    /// <summary>
    /// Draw gizmos or not
    /// </summary>
    public bool displayAnalytics = false;

    /// <summary>
    /// Index of the data to be display
    /// </summary>
    private int m_iDataIndex = 0;

    /// <summary>
    /// Index of the scale type Log or normalized ( linear)
    /// </summary>
    private int m_iScaleIndex = 0;

    /// <summary>
    /// If the dpad was press or not
    /// </summary>
    private bool m_bPress = false;


    // Use this for initialization
    void Start()
    {
        m_AO = AnalyticsObject.LoadData(DataType.JSON, 0);
        if (m_AO == null)
        {
            m_AO = new AnalyticsObject(m_iGridSizeX, m_iGridSizeZ);
            m_AO.levelName = SceneManager.GetActiveScene().name;
            
        }
        
       
        m_Player = FindObjectOfType<PlayerContoller>().transform;
        //m_AO = AnalyticsObject.LoadData(DataType.JSON, m_AO.gameId);
        print(Application.persistentDataPath);
    }

    void Update()
    {
        //if(Input.GetButtonDown("Emoticons"))
        //{
        //    m_AO = AnalyticsObject.LoadData(DataType.JSON, 0);
        //}
        if (Input.GetButtonDown("Start"))
        {
            m_iScaleIndex = (m_iScaleIndex + 1) % 2;
        }
        if (Input.GetAxisRaw("D-PADVertical") == 1 && !m_bPress)
        {
            m_bPress = true;
            m_iDataIndex = (m_iDataIndex + 1) % System.Enum.GetValues(typeof(DataDraw)).Length;
        }
        if (Input.GetAxisRaw("D-PADVertical") == 0 && m_bPress)
        {
            m_bPress = false;
        }
        GetPosition();
        SetPositionData();
    }
   

    private void GetPosition()
    {
        if (m_Player == null) return;
        m_iXIndex = (int)((m_Player.position.x - transform.position.x) + (float)(m_iGridGran / 2f)) / m_iGridGran;
        m_iZIndex = (int)((m_Player.position.z - transform.position.z) + (float)(m_iGridGran / 2f)) / m_iGridGran;
    }

    /// <summary>
    /// Check if the index is inside the array
    /// </summary>
    /// <returns>True - inside the range. False - out of range</returns>
    private bool CheckIndex()
    {
        System.Func<int, int, bool> check = (x, y) => (x > 0 && x < y);

        if (check(m_iXIndex, m_iGridSizeX) && check(m_iZIndex, m_iGridSizeZ))
        {
            return true;
        }
        return false;
    }

    void SetPositionData()
    {
        if (!CheckIndex()) return;
        Data data = new Data(0.1f, new Vector2(m_iXIndex, m_iZIndex));
        m_AO.SetData(data, m_iXIndex, m_iZIndex);
    }

    public void SetPulseData()
    {
        if (!CheckIndex()) return;
        Data data = new Data(0.1f, 0, 0);
        m_AO.SetData(data, m_iXIndex, m_iZIndex);
    }

    public void SetDashData()
    {
        if (!CheckIndex()) return;
        Data data = new Data(0, 0.1f, 0);
        m_AO.SetData(data, m_iXIndex, m_iZIndex);
    }

    public void SetDopplerData()
    {
        if (!CheckIndex()) return;
        Data data = new Data(0, 0, 0.1f);
        m_AO.SetData(data, m_iXIndex, m_iZIndex);
    }

    public void OnDrawGizmos()
    {
        if(displayAnalytics)
        {
            if(!Application.isPlaying)
            {
                Gizmos.DrawWireCube(transform.position + new Vector3((m_iGridSizeX * m_iGridGran) / 2, 0, (m_iGridSizeZ * m_iGridGran) / 2), new Vector3(m_iGridSizeX*m_iGridGran, 1, m_iGridSizeZ * m_iGridGran));
            }
            if(m_AO != null)
            {
                
                m_AO.DrawData((DataDraw)m_iDataIndex, m_GradientData, transform,m_iScaleIndex,m_fScaleHeight,m_iGridGran);
            }
            
        }
        
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_AO.SaveData(DataType.JSON);
    }
}
