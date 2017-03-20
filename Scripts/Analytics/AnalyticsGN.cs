using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;

public class AnalyticsGN : MonoBehaviour
{
    public string m_Filename = "AnalyticsGN.json";
    private Unit m_Player;

    private List<GNAnalyticsData> analyticsData = new List<GNAnalyticsData>();
    private int m_id = 0;

    private Vector4[] m_LocHeatMap;
    private Vector4[] m_FPSHeatMap;
    private Vector4[] m_HealthHeatMap;
    private Vector4[] m_EnergyHeatMap;

    private Vector3 m_PreviousLocation;
    private float m_GridGran;

    public float m_fCaptureInterval = 0.25f;

    public bool UseCustomGraident = false;
    public Gradient heatGraident;
    public Color LowestColorValue = Color.cyan;
    public Color LowColorValue = Color.blue;
    public Color MidColorValue = Color.green;
    public Color HighColorValue = Color.yellow;
    public Color HighestColorValue = Color.red;

    // Normized stuff
    private float m_MinValue = 0.01f;
    private float m_fLocMaxValue = 0.01f;
    private float m_fFPSMaxValue = 0.01f;
    private float m_fHealthMaxValue = 0.01f;
    private float m_fEnergyMaxValue = 0.01f;
    private bool m_bShowLog10 = false;

    // OnDrawGizmos Draw Mode
    private int drawmode = 0;

    // Use this for initialization
    void Start()
    {
    m_Player = GameManager.Instance.m_Player;
        m_PreviousLocation = Vector3.zero;
        StartCoroutine(TrackPlayer());

        m_fHealthMaxValue = m_Player.m_iHealth;
        m_fEnergyMaxValue = m_Player.m_iEnergy;

        // Heatmap Graident
        heatGraident = new Gradient();

        if (!UseCustomGraident)
        {
            GradientColorKey[] gck = new GradientColorKey[5];
            gck[0].color = LowestColorValue;
            gck[0].time = 0.01f;
            gck[1].color = LowColorValue;
            gck[1].time = 0.25f;
            gck[2].color = MidColorValue;
            gck[2].time = 0.50f;
            gck[3].color = HighColorValue;
            gck[3].time = 0.75f;
            gck[3].color = HighestColorValue;
            gck[3].time = 0.90f;

            GradientAlphaKey[] gak = new GradientAlphaKey[5];
            gak[0].alpha = 1.0f;
            gak[0].time = 1.0f;
            gak[1].alpha = 1.0f;
            gak[1].time = 1.0f;
            gak[2].alpha = 1.0f;
            gak[2].time = 1.0f;
            gak[3].alpha = 1.0f;
            gak[3].time = 1.0f;
            gak[4].alpha = 1.0f;
            gak[4].time = 1.0f;

            heatGraident.SetKeys(gck, gak);
        }

        if (m_MinValue > 1f)
            m_MinValue = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        // Sets Up Granularity based on input interval
        m_GridGran = (m_Player.m_fUnitSpeed * m_fCaptureInterval) / 2;

        // Clears All Data
        if (Input.GetKeyDown("0"))
        {
            analyticsData.Clear();
            ProcessData();
            drawmode = 0;
        }

        // Draw Location Heat Map
        if (Input.GetKeyDown("1"))
        {
            ProcessData();
            drawmode = 1;
        }

        // Draw FPS HeatMap
        if (Input.GetKeyDown("2"))
        {
            ProcessData();
            drawmode = 2;
        }

        //Draw Health Map
        if (Input.GetKeyDown("3"))
        {
            ProcessData();
            drawmode = 3;
        }

        //Draw Energy Map
        if (Input.GetKeyDown("4"))
        {
            ProcessData();
            drawmode = 4;
        }

        // Save Json File
        if (Input.GetKeyDown("5"))
        {
            print("Saving Json File to: " + Application.persistentDataPath + "/" + m_Filename);
            drawmode = 0;
            MakeJsonFile();
        }

        // Load Json File
        if (Input.GetKeyDown("6"))
        {
            print("Loading Json File from: " + Application.persistentDataPath + "/" + m_Filename);
            drawmode = 0;
            LoadJsonFile();
        }

        // Append From Json File
        if (Input.GetKeyDown("7"))
        {
            print("Append From Json File: " + Application.persistentDataPath + "/" + m_Filename);
            drawmode = 0;
            AppendFromJsonFile();
        }

        // Append To Json File
        if (Input.GetKeyDown("8"))
        {
            print("Append To Json File: " + Application.persistentDataPath + "/" + m_Filename);
            drawmode = 0;
            AppendToJsonFile();
        }

        // Switches between Normalised (Default) and Log10
        if (Input.GetKeyDown("9"))
        {
            m_bShowLog10 = !m_bShowLog10;
        }
    }

    // Tracks the Player based on the capture interval
    private IEnumerator TrackPlayer()
    {
        Vector3 roundedDown;
        Vector3 playerPos;

        while (Application.isPlaying)
        {
            playerPos = m_Player.transform.position;
            roundedDown = new Vector3(Mathf.RoundToInt(playerPos.x), Mathf.RoundToInt(playerPos.y), Mathf.RoundToInt(playerPos.z));
            analyticsData.Add(new GNAnalyticsData(m_id, roundedDown, m_Player.m_iHealth, m_Player.m_iEnergy, 1 / Time.deltaTime));
            m_id++;
            yield return new WaitForSeconds(m_fCaptureInterval);
        }
    }

    // Builds visual feedback (including Normalization)
    private void ProcessData()
    {
        List<GNAnalyticsData> distinctList = analyticsData.GroupBy(x => x.m_Location).Select(y => y.FirstOrDefault()).ToList();

        List<Vector4> locHeatMap = new List<Vector4>();
        List<Vector4> fpsHeatMap = new List<Vector4>();
        List<Vector4> healthHeatMap = new List<Vector4>();
        List<Vector4> energyHeatMap = new List<Vector4>();

        Vector3 itemToCompare;

        Vector4 heatOnLocation;
        Vector4 fpsOnLocation;
        Vector4 healthOnLocation;
        Vector4 energyOnLocation;

        float fpslowValue;
        float healthlowValue;
        float energylowValue;

        for (int a = 0; a < distinctList.Count; a++)
        {
            itemToCompare = distinctList.ElementAt(a).m_Location;

            heatOnLocation = Vector4.zero;
            fpsOnLocation = Vector4.zero;
            healthOnLocation = Vector4.zero;
            energyOnLocation = Vector4.zero;

            fpslowValue = 0;
            healthlowValue = 0;
            energylowValue = 0;

            for (int b = a + 1; b < analyticsData.Count; b++)
            {
                // Location Heat Mapping
                if (itemToCompare == analyticsData.ElementAt(b).m_Location)
                {

                    // Location HeatMap Data
                    heatOnLocation = new Vector4(itemToCompare.x, itemToCompare.y, itemToCompare.z, heatOnLocation.w + 1);
                    if (heatOnLocation.w > m_fLocMaxValue)
                    {
                        m_fLocMaxValue = heatOnLocation.w;
                    }

                    // FPS Heat Data
                    fpsOnLocation = new Vector4(itemToCompare.x, itemToCompare.y, itemToCompare.z, analyticsData.ElementAt(b).m_fFps);
                    if (fpsOnLocation.w > m_fFPSMaxValue)
                    {
                        m_fFPSMaxValue = analyticsData.ElementAt(b).m_fFps;
                    }

                    if (fpslowValue == 0 || fpsOnLocation.w < fpslowValue)
                    {
                        fpslowValue = fpslowValue = fpsOnLocation.w;
                    }

                    // Health Data
                    healthOnLocation = new Vector4(itemToCompare.x, itemToCompare.y, itemToCompare.z, analyticsData.ElementAt(b).m_fHealth);
                    if (healthlowValue == 0 || healthOnLocation.w < healthlowValue)
                    {
                        healthlowValue = healthlowValue = healthOnLocation.w;
                    }

                    // Energy Data
                    energyOnLocation = new Vector4(itemToCompare.x, itemToCompare.y, itemToCompare.z, analyticsData.ElementAt(b).m_fEnergy);
                    if (energylowValue == 0 || energyOnLocation.w < energylowValue)
                    {
                        energylowValue = energylowValue = energyOnLocation.w;
                    }

                }
            }
            locHeatMap.Add(heatOnLocation);
            fpsHeatMap.Add(fpsOnLocation);
            healthHeatMap.Add(healthOnLocation);
            energyHeatMap.Add(energyOnLocation);
        }
        m_LocHeatMap = locHeatMap.ToArray();
        m_FPSHeatMap = fpsHeatMap.ToArray();
        m_HealthHeatMap = healthHeatMap.ToArray();
        m_EnergyHeatMap = energyHeatMap.ToArray();
    }

    // Json Loading Array Wrapper
    public static T[] FromJson<T>(string json)
    {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    // Json Saving Array Wrapper
    public static string ToJson<T>(T[] array)
    {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    // JSON Array Wapper Helper
    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }

    // Saves Json file
    private void MakeJsonFile()
    {
        StreamWriter jsonStream = new StreamWriter(Application.persistentDataPath + "/" + m_Filename);
        string jsonData = ToJson<GNAnalyticsData>(analyticsData.ToArray());
        jsonStream.Write(jsonData);
        jsonStream.Close();
    }

    // Loads Json File
    private void LoadJsonFile()
    {
        StreamReader jsonStream = new StreamReader(Application.persistentDataPath + "/" + m_Filename);
        string jsonData = jsonStream.ReadToEnd();
        jsonStream.Close();
        analyticsData = FromJson<GNAnalyticsData>(jsonData).ToList();
    }

    // Appends From Json File
    private void AppendFromJsonFile()
    {
        List<GNAnalyticsData> currentAnalyticsData = analyticsData;
        LoadJsonFile();
        analyticsData.AddRange(currentAnalyticsData);
    }

    // Appends to Json File
    private void AppendToJsonFile()
    {
        List<GNAnalyticsData> currentAnalyticsData = analyticsData;
        LoadJsonFile();
        analyticsData.AddRange(currentAnalyticsData);
        MakeJsonFile();
    }

    // MUST be used in OnDrawGizmos()
    private void DrawPath()
    {
        foreach (GNAnalyticsData data in analyticsData)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(m_PreviousLocation, data.m_Location);
            m_PreviousLocation = data.m_Location;

            if (data.PlayerDied())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(data.m_Location, 1.5f);
            }
        }
        if (analyticsData.Count > 0)
        {
            m_PreviousLocation = analyticsData[0].m_Location;
        }
    }

    // MUST be used in OnDrawGizmos()
    private void DrawHeatMap(Vector4[] heatMap, float maxValue)
    {
        foreach (Vector4 data in heatMap)
        {
            float normalised = Normaliser(data.w, maxValue);

            Gizmos.color = heatGraident.Evaluate(normalised);
            Gizmos.DrawCube(data, new Vector3(m_GridGran, m_GridGran, m_GridGran));
        }
    }

    // MUST be used in OnDrawGizmos()
    private void DrawHeatChanges(Vector4[] heatMap, float maxValue)
    {
        foreach (Vector4 data in heatMap)
        {
            float normalised = Normaliser(data.w, maxValue);

            Gizmos.color = heatGraident.Evaluate(normalised);

            if (data.w < maxValue)
            {
                Gizmos.DrawCube(data, new Vector3(m_GridGran, m_GridGran, m_GridGran));
            }
        }
    }

    // Liner and Log10 Normalization
    private float Normaliser(float heatData, float maxValue)
    {
        float normalised;
        if (m_bShowLog10)
            normalised = Mathf.Log10((heatData - m_MinValue) / (maxValue - m_MinValue));
        else
            normalised = (heatData - m_MinValue) / (maxValue - m_MinValue);

        return normalised;
    }

    public void OnDrawGizmos()
    {
        DrawPath();

        switch (drawmode)
        {
            case 1:
                DrawHeatMap(m_LocHeatMap, m_fLocMaxValue);
                break;
            case 2:
                DrawHeatMap(m_FPSHeatMap, m_fFPSMaxValue);
                break;
            case 3:
                DrawHeatChanges(m_HealthHeatMap, m_fHealthMaxValue);
                break;
            case 4:
                DrawHeatChanges(m_EnergyHeatMap, m_fEnergyMaxValue);
                break;
            default:
                break;
        }      
    }
}
